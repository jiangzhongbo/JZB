using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Skynet1
{

    struct skynet_message
    {
        public int source;
        public int dest;
        public int session;
        public int type;
        public string func_name;
        public object[] args;
    }

    public class Skynet : MonoBehaviour
    {
        public bool Debug = false;
        private static Skynet instance;
        public const int TYPE_NORMAL = 0;
        public const int TYPE_RESPONSE = 1;
        private static int handle_index = 0;
        private static Dictionary<int, Queue<skynet_message>> Q = new Dictionary<int, Queue<skynet_message>>();
        private static Dictionary<int, string> id_name = new Dictionary<int, string>();
        private static Dictionary<string, int> name_id = new Dictionary<string, int>();
        private static Dictionary<int, SkynetService> services = new Dictionary<int, SkynetService>();

        private static float t = 0;
        private static List<Tuple<float, Action>> timeouts = new List<Tuple<float, Action>>();
        private static List<Tuple<float, Action>> removeTimeouts = new List<Tuple<float, Action>>();

        void Awake()
        {
            instance = this;
        }

        public static int Send(int source, int dest, int type, int session, string func_name, params object[] args)
        {
            if (instance && instance.Debug)
            {
                string source_name = null;
                string dest_name = null;
                if (id_name.ContainsKey(source))
                {
                    source_name = id_name[source];
                }
                if (id_name.ContainsKey(dest))
                {
                    dest_name = id_name[dest];
                }
                if (source_name == null)
                {
                    source_name = source.ToString();
                }
                if (dest_name == null)
                {
                    dest_name = dest.ToString();
                }
                _.Log("#Skynet# Frames: ", Time.frameCount, "      Send MSG: ", source_name, "->", dest_name, ":", func_name);
            }
            Q[dest].Enqueue(new skynet_message() { source = source, dest = dest, session = session, type = type, func_name = func_name, args = args });
            return session;
        }


        public static void Timeout(float ti, Action func)
        {
            timeouts.Add(new Tuple<float, Action>(t + ti, func));
        }

        public static Action CancelableTimeout(float ti, Action func)
        {
            Action cb = () =>
            {
                if (func != null)
                {
                    func();
                }
            };
            Action cancel = () =>
            {
                func = null;
            };
            Timeout(ti, cb);
            return cancel;
        }

        public static int QueryService(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception(name + " is null!");
            }
            if (!name_id.ContainsKey(name))
            {
                throw new Exception(name + "Not Found!");
            }
            return name_id[name];
        }

        public static string QueryServiceName(int addr)
        {
            return id_name[addr];
        }

        public static SkynetService ServiceRef(string name)
        {
            int addr = QueryService(name);
            if (addr == -1)
            {
                return null;
            }
            return services[addr];
        }

        public static T ServiceRef<T>(string name) where T : SkynetService
        {
            int addr = QueryService(name);
            if (addr == -1)
            {
                return null;
            }
            return services[addr] as T;
        }

        public static void ServiceOf(SkynetService ss, string name = null)
        {
            ss.self_handle = handle_index;
            services[ss.self_handle] = ss;
            Q[ss.self_handle] = new Queue<skynet_message>();
            if (!string.IsNullOrEmpty(name))
            {
                id_name[ss.self_handle] = name;
                name_id[name] = ss.self_handle;
                ss.self_name = name;
            }
            ss.init = true;
            ++handle_index;
        }

        void Update()
        {
            foreach (var kv in Q)
            {
                if (kv.Value.Count > 0)
                {
                    var m = kv.Value.Dequeue();
                    if (Debug)
                    {
                        string source_name = null;
                        string dest_name = null;
                        if (id_name.ContainsKey(m.source))
                        {
                            source_name = id_name[m.source];
                        }
                        if (id_name.ContainsKey(m.dest))
                        {
                            dest_name = id_name[m.dest];
                        }
                        if (source_name == null)
                        {
                            source_name = m.source.ToString();
                        }
                        if (dest_name == null)
                        {
                            dest_name = m.dest.ToString();
                        }
                        _.Log("#Skynet# Frames: ", Time.frameCount, "      Handle MSG: ", source_name, "->", dest_name, ":", m.func_name);
                    }
                    services[kv.Key].DispatchMessage(m.source, m.dest, m.type, m.session, m.func_name, m.args);

                }
            }

            t += Time.deltaTime;

            for (int i = 0; i < timeouts.Count; i++)
            {
                if (t >= timeouts[i].Item1)
                {
                    if (timeouts[i].Item2 != null) timeouts[i].Item2();
                    removeTimeouts.Add(timeouts[i]);
                }
            }

            for (int i = 0; i < removeTimeouts.Count; i++)
            {
                timeouts.Remove(removeTimeouts[i]);
            }
        }
    }

}
