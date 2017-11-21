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
                    pool.Remove(co);
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
                if(c is IDeferred)
                {
                    pool.Remove(co);
                    Action<_Coroutine> f = (arg) =>
                    {
                        var p = (IDeferred)c;
                        p.Promise().Then(
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
        public Promise Then(Action<Coroutine> cb)
        {
            return new Promise((a, b) =>
            {
                SetThen(() =>
                {
                    cb(this);
                    a(this);
                });
            });
        }

        public Promise Then(Func<Coroutine,object> cb)
        {
            return new Promise((a, b) =>
            {
                SetThen(() =>
                {
                    a(cb(this));
                });
            });
        }

        public Promise Then()
        {
            return new Promise((a, b) =>
            {
                SetThen(() =>
                {
                    a(this);
                });
            });
        }
    }

}
