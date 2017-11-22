using UnityEngine;
using System.Collections;
using System;
using UPromise;
public partial class Co
{
    public interface IPromiser
    {
        Promise GetPromise();
    }

    private void filterPromise()
    {
        filter(
            (pool, co, c) =>
            {
                if (c is Promise)
                {
                    pool.Remove(co);
                    Action<Coroutine> f = (arg) =>
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
                if(c is IPromiser)
                {
                    pool.Remove(co);
                    Action<Coroutine> f = (arg) =>
                    {
                        var p = (IPromiser)c;
                        p.GetPromise().Then(
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

    public partial class Coroutine
    {
        public Promise ToPromise()
        {
            return new Promise((a, b) =>
            {
                Then(value =>
                {
                    a(value);
                });
            });
        }
    }
}
