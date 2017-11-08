using UnityEngine;
using System.Collections;
using UActor;
using System;
public class TestActor : Actor<Sequence>
{
    public override IEnumerator Init()
    {
        yield return 
            Ask(() => { return A(0); })
            .Then(value =>
            {
                return Ask(() => { return A((int)value); });
            });
    }

    public IEnumerator A(int value)
    {
        yield return new WaitForSeconds(10);
        Reply(value + 1);
        yield return null;
    }

    public IEnumerator B(int value)
    {
        Reply(value + 1);
        yield return null;
    }
}
