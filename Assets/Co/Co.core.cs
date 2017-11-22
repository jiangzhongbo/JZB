using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UPromise;
public sealed partial class Co : MonoBehaviour
{
    private static Action noop = () => { };
    private PoolType poolType = PoolType.Concurrent;
    private ICoroutinePool pool = new ConcurrentPool();
    private Dictionary<Coroutine, Coroutine> child_parent = new Dictionary<Coroutine, Coroutine>();
    private Dictionary<Coroutine, CoroutineInterHandler> co_internal = new Dictionary<Coroutine, CoroutineInterHandler>();
    private List<Action<ICoroutinePool, Coroutine, object>> _filters = new List<Action<ICoroutinePool, Coroutine, object>>();
    private Coroutine co_current = null;

    void Awake()
    {
        filters();
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
                    co_internal[co].SetCoroutineState(CoroutineState.Normal);
                    pool.Remove(co);
                    var _co = create(c as IEnumerator);
                    co_internal[co].SetCoroutineState(CoroutineState.Suspend);
                    pool.Add(_co);
                    child_parent[_co] = co;
                }
            }
        );
        filter(
            (pool, co, c) =>
            {
                if (c is Coroutine)
                {
                    co_internal[co].SetCoroutineState(CoroutineState.Normal);
                    pool.Remove(co);
                    var _co = c as Coroutine;
                    co_internal[co].SetCoroutineState(CoroutineState.Suspend);
                    pool.Add(_co);
                    child_parent[_co] = co;
                }
            }
        );
    }

    private Coroutine create(IEnumerator ie, RunType type = RunType.Update)
    {
        var hander = new CoroutineInterHandler();
        hander.IE = ie;
        var co = new Coroutine(ie, type, out hander.SetRunType, out hander.SetCoroutineState, out hander.GetOnFinishs);
        co_internal[co] = hander;
        return co; 
    }

    private void Update()
    {
        handle(RunType.Update);
    }

    private void FixedUpdate()
    {
        handle(RunType.FixedUpdate);
    }

    private void LateUpdate()
    {
        handle(RunType.LateUpdate);
    }

    private void handle(RunType type)
    {
        var cos = pool.GetByType(type);
        if (cos.Count > 0)
        {
            for(int i = 0; i < cos.Count; i++)
            {
                var c = cos[i];
                handle_coroutine(c);
            }
        }
    }

    private void handle_coroutine(Coroutine c)
    {
        co_current = c;
        co_internal[c].SetCoroutineState(CoroutineState.Running);
        bool ok = co_internal[c].IE.MoveNext();
        co_internal[c].SetCoroutineState(CoroutineState.Suspend);
        co_current = null;
        if (ok)
        {
            var current = co_internal[c].IE.Current;
            co_internal[c].Current = current;
            for (int i = 0; i < _filters.Count; i++)
            {
                _filters[i](pool, c, current);
            }
        }
        else
        {
            if (child_parent.ContainsKey(c))
            {
                var p = child_parent[c];
                pool.Add(p);
                child_parent.Remove(c);
            }
            co_internal[c].SetCoroutineState(CoroutineState.Dead);
            pool.Remove(c);
            var thens = co_internal[c].GetOnFinishs();
            for(int i = 0; i < thens.Count; i++)
            {
                thens[i]?.Invoke(co_internal[c].Current);
            }
            co_internal.Remove(c);
        }
    }
}
