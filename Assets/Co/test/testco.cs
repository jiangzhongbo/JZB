using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System;
public class testco {

	[Test]
	public void test_co_base() {
        var co = new GameObject().AddComponent<Co>();
        co.Run(test1());
	}

    [UnityTest]
    public IEnumerator test_co_promise()
    {
        var co = new GameObject().AddComponent<Co>();
        co.Run(testPromise(co));
        yield return new WaitForSeconds(7);
    }

    [UnityTest]
    public IEnumerator test_co_next()
    {
        var co = new GameObject().AddComponent<Co>();
        co.Run(testnextframe());
        yield return null;
        yield return Co.ToNext;
    }

    IEnumerator testnextframe()
    {
        int i1 = Time.frameCount;
        yield return Co.ToNext;
        int i2 = Time.frameCount;

        Assert.AreEqual(i1, i2, "test next error i1 = {0}, i2 = {1}", i1, i2);
    }

    IEnumerator testPromise(MonoBehaviour mono)
    {
        _.Log("1");
        yield return new UPromise.Promise((a, b) =>
        {
            mono.StartCoroutine(later(5, () => a(1)));
        });
        _.Log("2");
    }



    IEnumerator later(int time, Action cb)
    {
        yield return new WaitForSeconds(time);
        _.Log("fuck");
        if(cb != null)
        {
            cb();
        }
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
}
