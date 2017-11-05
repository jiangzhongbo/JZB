using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UPromise;
public abstract class SkynetService : MonoBehaviour
{
    private class cotask
    {
        public const int STATE_SUSPEND = 0;
        public const int STATE_RUNNING = 1;
        public IEnumerator ie;
        public int call_session;
        public int call_addr;
        public int call_type;
        public string call_funcname;
        public int state;
        public object[] args;
        public cotask()
        {
            state = STATE_SUSPEND;
        }
    }

    public bool Debug = false;
    private int session_id = 0;
    public string self_name;
    public int self_handle = 0;
    public bool init = false;
    public string current_task_name = "None";
    public int task_num = 0;

    // out
    private Queue<cotask> localqueue = new Queue<cotask>();
    private Dictionary<int, Promise.cb> localsession_promisecb = new Dictionary<int, Promise.cb>();
    private List<cotask> ie_ret = new List<cotask>();

    private cotask current_cotask = null;


    void OnEnable()
    {
        StopAllCoroutines();
        fork(mainLoop());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator mainLoop()
    {
        while (true)
        {
            if (localqueue.Count > 0)
            {
                current_cotask = localqueue.Dequeue();
                task_num = localqueue.Count;
                current_task_name = current_cotask.call_funcname;
                bool is_suspend = false;
                while (current_cotask.ie.MoveNext())
                {
                    var c = current_cotask.ie.Current;
                    if (c is Promise)
                    {
                        var p = (Promise)c;
                        p.Then(value =>
                        {
                            localqueue.Enqueue(current_cotask);
                        });
                        is_suspend = true;
                        break;
                    }
                    else if (c is YieldInstruction)
                    {
                        yield return null;
                    }
                    else if (c is WWW)
                    {
                        yield return null;
                    }
                    if (c is IEnumerator)
                    {
                        yield return fork(c as IEnumerator);
                    }
                    else
                    {
                        yield return c;
                    }
                }
                if (!is_suspend)
                {
                    if (!ie_ret.Contains(current_cotask))
                    {
                        rawret(current_cotask.call_addr, current_cotask.call_type, current_cotask.call_session);
                    }
                }
                if (ie_ret.Contains(current_cotask))
                {
                    ie_ret.Remove(current_cotask);
                }
            }
            yield return null;
        }
    }

    private int newSession()
    {
        session_id++;
        return session_id;
    }

    public int self()
    {
        return self_handle;
    }

    protected Coroutine fork(IEnumerator ie)
    {
        return StartCoroutine(ie);
    }


    public int send(int addr, string func_name, params object[] args)
    {
        if (Debug)
        {
            string source_name = self_name;
            string dest_name = null;

            dest_name = Skynet.QueryServiceName(addr);
            if (source_name == null)
            {
                source_name = self_handle.ToString();
            }
            if (dest_name == null)
            {
                dest_name = addr.ToString();
            }
            _.Log("#Skynet#", self_name + "(" + self_handle + ")", "      Frames: ", Time.frameCount, "      Send MSG: ", source_name, "->", dest_name, ":", func_name);
        }
        return Skynet.Send(self_handle, addr, Skynet.TYPE_NORMAL, newSession(), func_name, args);
    }

    public int send(string addr, string func_name, params object[] args)
    {
        return send(Skynet.QueryService(addr), func_name, args);
    }

    protected Promise call(int addr, string func_name, params object[] args)
    {
        int session = send(addr, func_name, args);
        var p = new Promise((c, f) =>
        {
            localsession_promisecb[session] = c;
        });
        return p;
    }

    protected Promise call(string addr, string func_name, params object[] args)
    {
        return call(Skynet.QueryService(addr), func_name, args);
    }

    public int tell(string func_name, params object[] args)
    {
        return send(self(), func_name, args);
    }

    public Promise ask(string func_name, params object[] args)
    {
        return call(self(), func_name, args);
    }

    protected void ret(params object[] objs)
    {
        if (!ie_ret.Contains(current_cotask))
        {
            ie_ret.Add(current_cotask);
            rawret(current_cotask.call_addr, current_cotask.call_type, current_cotask.call_session, objs);
        }
        else
        {
            throw new Exception("current_ie_ret_handled = true");
        }
    }

    private void rawret(int addr, int type, int session, params object[] objs)
    {
        if (type == Skynet.TYPE_NORMAL)
        {
            Skynet.Send(self_handle, addr, Skynet.TYPE_RESPONSE, session, null, objs);
        }
    }

    protected Action response(params object[] objs)
    {
        if (!ie_ret.Contains(current_cotask))
        {
            int addr = current_cotask.call_addr;
            int type = current_cotask.call_type;
            int session = current_cotask.call_session;
            ie_ret.Add(current_cotask);
            return () =>
            {
                rawret(addr, type, session, objs);
            };
        }
        else
        {
            throw new Exception("current_ie_ret_handled = true");
        }
    }


    public void DispatchMessage(int source, int dest, int type, int session, string func_name, params object[] args)
    {
        if (type == Skynet.TYPE_RESPONSE)
        {
            if (Debug)
            {
                string source_name = null;
                string dest_name = null;

                dest_name = Skynet.QueryServiceName(dest);
                source_name = Skynet.QueryServiceName(source);
                if (source_name == null)
                {
                    source_name = source.ToString();
                }
                if (dest_name == null)
                {
                    dest_name = dest.ToString();
                }
                _.Log("#Skynet#", self_name + "(" + self_handle + ")", "      Frames: ", Time.frameCount, "      Handle Response Msg: ", source_name, "->", dest_name);
            }
            if (localsession_promisecb.ContainsKey(session))
            {
                localsession_promisecb[session](args);
                localsession_promisecb.Remove(session);
            }
        }
        else if (type == Skynet.TYPE_NORMAL)
        {
            if (Debug)
            {
                string source_name = self_name;
                string dest_name = null;

                dest_name = Skynet.QueryServiceName(dest);
                if (source_name == null)
                {
                    source_name = self_handle.ToString();
                }
                if (dest_name == null)
                {
                    dest_name = dest.ToString();
                }
                _.Log("#Skynet#", self_name + "(" + self_handle + ")", "      Frames: ", Time.frameCount, "      Handle Normal Msg: ", source_name, "->", dest_name, ":", func_name);
            }
            IEnumerator ie = GetType()
                .GetMethod(func_name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(this, args) as IEnumerator;
            localqueue.Enqueue(new cotask() { ie = ie, call_session = session, call_addr = source, call_type = type, call_funcname = func_name, args = args });
            task_num = localqueue.Count;
        }
    }
    void handleQueue()
    {
        if (localqueue.Count > 0)
        {
            current_cotask = localqueue.Dequeue();
            task_num = localqueue.Count;
            current_task_name = current_cotask.call_funcname;
            bool is_suspend = false;
            bool ok = current_cotask.ie.MoveNext();
            if (ok)
            {
                var c = current_cotask.ie.Current;
                if (c is Promise)
                {
                    var p = (Promise)c;
                    p.Then(value =>
                    {
                        localqueue.Enqueue(current_cotask);
                    });
                    is_suspend = true;
                }
                else if (c is YieldInstruction)
                {
                }
                else if (c is WWW)
                {
                }
                if (c is IEnumerator)
                {
                }
                else
                {
                }
            }
            else
            {
                if (!is_suspend)
                {
                    if (!ie_ret.Contains(current_cotask))
                    {
                        rawret(current_cotask.call_addr, current_cotask.call_type, current_cotask.call_session);
                    }
                }
                if (ie_ret.Contains(current_cotask))
                {
                    ie_ret.Remove(current_cotask);
                }
            }

        }
    }
}
