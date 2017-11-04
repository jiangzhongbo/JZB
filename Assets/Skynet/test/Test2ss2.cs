using UnityEngine;
using System.Collections;

public class Test2ss2 : SkynetService
{
    void Awake()
    {
        Skynet.ServiceOf(this, "Test2ss2");
    }

    IEnumerator fuck()
    {

        yield return new WaitForSeconds(5);
        ret(111);
    }

    IEnumerator fuck2()
    {

        yield return new WaitForSeconds(3);
        ret(222);
    }

    IEnumerator fuck3()
    {

        yield return new WaitForSeconds(9);
        ret(333);
    }
}
