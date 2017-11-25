using UnityEngine;
using System.Collections;
using UPromise;
namespace Actor
{
    public class ActorRef
    {
        public readonly int Handle;

        public ActorRef(int handle)
        {
            Handle = handle;
        }

        public void Tell(params object[] args)
        {
            Skynet.Send(
                Handle, 
                null,
                args
            );
        }

        public Promise Ask(params object[] args)
        {
            return new Promise((ok, errer) =>
            {
                Skynet.Send(
                    Handle,
                    values =>
                    {
                        ok(values);
                    },
                    args
                );
            });
        }
    }
}
