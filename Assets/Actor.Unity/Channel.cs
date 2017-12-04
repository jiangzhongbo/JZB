using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UPromise;
namespace Actor
{
    public sealed class Channel : Co.IPromiser
    {
        public class Mail
        {
            private Skynet.SkynetMessage msg;
            public Mail(Skynet.SkynetMessage msg)
            {
                this.msg = msg;
            }

            public object[] Args
            {
                get
                {
                    return msg.Args;
                }
            }
            public void Reply(params object[] values)
            {
                msg.CB(values);
            }
        }
        public readonly int Handle;
        private readonly Action<int, Promise.CB> tigger;
        private Queue<Skynet.SkynetMessage> queue = new Queue<Skynet.SkynetMessage>(1);
        public Channel(int handle, Action<int, Promise.CB> fn)
        {
            Handle = handle;
            tigger = fn;
        }

        public Promise GetPromise()
        {
            return new Promise((a, b) =>
            {
                tigger(Handle, a);
            })
            .Then(value =>
            {
                queue.Enqueue((Skynet.SkynetMessage)value);
            });
        }

        public Mail Read()
        {
            if(queue.Count > 0)
            {
                return new Mail(queue.Dequeue());
            }
            throw new Exception("no msg");
        }
     
    
    }

}
