using UnityEngine;
using System.Collections;
using System;

namespace UPromise
{
    public class Deferred
    {
        private Promise.CB resolve;
        private Promise.CB reject;
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