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

        public static Promise All(params Promise[] args)
        {
            return new Promise((resolve, reject) =>
            {
                if (args == null || args.Length == 0) resolve(new Promise[0]);
                var resolvedCounter = 0;
                var promiseNum = args.Length;
                var resolvedValues = new object[promiseNum];
                for (var i = 0; i < promiseNum; i++)
                {
                    Action<int> f = (index) =>
                    {
                        Promise.Resolve(args[index]).Then(value =>
                        {
                            resolvedCounter++;
                            resolvedValues[index] = value;
                            if (resolvedCounter == promiseNum) {
                                resolve(resolvedValues);
                            }
                        }
                        ,
                        reason =>
                        {
                            reject(reason);
                        });
                    };
                    f(i);
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