/*
Copyright 2017-2021 Matti Hiltunen (https://www.mattihiltunen.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Generic;

namespace mtti.Pools
{
    /// <summary>
    /// A wrapper around <see cref="System.Collections.Generic.List{T}"/>
    /// which retrieves the list from an object pool and clears it and then
    /// returns it to the pool when the <c>PooledList</c> instance is disposed.
    /// </summary>
    /// <remarks>
    /// Implements <see cref="System.Collections.Generic.ICollection{T}"/>
    /// by proxying the calls to the underlying
    /// <see cref="System.Collections.Generic.List{T}"/> instance.
    /// </remarks>
    public class PooledList<T> : IDisposable, ICollection<T>
    {
        public static PooledList<T> Copy(IList<T> source)
        {
            var result = Claim();
            for (int i = 0, count = source.Count; i < count; i++)
            {
                result.Add(source[i]);
            }
            return result;
        }

        public static PooledList<T> Claim()
        {
            return Claim(ObjectPool<List<T>>.Instance);
        }

        public static PooledList<T> Claim(ObjectPool<List<T>> pool)
        {
            var value = pool.Claim();
            var obj = ObjectPool<PooledList<T>>.Instance.Claim();
            obj.Initialize(value, pool);
            return obj;
        }

        protected List<T> _value;

        private ObjectPool<List<T>> _pool;

        public List<T> Values { get { return _value; } }

        public T this[int key]
        {
            get { return _value[key]; }
        }

        public int Count { get { return _value.Count; } }

        public bool IsReadOnly { get { return false; } }

        public void Add(T value) { _value.Add(value); }

        public void Clear() { _value.Clear(); }

        public bool Contains(T value) { return _value.Contains(value); }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _value.CopyTo(array, arrayIndex);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _value.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _value.GetEnumerator();
        }

        public bool Remove(T value)
        {
            return _value.Remove(value);
        }

        public virtual void Dispose()
        {
            _value.Clear();
            _pool.Release(_value);
            _value = null;
            _pool = null;
            ObjectPool<PooledList<T>>.Instance.Release(this);
        }

        private void Initialize(List<T> value, ObjectPool<List<T>> pool)
        {
            _value = value;
            _pool = pool;
        }
    }
}
