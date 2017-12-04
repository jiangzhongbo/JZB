using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
namespace UPromise
{
    public partial class Promise
    {
        public delegate void CB(object value);
        public delegate object CB_With_Result(object value);

        public delegate void exec(CB a, CB b);

        private class Handler
        {
            public bool no_result = false;
            public CB_With_Result onFulfilled;
            public CB_With_Result onRejected;
            public Promise promise;

            public Handler(CB_With_Result onFulfilled, CB_With_Result onRejected, Promise promise, bool no_result)
            {
                this.onFulfilled = onFulfilled;
                this.onRejected = onRejected;
                this.promise = promise;
                this.no_result = no_result;
            }
        }

        private struct Error
        {

        }

        public enum State
        {
            _0_pending,
            _1_fulfilled,
            _2_rejected,
            _3_adopted,
        }

        private State _state = State._0_pending;
        private object _value = null;
        private List<Handler> _handlers = null;

        private Exception LAST_ERROR = null;
        private static object IS_ERROR = new Error();


        private static exec noop = (a, b) => { };

        public Promise(exec fn)
        {
            this._state = State._0_pending;
            this._value = null;
            this._handlers = null;
            if (fn == noop) return;
            doResolve(fn);
        }

        public Promise Then(CB_With_Result onFulfilled = null, CB_With_Result onRejected = null)
        {
            var res = new Promise(noop);
            handle(new Handler(onFulfilled, onRejected, res, false));
            return res;
        }

        public Promise Then(CB onFulfilled = null, CB onRejected = null)
        {
            var res = new Promise(noop);
            handle(new Handler(
                value =>
                {
                    if (onFulfilled != null)
                    {
                        onFulfilled(value);
                    }
                    return null;
                }
                ,
                reason =>
                {
                    if (onRejected != null)
                    {
                        onRejected(reason);
                    }
                    return null;
                }
                ,
                res
                ,
                true
                ));
            return res;
        }

        private void handle(Handler deferred)
        {
            Promise self = this;
            while (self._state == State._3_adopted)
            {
                self = self._value as Promise;
            }
            if (self._state == State._0_pending)
            {
                if (self._handlers == null)
                {
                    self._handlers = new List<Handler>();
                }
                self._handlers.Add(deferred);
                return;
            }
            handleResolved(deferred);
        }

        private void handleResolved(Handler deferred)
        {
            UnityScheduler.Timeout(() =>
            {
                var cb = this._state == State._1_fulfilled ? deferred.onFulfilled : deferred.onRejected;
                if (cb == null)
                {
                    if (this._state == State._1_fulfilled)
                    {
                        deferred.promise.resolve(this._value);
                    }
                    else
                    {
                        deferred.promise.reject(this._value);
                    }
                    return;
                }
                var ret = tryCallOne(cb, this._value);
                if (ret == IS_ERROR)
                {
                    deferred.promise.reject(LAST_ERROR);
                }
                else if (deferred.no_result)
                {
                    deferred.promise.resolve(this._value);
                }
                else
                {
                    deferred.promise.resolve(ret);
                }
            },
            0);
            
        }

        private void resolve(object newValue)
        {
            if (newValue == this)
            {
                reject(new Exception("A promise cannot be resolved with itself."));
            }
            if (newValue != null && newValue is Promise)
            {
                this._state = State._3_adopted;
                this._value = newValue;
                finale();
                return;
            }
            this._state = State._1_fulfilled;
            this._value = newValue;
            finale();
        }

        private void reject(object newValue)
        {
            this._state = State._2_rejected;
            this._value = newValue;
            finale();
        }

        private void finale()
        {
            if (_handlers != null)
            {
                int i = 0;
                int count = _handlers.Count;
                while (i < count)
                {
                    handle(_handlers[i]);
                    i++;
                }
                _handlers = null;
            }
        }

        private void doResolve(exec fn)
        {
            bool done = false;
            var res = tryCallTwo
                (
                fn,
                value =>
                {
                    if (done) return;
                    done = true;
                    resolve(value);
                },
                reason =>
                {
                    if (done) return;
                    done = true;
                    reject(reason);
                });
            if (!done && res == IS_ERROR)
            {
                done = true;
                reject(LAST_ERROR);
            }
        }

        private object tryCallOne(CB_With_Result fn, object a)
        {
            try
            {
                return fn(a);
            }
            catch (Exception ex)
            {
                LAST_ERROR = ex;
                return IS_ERROR;
            }
        }

        private object tryCallTwo(exec fn, CB a, CB b)
        {
            try
            {
                fn(a, b);
                return null;
            }
            catch (Exception ex)
            {
                LAST_ERROR = ex;
                return IS_ERROR;
            }
        }


    }

}

