using UnityEngine;
using System.Collections;
using UActor;
using System;
public class TestActor : Actor<SequenceMailBox>
{
    public override IEnumerator Init()
    {
        var answer = Ask(() => { return A(0); });
        yield return answer;
        var answer2 = Ask(() => { return A((int)answer.GetValue()); });
        yield return answer2;

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
