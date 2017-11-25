using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UPromise;
namespace Actor
{
    public sealed class Skynet : MonoBehaviour
    {
        private static int handle_index = 0;
        public struct SkynetMessage
        {
            public Action<object[]> CB;
            public object[] Args;
        }

        private static Dictionary<int, Queue<SkynetMessage>> Q = new Dictionary<int, Queue<SkynetMessage>>();
        private static Dictionary<int, Queue<Promise.cb>> handle_cb = new Dictionary<int, Queue<Promise.cb>>();
        public static void Send(int addr, Action<object[]> cb, params object[] args)
        {
            Q[addr].Enqueue(new SkynetMessage() { CB = cb, Args = args });
        }

        public static ActorRef ActorOf(Func<Channel, IEnumerator> fn)
        {
            ++handle_index;
            var actorRef = new ActorRef(handle_index);
            var chan = new Channel(handle_index, tigger);
            handle_cb[handle_index] = new Queue<Promise.cb>();
            Q[handle_index] = new Queue<SkynetMessage>();
            co.Run(fn(chan));
            return actorRef;
        }
        private static Co co;
        private void Start()
        {
            co = gameObject.AddComponent<Co>();
        }

        private static void tigger(int handle, Promise.cb fn)
        {
            if (Q[handle].Count > 0)
            {
                var msg = Q[handle].Dequeue();
                fn(msg);
            }
            else
            {
                handle_cb[handle].Enqueue(fn);
            }
        }

        void Update()
        {
            foreach (var kv in handle_cb)
            {
                if(kv.Value.Count > 0)
                {
                    if(Q[kv.Key].Count > 0)
                    {
                        var msg = Q[kv.Key].Dequeue();
                        var fn = kv.Value.Dequeue();
                        fn(msg);
                    }
                }
            }
        }
    }
}
