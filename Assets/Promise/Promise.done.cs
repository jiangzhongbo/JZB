using UnityEngine;
using System.Collections;
using System;
namespace UPromise
{
    public partial class Promise
    {
        public void Done(cb onFulfilled = null, cb onRejected = null)
        {
            Then(onFulfilled, onRejected)
                .Catch(error =>
                {
                    throw error as Exception;
                });
        }
    }
}