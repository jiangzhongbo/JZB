using UnityEngine;
using System.Collections;

public partial class Co
{
    public void WithPool(PoolType t)
    {
        if (t == poolType) return;
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
        pool.Add(co);
        return co;
    }

    public Coroutine Run(IEnumerator ie, RunType type = RunType.Update)
    {
        return Run(Create(ie, type));
    }

    public Coroutine Create(IEnumerator ie, RunType type = RunType.Update)
    {
        Coroutine _c = create(ie, type);
        return _c;
    }

    public Coroutine Running()
    {
        return co_current;
    }
}
