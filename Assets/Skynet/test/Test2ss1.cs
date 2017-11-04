using UnityEngine;
using System.Collections;
using UPromise;
using System;
public class Test2ss1 : SkynetService
{
    void Awake()
    {
        Skynet.ServiceOf(this, "Test2ss1");
    }

    void Start()
    {
        ask("fuck");
    }

    IEnumerator fuck()
    {
        var p = call("Test2ss2", "fuck");
        yield return p;
        p.Then(value =>
        {
            _.Log(((object[])value)[0]);
        });
        yield return new Promise((s, f) =>
        {

        });
    }

    void fuck2()
    {

    }

    IEnumerator a()
    {
        yield return null;
    }
}
