using UnityEngine;
using System.Collections;
using System;

namespace UPromise
{
    public partial class Promise
    {
        public delegate object cb_without_parm();

        public Promise Finally(cb_without_parm cb)
        {
            return Then(
                value =>
                {
                    Promise.Resolve(cb()).Then( _ =>
                    {
                        return value;
                    });
                },
                reason =>
                {
                    Promise.Resolve(cb()).Then( _ =>
                    {
                        throw reason as Exception;
                    });
                });
        }

    }
}