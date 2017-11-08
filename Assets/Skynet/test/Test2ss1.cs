using UnityEngine;
using System.Collections;
using UPromise;
using System;
using System.Reflection;
using Skynet1;
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
        aaa();
        yield return null;
    }

    void aaa()
    {
        _.Log(MethodBase.GetCurrentMethod().DeclaringType);
    }

    void fuck2()
    {

    }

    IEnumerator a()
    {
        yield return null;
    }
}
