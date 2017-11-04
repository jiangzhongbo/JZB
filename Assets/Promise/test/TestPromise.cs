using UnityEngine;
using System.Collections;
using System;
using UPromise;
public class TestPromise : MonoBehaviour
{

    void Start()
    {
        new Promise((a, b) =>
        {
            _.Log("start1");
            a(0);
        })
        .Then(value =>
        {
            _.Log("1result:", value);
            return 1;
        })
        .Then(value =>
        {
            _.Log("2result:", value);
            return 2;
        })
        .Then(value =>
         {
            _.Log("3result:", value);
         });
    }

    IEnumerator later(Promise.cb a, Promise.cb b)
    {
        yield return new WaitForSeconds(1);
        _.Log("later done");
        a(1);
    }

    IEnumerator later2(Promise.cb a, Promise.cb b)
    {
        yield return new WaitForSeconds(1);
        _.Log("later2 done");
        a(2);
    }
}
