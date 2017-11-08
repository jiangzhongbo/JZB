using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
namespace UActor
{
    public class Skynet : MonoBehaviour
    {
        private struct skynet_message
        {
            public int source;
            public int dest;
            public int session;
            public int type;
            public string func_name;
            public object[] args;
            public Delegate fn;
        }

        private static Dictionary<int, Queue<skynet_message>> Q = new Dictionary<int, Queue<skynet_message>>();

        public static int Send(
            int source, 
            int dest, 
            int session, 
            int type, 
            string func_name, 
            params object[] args
            )
        {
            return 0;
        }

        public static int Send<T>(
            int source,
            int dest,
            int session,
            int type,
            Func<T, IEnumerator> fn
            ) where T : Actor<IDispatch>
        {
            return 0;
        }

        public static int Timeout(float delay, Action fn)
        {
            return 0;
        }

        public static Action CancalableTimeout(float delay, Action fn)
        {
            return () => { };
        }
    }
}
