using UnityEngine;
using System.Collections;

public partial class Co
{
    public readonly static object ToNext = new object();
    public readonly static object ToUpdate = new object();
    public readonly static object ToFixedUpdate = new object();
    public readonly static object ToLateUpdate = new object();

    private void filterExtensions()
    {
        filter(
            (pool, co, c) =>
            {
                if (c is object && c == ToNext)
                {
                    handle_coroutine(co);
                }
            }
        );

        filter(
            (pool, co, c) =>
            {
                if (c is object && c == ToUpdate)
                {
                    co_internal[co].SetRunType(RunType.Update);
                }
            }
        );
        filter(
            (pool, co, c) =>
            {
                if (c is object && c == ToFixedUpdate)
                {
                    co_internal[co].SetRunType(RunType.FixedUpdate);
                }
            }
        );
        filter(
            (pool, co, c) =>
            {
                if (c is object && c == ToLateUpdate)
                {
                    co_internal[co].SetRunType(RunType.LateUpdate);
                }
            }
        );
    }
}
