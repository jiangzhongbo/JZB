using UnityEngine;
using System.Collections;
using System;
namespace UPromise
{
    public partial class Promise
    {
        public void Done(CB onFulfilled = null, CB onRejected = null)
        {
            Then(onFulfilled, onRejected)
                .Catch(error =>
                {
                    throw error as Exception;
                });
        }
    }
}