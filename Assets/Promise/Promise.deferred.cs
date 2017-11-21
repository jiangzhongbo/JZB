using UnityEngine;
using System.Collections;
using System;

namespace UPromise
{
    public interface IDeferred
    {
        void Done(object value);
        void Fail(object reason);
        Promise Promise();
    }

    public class Deferred : IDeferred
    {
        private Promise.cb resolve;
        private Promise.cb reject;
        private Promise promise;
        public Deferred()
        {
            promise = new Promise((a, b) =>
            {
                this.resolve = a;
                this.reject = b;
            });
        }

        public void Done(object value)
        {
            resolve(value);
        }

        public void Fail(object reason)
        {
            reject(reason);
        }

        public Promise Promise()
        {
            return promise;
        }
    }

    public partial class Promise
    {
        public static Deferred Defer()
        {
            return new Deferred();
        }
    }
}