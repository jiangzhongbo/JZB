using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
namespace UActor
{
    public class Skynet : MonoBehaviour
    {
        private static int handle_index = 0;
        private struct skynet_message
        {

        }

        private static Dictionary<int, Queue<skynet_message>> Q = new Dictionary<int, Queue<skynet_message>>();
        private static Dictionary<int, ActorRef> handle_ref = new Dictionary<int, ActorRef>();
        private static Dictionary<int, Channel> handle_chan = new Dictionary<int, Channel>();
        public static int SendToChannel(
            int session
            )
        {
            return session;
        }

        public static int SendToActorRef(
            int session
            )
        {
            return session;
        }

        public static ActorRef ActorOf(Func<Channel, IEnumerator> fn)
        {
            ++handle_index;
            var actorRef = new ActorRef(handle_index);
            var chan = new Channel(handle_index);
            handle_ref[handle_index] = actorRef;
            handle_chan[handle_index] = chan;
            fn(chan);
            return actorRef;
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
