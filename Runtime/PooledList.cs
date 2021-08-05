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
    public class PooledList<T> : List<T>, IDisposable
    {
        private static ObjectPool<PooledList<T>> s_pool;

        public static PooledList<T> From(IList<T> source)
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
            if (s_pool == null) s_pool = new ObjectPool<PooledList<T>>(CreateNew);
            var list = s_pool.Claim();
            return list;
        }

        private static PooledList<T> CreateNew()
        {
            var result = new PooledList<T>();
            return result;
        }

        private byte _version = 0;

        /// <summary>
        /// Pooled object version incremented each time the object is released
        /// back into its pool (with overflow back to 0). Can be used to check
        /// that a reference to a pooled object is still valid.
        /// </summary>
        public byte Version { get { return _version; } }

        public void Dispose()
        {
            unchecked { _version++; }
            Clear();
            s_pool.Release(this);
        }
    }

    public static class PooledList
    {
        /// <summary>
        /// Claim a pooled list, filling it with values from an existing list.
        /// </summary>
        public static PooledList<T> From<T>(IList<T> source)
        {
            return PooledList<T>.From(source);
        }
    }
}
