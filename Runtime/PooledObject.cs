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
using UnityEngine;

namespace mtti.Pools
{
    public class PooledObject<T> : IDisposable
        where T : class, new()
    {
        public static void Release(GameObject obj)
        {
            PooledGameObject.Release(obj);
        }

        public static void Release(Component obj)
        {
            PooledGameObject.Release(obj);
        }

        public static PooledObject<T> Claim()
        {
            return Claim(ObjectPool<T>.Instance);
        }

        public static PooledObject<T> Claim(ObjectPool<T> pool)
        {
            var value = pool.Claim();
            var obj = ObjectPool<PooledObject<T>>.Instance.Claim();
            obj.Initialize(value, pool);
            return obj;
        }

        protected T _value;

        private ObjectPool<T> _pool;

        public virtual void Dispose()
        {
            OnDispose();
            _pool.Release(_value);
            _value = null;
            _pool = null;
            ObjectPool<PooledObject<T>>.Instance.Release(this);
        }

        protected virtual void OnDispose() { }

        private void Initialize(T value, ObjectPool<T> pool)
        {
            _value = value;
            _pool = pool;
        }
    }
}
