using System.Collections;
using System.Collections.Generic;
using System;
public partial class Co
{
    private interface ICoroutinePool
    {
        List<_Coroutine> Get();
        List<_Coroutine> GetByType(RunType type);
        void Remove(_Coroutine ie);
        void Add(_Coroutine ie);
    }

    private class SequencePool : ICoroutinePool
    {
        private Queue<_Coroutine> queue = new Queue<_Coroutine>();
        private List<_Coroutine> _ret = new List<_Coroutine>(1);

        public void Add(_Coroutine ie)
        {
            queue.Enqueue(ie);
        }

        public List<_Coroutine> Get()
        {
            _ret.Clear();
            _ret.Add(queue.Peek());
            return _ret;
        }

        public List<_Coroutine> GetByType(RunType type)
        {
            _ret.Clear();
            if (queue.Peek().Type == type)
            {
                _ret.Add(queue.Peek());
            }
            return _ret;
        }

        public void Remove(_Coroutine ie)
        {
            if (queue.Peek() == ie)
            {
                queue.Dequeue();
            }
        }
    }

    private class ConcurrentPool : ICoroutinePool
    {
        private List<_Coroutine> cs = new List<_Coroutine>();
        private List<_Coroutine> _cs = new List<_Coroutine>();


        public List<_Coroutine> Get()
        {
            _cs.Clear();
            _cs.AddRange(cs);
            return _cs;
        }

        public void Remove(_Coroutine ie)
        {
            cs.Remove(ie);
        }

        public void Add(_Coroutine ie)
        {
            cs.Add(ie);
        }

        public List<_Coroutine> GetByType(RunType type)
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

    private class _Coroutine
    {
        public IEnumerator IE;
        public RunType Type;
        public CoroutineState State = CoroutineState.Suspend;
        public _Coroutine(IEnumerator ie, RunType type)
        {
            this.IE = ie;
            this.Type = type;
        }
    }

    public partial class Coroutine
    {
        private Action then;

        public void SetThen(Action cb)
        {
            then = cb;
        }

        public void CallThen()
        {
            if(then != null)
            {
                then();
            }
        }
    }
}
