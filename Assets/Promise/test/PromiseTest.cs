using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UPromise;
using System;
public class PromiseTest {

    [Test]
    public void Test_must_be_called_immediately_before_promise_return()
    {
        var called = false;
        new Promise((a, b) =>
        {
            called = true;

        });
        Assert.IsTrue(called);
    }

    [Test]
    public void Test_then_immediately()
    {
        new Promise((a, b) =>
        {
            a(1);
        }).Then(value =>
        {
            Assert.AreEqual(value, 1, "Then error");
        });
	}

	[UnityTest]
    public IEnumerator Test_then_later()
    {
        Promise.CB cb = null;
        new Promise((a, b) =>
        {
            cb = a;
        }).Then(value =>
        {
            Assert.AreEqual(value, 1, "Then error");
        });
        yield return later(5);
        cb(1);
	}

    IEnumerator later(float t, Action func = null)
    {
        yield return new WaitForSeconds(t);
        if(func != null) func();
    }
}
