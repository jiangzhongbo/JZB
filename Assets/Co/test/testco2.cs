using UnityEngine;
using System.Collections;

public class testco2 : Co
{
    void Start()
    {
        Resume(test1());
    }


    IEnumerator test1()
    {
        _.Log(1);
        yield return test2();
        _.Log(4);
    }

    IEnumerator test2()
    {
        _.Log(2);
        yield return null;
        _.Log(3);
    }

    IEnumerator test3()
    {
        yield return ToNext;
        yield return ToUpdate;
        yield return ToFixedUpdate;
        yield return ToLateUpdate;
    }
}
