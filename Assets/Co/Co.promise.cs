using UnityEngine;
using System.Collections;
using System;
using UPromise;
public partial class Co
{
    private void filterPromise()
    {
        filter(
            (pool, co, c) =>
            {
                if (c is Promise)
                {
                    Action<_Coroutine> f = (arg) =>
                    {
                        var p = (Promise)c;
                        p.Then(
                            value =>
                            {
                                pool.Add(arg);
                            },
                            reason =>
                            {
                                throw reason as Exception;
                            }
                        );
                    };
                    f(co);
                }
            }
        );
    }
}
