using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
public class testco2 : MonoBehaviour
{
    void Start()
    {
        var co = gameObject.AddComponent<Co>();
        co.Run(test1())
            .Then(c => { });
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
        yield return UPromise.Promise.Resolve(0);
        yield return Co.ToNext;
        yield return Co.ToUpdate;
        yield return Co.ToFixedUpdate;
        yield return Co.ToLateUpdate;
    }

    async void test_async()
    {
        await TaskEx.Run(async () =>
        {
            await 0;
        });
    }
}
