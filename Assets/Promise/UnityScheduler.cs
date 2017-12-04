using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
namespace UPromise
{
    public class UnityScheduler : MonoBehaviour
    {
        private static List<Tuple<float, Action>> timeouts = new List<Tuple<float, Action>>();
        private static List<Tuple<float, Action>> removeTimeouts = new List<Tuple<float, Action>>();
        private static float t = 0;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            var go = new GameObject("UnityScheduler");
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<UnityScheduler>();
        }

        void Update()
        {
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

        public static void Timeout(Action func, float ti)
        {
            timeouts.Add(new Tuple<float, Action>(t + ti, func));
        }

        public static Action CancelableTimeout(Action func, float ti)
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
            Timeout(cb, ti);
            return cancel;
        }
    }

}
