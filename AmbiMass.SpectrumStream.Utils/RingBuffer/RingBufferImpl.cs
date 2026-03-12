using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AmbiMass.SpectrumStream.Utils.RingBuffer
{
    public class RingBuffer<T>
    {
        private T[] buffer;
        public int head;
        public int tail;
        public int size;
        public int capacity;

        public RingBuffer(int capacity)
        {
            this.capacity = capacity;
            buffer = new T[capacity];
            head = 0;
            tail = 0;
            size = 0;
        }

        public bool IsEmpty => size == 0;
        public bool IsFull => size == capacity;

        public int getSize()
        {
            return size;
        }
        public void Enqueue(T item)
        {
            buffer[tail] = item;
            tail = (tail + 1) % capacity;
            size++;
        }

        public T Dequeue()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Buffer is empty");
            }

            T item = buffer[head];
            head = (head + 1) % capacity;
            size--;
            return item;
        }

        public T Peek()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Buffer is empty");
            }

            return buffer[head];
        }

        public void clear()
        {
            head = 0;
            tail = 0;
            size = 0;
        }

        public void getContentHere(StringBuilder stringBuilder)
        {
            if (tail > head)
            {
                int i = head;

                while (i < tail)
                {
                    stringBuilder.Append(buffer[i]);

                    i++;
                }
            }
            else
            {
                int i = head;

                while(i < capacity)
                {
                    stringBuilder.Append(buffer[i]);

                    i++;
                }

                i = 0;
                while( i < tail)
                {
                    stringBuilder.Append(buffer[i]);

                    i++;
                }
            }
        }

        public void copyBufferHere( T[] target, uint limit, out uint count )
        {
            uint index = 0;

            if (tail > head)
            {
                int i = head;

                while (i < tail && index < limit )
                {
                    target[ index++ ] = buffer[i];

                    i++;
                }
            }
            else
            {
                int i = head;

                while(i < capacity && index < limit )
                {
                    target[ index++] = buffer[i];

                    i++;
                }

                i = 0;
                while( i < tail && index < limit )
                {
                    target[ index++ ] = buffer[i];

                    i++;
                }
            }    
            
            count = index;
        }
    }
}
