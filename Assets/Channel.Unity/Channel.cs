using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace UChannel
{
    public sealed class Channel<T>
    {
        private Queue<T> _buffer;
        private uint _buffer_size = 1;

        public Channel(uint bufferSize = 1)
        {
            _buffer_size = bufferSize;
            _buffer = new Queue<T>((int)_buffer_size);
        }

        public T Read()
        {
            return _buffer.Dequeue();
        }

        public int Write(T t)
        {
            _buffer.Enqueue(t);
            return 0;
        }

    }
}


