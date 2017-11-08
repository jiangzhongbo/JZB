using UnityEngine;
using System.Collections;
using System;

namespace UPromise
{
    public partial class Promise
    {
        public bool IsPending()
        {
            return _state == State._0_pending;
        }

        public bool IsFulfilled()
        {
            return _state == State._1_fulfilled;
        }

        public bool IsRejected()
        {
            return _state == State._2_rejected;
        }

        public bool IsAdopted()
        {
            return _state == State._3_adopted;
        }

        public object GetValue()
        {
            if (this._state == State._3_adopted)
            {
                return (this._value as Promise).GetValue();
            }

            if (!this.IsFulfilled())
            {
                throw new Exception("Cannot get a value of an unfulfilled promise.");
            }

            return this._value;
        }

        public object GetReason()
        {
            if (this._state == State._3_adopted)
            {
                return (this._value as Promise).GetReason();
            }

            if (!this.IsRejected())
            {
                throw new Exception("Cannot get a rejection reason of a non-rejected promise.");
            }

            return this._value;
        }


        public State GetState()
        {
            if (this._state == State._3_adopted)
            {
                return (this._value as Promise).GetState();
            }
            return this._state;
        }
    }
}
