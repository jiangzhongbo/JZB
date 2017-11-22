using System.Collections;
using System.Collections.Generic;
using System;
public partial class Co
{
    private interface ICoroutinePool
    {
        List<Coroutine> Get();
        List<Coroutine> GetByType(RunType type);
        void Remove(Coroutine ie);
        void Add(Coroutine ie);
    }

    private class SequencePool : ICoroutinePool
    {
        private Queue<Coroutine> queue = new Queue<Coroutine>();
        private List<Coroutine> _ret = new List<Coroutine>(1);

        public void Add(Coroutine ie)
        {
            queue.Enqueue(ie);
        }

        public List<Coroutine> Get()
        {
            _ret.Clear();
            _ret.Add(queue.Peek());
            return _ret;
        }

        public List<Coroutine> GetByType(RunType type)
        {
            _ret.Clear();
            if (queue.Peek().Type == type)
            {
                _ret.Add(queue.Peek());
            }
            return _ret;
        }

        public void Remove(Coroutine ie)
        {
            if (queue.Peek() == ie)
            {
                queue.Dequeue();
            }
        }
    }

    private class ConcurrentPool : ICoroutinePool
    {
        private List<Coroutine> cs = new List<Coroutine>();
        private List<Coroutine> _cs = new List<Coroutine>();


        public List<Coroutine> Get()
        {
            _cs.Clear();
            _cs.AddRange(cs);
            return _cs;
        }

        public void Remove(Coroutine ie)
        {
            cs.Remove(ie);
        }

        public void Add(Coroutine ie)
        {
            cs.Add(ie);
        }

        public List<Coroutine> GetByType(RunType type)
        {
            _cs.Clear();
            for (int i = 0; i < _cs.Count; i++)
            {
                if (cs[i].Type == type)
                {
                    _cs.Add(cs[i]);
                }
            }
            return _cs;
        }
    }
    public enum RunType
    {
        Update,
        LateUpdate,
        FixedUpdate,
    }

    public enum PoolType
    {
        Sequence,
        Concurrent
    }

    public enum CoroutineState
    {
        Suspend,
        Running,
        Dead,
        Normal
    }

    public sealed partial class Coroutine
    {
        private IEnumerator ie;
        private RunType type;
        private CoroutineState state;
        private List<Action<object>> onFinishs = new List<Action<object>>(1);
        public RunType Type
        {
            get
            {
                return type;
            }
        }

        public CoroutineState State
        {
            get
            {
                return state;
            }
        }

        public Coroutine(IEnumerator ie, RunType type,
            out Action<RunType> setRunType,
            out Action<CoroutineState> setCoroutineState,
            out Func<List<Action<object>>> getOnFinishs
            )
        {
            this.ie = ie;
            this.type = type;
            setRunType = this.setRunType;
            setCoroutineState = this.setCoroutineState;
            getOnFinishs = this.getOnFinishs;
        }

        public Coroutine Then(Action<object> onFinish)
        {
            this.onFinishs.Add(onFinish);
            return this;
        }

        private List<Action<object>> getOnFinishs()
        {
            return onFinishs;
        }

        private void setRunType(RunType t)
        {
            type = t;
        }

        private void setCoroutineState(CoroutineState s)
        {
            state = s;
        }

    }

    private class CoroutineInterHandler
    {
        public IEnumerator IE;
        public Action<RunType> SetRunType;
        public Action<CoroutineState> SetCoroutineState;
        public Func<List<Action<object>>> GetOnFinishs;
        public object Current;
    }
}
