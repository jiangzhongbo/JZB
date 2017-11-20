using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
namespace UActor
{
    public sealed class Skynet : MonoBehaviour
    {
        private static int handle_index = 0;
        private struct skynet_message
        {
            public Action<object[]> CB;
            public object[] Args;
        }

        private static Dictionary<int, Queue<skynet_message>> Q = new Dictionary<int, Queue<skynet_message>>();
        private static Dictionary<int, Channel> handle_chan = new Dictionary<int, Channel>();

        public static void Send(int addr, Action<object[]> cb, params object[] args)
        {
            Q[addr].Enqueue(new skynet_message() { CB = cb, Args = args });
        }

        public static ActorRef ActorOf(Func<Channel, IEnumerator> fn)
        {
            ++handle_index;
            var actorRef = new ActorRef(handle_index);
            var chan = new Channel(handle_index);
            handle_chan[handle_index] = chan;
            Q[handle_index] = new Queue<skynet_message>();
            fn(chan);
            return actorRef;
        }
        private static Co co;
        private void Start()
        {
            co = gameObject.AddComponent<Co>();
        }

        void Update()
        {
            foreach (var kv in Q)
            {
                if (kv.Value.Count > 0)
                {
                    
                    
                }
            }
        }
    }
}
