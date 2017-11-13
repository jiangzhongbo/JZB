using UnityEngine;
using System.Collections;
using System;
using UPromise;
namespace UActor
{
    public interface IMailBox
    {

    }


    public class SequenceMailBox : IMailBox
    {

    }

    public class ConcurrentMailBox : IMailBox
    {

    }

    public abstract class Actor<T> where T : IMailBox
    {
        public int self_handle;
        private Co co;
        public abstract IEnumerator Init();
        void Awake()
        {
            //co = gameObject.AddComponent<Co>();
            if(typeof(T) == typeof(SequenceMailBox))
            {
                co.With(Co.PoolType.Sequence);
            }
            else if (typeof(T) == typeof(ConcurrentMailBox))
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

        public int Tell<K>(Func<K, IEnumerator> fn) where K : Actor<IMailBox>
        {
            return Skynet.Send<K>(0, 0, 0, 0, fn);
        }

        public int Tell(Func<IEnumerator> fn)
        {
            return 0;
        }

        public Promise Ask<K>(Func<K, IEnumerator> fn) where K : Actor<IMailBox>
        {
            return Promise.Resolve(0);
        }

        public Promise Ask(Func<object, IEnumerator> fn)
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

