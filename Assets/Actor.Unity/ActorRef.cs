using UnityEngine;
using System.Collections;
using UPromise;
namespace UActor
{
    public class ActorRef
    {
        public readonly int Handle;

        public ActorRef(int handle)
        {
            Handle = handle;
        }

        public void Tell(object value)
        {

        }

        public Promise Ask(object value)
        {
            return null;
        }
    }
}
