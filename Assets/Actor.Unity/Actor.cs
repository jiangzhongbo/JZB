using UnityEngine;
using System.Collections;
using System;
using UPromise;
namespace UActor
{
    public abstract class Actor : MonoBehaviour
    {
        public int self_handle;
        private Co co;
        public abstract IEnumerator Init();
        void Awake()
        {
            co = gameObject.AddComponent<Co>();
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

        public int Tell<T>(Func<T, IEnumerator> fn) where T : Actor
        {
            return Skynet.Send<T>(0, 0, 0, 0, fn);
        }

        public int Tell(Func<IEnumerator> fn)
        {
            return 0;
        }

        public Promise Ask<T>(Func<T, IEnumerator> fn) where T : Actor
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

