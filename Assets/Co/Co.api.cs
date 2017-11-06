using UnityEngine;
using System.Collections;

public partial class Co
{
    public void With(PoolType t)
    {
        poolType = t;
        if (t == PoolType.Sequence)
        {
            pool = new SequencePool();
        }
        else if (t == PoolType.Concurrent)
        {
            pool = new ConcurrentPool();
        }
    }

    public PoolType GetPoolType()
    {
        return poolType;
    }

    public Coroutine Run(Coroutine co)
    {
        if (outer_inter.ContainsKey(co))
        {
            pool.Add(outer_inter[co]);
        }
        return co;
    }

    public Coroutine Run(IEnumerator ie, RunType type = RunType.Update)
    {
        return Run(Create(ie, type));
    }

    public CoroutineState Status(Coroutine co)
    {
        if (outer_inter.ContainsKey(co))
        {
            return outer_inter[co].State;
        }
        return CoroutineState.Dead;
    }

    public Coroutine Create(IEnumerator ie, RunType type = RunType.Update)
    {
        Coroutine c = new Coroutine();
        _Coroutine _c = create(ie, type);
        outer_inter[c] = _c;
        inter_outer[_c] = c;
        return c;
    }

    public Coroutine Running()
    {
        if(co_current == null || !inter_outer.ContainsKey(co_current))
        {
            return null;
        }
        return inter_outer[co_current];
    }
}
