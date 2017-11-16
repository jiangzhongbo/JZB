using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public sealed class Channel
{
    public readonly int Handle;
    public Channel(int handle)
    {
        Handle = handle;
    }

    public void Dispatch(Action<object[]> cb, object[] args)
    {

    }

    public Action<object> Read()
    {
        return value => { };
    }
         
}
