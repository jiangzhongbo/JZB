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
    private Dictionary<_Coroutine, _Coroutine> child_parent = new Dictionary<_Coroutine, _Coroutine>();
    private List<Action<ICoroutinePool, _Coroutine, object>> _filters = new List<Action<ICoroutinePool, _Coroutine, object>>();
    private Dictionary<Coroutine, _Coroutine> outer_inter = new Dictionary<Coroutine, _Coroutine>();
    private Dictionary<_Coroutine, Coroutine> inter_outer = new Dictionary<_Coroutine, Coroutine>();
    private _Coroutine co_current = null;

    void Awake()
    {
        filters();
    }

    private void filter(Action<ICoroutinePool, _Coroutine, object> fn)
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
                    co.State = CoroutineState.Normal;
                    pool.Remove(co);
                    var _co = create(c as IEnumerator);
                    _co.State = CoroutineState.Suspend;
                    pool.Add(_co);
                    child_parent[_co] = co;
                }
            }
        );
        filter(
            (pool, co, c) =>
            {
                if (c is _Coroutine)
                {
                    co.State = CoroutineState.Normal;
                    pool.Remove(co);
                    var _co = c as _Coroutine;
                    _co.State = CoroutineState.Suspend;
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
                    co.State = CoroutineState.Normal;
                    pool.Remove(co);
                    var _co = outer_inter[(Coroutine)c];
                    _co.State = CoroutineState.Suspend;
                    pool.Add(_co);
                    child_parent[_co] = co;
                }
            }
        );
    }

    private _Coroutine create(IEnumerator ie, RunType type = RunType.Update)
    {
        var _co = new _Coroutine(ie, type);
        return _co; 
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

    private void handle_coroutine(_Coroutine c)
    {
        co_current = c;
        c.State = CoroutineState.Running;
        bool ok = c.IE.MoveNext();
        c.State = CoroutineState.Suspend;
        co_current = null;
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
            if (child_parent.ContainsKey(c))
            {
                var p = child_parent[c];
                pool.Add(p);
                child_parent.Remove(c);
            }
            c.State = CoroutineState.Dead;
            pool.Remove(c);
            if (inter_outer.ContainsKey(c))
            {
                var out_c = inter_outer[c];
                out_c.CallThen();
                inter_outer.Remove(c);
                outer_inter.Remove(out_c);
            }
        }
    }
}
