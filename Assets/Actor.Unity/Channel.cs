using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UPromise;

public sealed class Channel : Co.IPromiser
{
    public readonly int Handle;
    private readonly Func<int, Tuple<Action<object[]>, object[]>> getMsg;
    private Promise.cb done;
    public Channel(int handle, Func<int, Tuple<Action<object[]>, object[]>> fn, out Action dispatch)
    {
        Handle = handle;
        getMsg = fn;
        dispatch = Dispatch;
    }

    private void Dispatch()
    {
        
    }

    public Promise GetPromise()
    {
        return new Promise((a, b) =>
        {
            done = a;
        });
    }

    public Action<object> Read()
    {
        return value => { };
    }
         
}
