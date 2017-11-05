using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UPromise;
public partial class Co : MonoBehaviour
{
    private interface IGroupPool
    {
        List<Group> Get();
        List<Group> GetByType(RunType type);
        void Remove(Group ie);
        void Add(Group ie);
    }
    
    private class ConcurrentPool : IGroupPool
    {
        private List<Group> gs = new List<Group>();
        private List<Group> _gs = new List<Group>();


        public List<Group> Get()
        {
            _gs.Clear();
            _gs.AddRange(gs);
            return _gs;
        }

        public void Remove(Group ie)
        {
            gs.Remove(ie);
        }

        public void Add(Group ie)
        {
            gs.Add(ie);
        }

        public List<Group> GetByType(RunType type)
        {
            _gs.Clear();
            for(int i = 0; i < _gs.Count; i++)
            {
                if(gs[i].Type == type)
                {
                    _gs.Add(gs[i]);
                }
            }
            return _gs;
        }
    }

    private class SequencePool : IGroupPool
    {
        private Queue<Group> queue = new Queue<Group>();
        private List<Group> _ret = new List<Group>(1);

        public void Add(Group ie)
        {
            queue.Enqueue(ie);
        }

        public List<Group> Get()
        {
            _ret.Clear();
            _ret.Add(queue.Peek());
            return _ret;
        }

        public List<Group> GetByType(RunType type)
        {
            _ret.Clear();
            if(queue.Peek().Type == type)
            {
                _ret.Add(queue.Peek());
            }
            return _ret;
        }

        public void Remove(Group ie)
        {
            if(queue.Peek() == ie)
            {
                queue.Dequeue();
            }
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

    public class Group
    {
        public IEnumerator IE;
        public RunType Type;

        public Group(IEnumerator ie, RunType type)
        {
            this.IE = ie;
            this.Type = type;
        }
    }

    public readonly static object ToNext = new object();

    public readonly static object ToUpdate = new object();
    public readonly static object ToFixedUpdate = new object();
    public readonly static object ToLateUpdate = new object();

    private IGroupPool pool = new ConcurrentPool();
    private Dictionary<IEnumerator, Group> childie_parentg = new Dictionary<IEnumerator, Group>();

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

    public void Resume(IEnumerator ie, RunType type = RunType.Update)
    {
        pool.Add(Wrap(ie, type));
    }

    public void Resume(Group g)
    {
        pool.Add(g);
    }

    public Group Wrap(IEnumerator ie, RunType type = RunType.Update)
    {
        return new Group(ie, type);
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

    void handle(IGroupPool pool, RunType type)
    {
        var gs = pool.GetByType(type);
        if (gs.Count > 0)
        {
            for(int i = 0; i < gs.Count; i++)
            {
                var g = gs[i];
                handle_g(g, pool);
            }
        }
    }

    void handle_g(Group g, IGroupPool pool)
    {
        bool ok = g.IE.MoveNext();
        if (ok)
        {
            var c = g.IE.Current;
            if (c is Promise)
            {
                Action<Group> f = (arg) =>
                {
                    var p = (Promise)c;
                    p.Then(value =>
                    {
                        pool.Add(arg);
                    });
                };
                f(g);
                pool.Add(g);
            }
            else if (c is IEnumerator)
            {
                pool.Remove(g);
                var _g = Wrap(c as IEnumerator);
                pool.Add(_g);
                childie_parentg[_g.IE] = g;
            }
            else if (c is Group)
            {
                pool.Remove(g);
                var _g = c as Group;
                pool.Add(_g);
                childie_parentg[_g.IE] = g;
            }
            else if(c is object && c == ToNext)
            {
                handle_g(g, pool);
            }
            else if(c is object && c == ToUpdate)
            {
                g.Type = RunType.Update;
            }
            else if (c is object && c == ToFixedUpdate)
            {
                g.Type = RunType.FixedUpdate;
            }
            else if (c is object && c == ToLateUpdate)
            {
                g.Type = RunType.LateUpdate;
            }
        }
        else
        {
            if (childie_parentg.ContainsKey(g.IE))
            {
                pool.Add(childie_parentg[g.IE]);
                childie_parentg.Remove(g.IE);
            }
            pool.Remove(g);
        }
    }
}
