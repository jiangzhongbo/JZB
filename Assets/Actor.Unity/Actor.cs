using UnityEngine;
using System.Collections;
using System;
using UPromise;
namespace UActor
{
    public interface IDispatch
    {

    }


    public class Sequence : IDispatch
    {

    }

    public class Concurrent : IDispatch
    {

    }

    public abstract class Actor<T> : MonoBehaviour where T : IDispatch
    {
        public int self_handle;
        private Co co;
        public abstract IEnumerator Init();
        void Awake()
        {
            co = gameObject.AddComponent<Co>();
            if(typeof(T) == typeof(Sequence))
            {
                co.With(Co.PoolType.Sequence);
            }
            else if (typeof(T) == typeof(Concurrent))
            {
                co.With(Co.PoolType.Concurrent);
            }
            else
            {

            }
            co.Run(Init());
        }

        public int Tell(int addr, string func_name, params object[] args)
        {
            return 0;
        }

        public int Tell(string addr, string func_name, params object[] args)
        {
            return 0;
        }

        public int Tell<K>(Func<K, IEnumerator> fn) where K : Actor<IDispatch>
        {
            return Skynet.Send<K>(0, 0, 0, 0, fn);
        }

        public int Tell(Func<IEnumerator> fn)
        {
            return 0;
        }

        public Promise Ask<K>(Func<K, IEnumerator> fn) where K : Actor<IDispatch>
        {
            return Promise.Resolve(0);
        }

        public Promise Ask(Func<IEnumerator> fn)
        {
            return Promise.Resolve(0);
        }

        public Promise Ask(int addr, string func_name, params object[] args)
        {
            return Promise.Resolve(0);
        }

        public Promise Ask(string addr, string func_name, params object[] args)
        {
            return Promise.Resolve(0);
        }

        protected void Reply(params object[] args)
        {

        }
    }
}

