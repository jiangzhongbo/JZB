using UnityEngine;
using System.Collections;
using System;
namespace UPromise
{
    public partial class Promise
    {

        private static Promise TRUE = valuePromise(true);
        private static Promise FALSE = valuePromise(false);
        private static Promise NULL = valuePromise(null);
        private static Promise ZERO = valuePromise(0);
        private static Promise EMPTYSTRING = valuePromise("");


        public Promise Catch(cb_with_result onRejected)
        {
            return Then(null, onRejected);
        }

        public Promise Catch(cb onRejected)
        {
            return Then(null, onRejected);
        }

        public static Promise Reject(object reason)
        {
            return new Promise((resolve, reject) =>
            {
                reject(reason);
            });
        }

        public static Promise Resolve(object value)
        {
            if (value is Promise) return value as Promise;

            if (value == null) return NULL;
            if (value.Equals(true)) return TRUE;
            if (value.Equals(false)) return FALSE;
            if (value.Equals(0)) return ZERO;
            if (value.Equals("")) return EMPTYSTRING;
            return valuePromise(value);
        }

        private static Promise valuePromise(object value)
        {
            var p = new Promise(Promise.noop);
            p._state = State._1_fulfilled;
            p._value = value;
            return p;
        }

        public static Promise All(params Promise[] iterable)
        {
            return new Promise((resolve, reject) =>
            {
                Action<int, Promise> res = null;
                res = (int i, Promise val) =>
                {
                    while (val._state == State._3_adopted)
                    {
                        val = val._value as Promise;
                    }
                    if (val._state == State._1_fulfilled) res(i, val._value as Promise);
                    if (val._state == State._2_rejected) reject(val._value);
                    cb d = v =>
                    {
                        res(i, v as Promise);
                    };
                    val.Then(d, reject);
                    return;
                };
                for (var i = 0; i < iterable.Length; i++)
                {
                    res(i, iterable[i]);
                }
            });
        }

        public static Promise Race(params Promise[] values)
        {
            return new Promise((resolve, reject) =>
            {
                for (int i = 0; i < values.Length; i++)
                {
                    Promise.Resolve(values[i]).Then(resolve, reject);
                }
            });
        }
    }
}