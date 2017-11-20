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

    public void Dispatch()
    {

    }

    public Action<object> Read()
    {
        return value => { };
    }
         
}
