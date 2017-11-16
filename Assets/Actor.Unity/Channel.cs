using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public sealed class Channel
{
    private Queue<object> _queue = new Queue<object>();
    public readonly int Handle;
    public Channel(int handle)
    {
        Handle = handle;
    }

    public Action<object> Read()
    {
        return value => { };
    }
         
}
