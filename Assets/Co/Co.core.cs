using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UPromise;
public sealed partial class Co : MonoBehaviour
{
    private PoolType poolType = PoolType.Concurrent;
    private ICoroutinePool pool = new ConcurrentPool();
    private Dictionary<IEnumerator, Coroutine> childie_parentco = new Dictionary<IEnumerator, Coroutine>();
    private List<Action<ICoroutinePool, Coroutine, object>> _filters = new List<Action<ICoroutinePool, Coroutine, object>>();

    void Awake()
    {
        filters();
    }

    public void With(PoolType t)
    {
        if(t == PoolType.Sequence)
        {
            pool = new SequencePool();
        }
        else if (t == PoolType.Concurrent)
        {
            pool = new ConcurrentPool();
        }
    }

    private void filter(Action<ICoroutinePool, Coroutine, object> fn)
    {
        _filters.Add(fn);
    }

    private void filters()
    {
        _filters.Clear();
        filterCore();
        filterPromise();
        filterExtensions();
    }
    private void filterCore()
    {
        filter(
            (pool, co, c) =>
            {
                if (c is IEnumerator)
                {
                    pool.Remove(co);
                    var _co = Create(c as IEnumerator);
                    pool.Add(_co);
                    childie_parentco[_co.IE] = co;
                }
            }
        );
        filter(
            (pool, co, c) =>
            {
                if (c is Coroutine)
                {
                    pool.Remove(co);
                    var _co = c as Coroutine;
                    pool.Add(_co);
                    childie_parentco[_co.IE] = co;
                }
            }
        );
    }

    public CoroutineState Status(Coroutine c)
    {
        return c.State;
    }

    public Coroutine Create(IEnumerator ie, RunType type = RunType.Update)
    {
        return new Coroutine(ie, type); 
    }

    public void Run(Coroutine c)
    {
        pool.Add(c);
    }

    public void Run(IEnumerator ie, RunType type = RunType.Update)
    {
        pool.Add(Create(ie, type));
    }

    private void Update()
    {
        handle(pool, RunType.Update);
    }

    private void FixedUpdate()
    {
        handle(pool, RunType.FixedUpdate);
    }

    private void LateUpdate()
    {
        handle(pool, RunType.LateUpdate);
    }

    private void handle(ICoroutinePool pool, RunType type)
    {
        var cos = pool.GetByType(type);
        if (cos.Count > 0)
        {
            for(int i = 0; i < cos.Count; i++)
            {
                var c = cos[i];
                handle_coroutine(c, pool);
            }
        }
    }

    private void handle_coroutine(Coroutine c, ICoroutinePool pool)
    {
        bool ok = c.IE.MoveNext();
        if (ok)
        {
            var current = c.IE.Current;
            for (int i = 0; i < _filters.Count; i++)
            {
                _filters[i](pool, c, current);
            }
        }
        else
        {
            if (childie_parentco.ContainsKey(c.IE))
            {
                pool.Add(childie_parentco[c.IE]);
                childie_parentco.Remove(c.IE);
            }
            pool.Remove(c);
        }
    }
}
