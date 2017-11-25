using UnityEngine;
using System.Collections;
using Actor;
using System;
using Actor;
public class TestActor
{
    IEnumerator test()
    {
        var actor = Skynet.ActorOf(echo);
        var ask1 = actor.Ask(1);
        yield return ask1;
        ask1.Then(value => Debug.Log(value));
    }

    IEnumerator echo(Channel chan)
    {
        yield return chan;
        var mail = chan.Read();
        mail.Reply(mail.Args[0]);
    }
}
