/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.

***************************************************************************/

namespace Puppet.ParserGenerator
{
    public class ParserStack<T>
    {
        public T[] array = new T[1];
        public int top = 0;


        public void Push(T value)
        {
            if (top >= array.Length)
            {
                T[] newarray = new T[array.Length * 2];
                System.Array.Copy(array, newarray, top);
                array = newarray;
            }
            array[top++] = value;
        }

        public T Pop()
        {
            return array[--top];
        }

        public T Top()
        {
            return array[top - 1];
        }

        public bool IsEmpty()
        {
            return top == 0;
        }
    }
}