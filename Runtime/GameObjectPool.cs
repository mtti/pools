/*
Copyright 2017-2020 Matti Hiltunen

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
using UnityEngine;

namespace mtti.Pools
{
    public delegate GameObject GameObjectPoolDelegate();

    public interface IGameObjectFactory
    {
        GameObject CreateGameObject();
    }

    /// <summary>
    /// Object pool for Unity GameObjects.
    /// </summary>
    public class GameObjectPool
    {
        protected GameObject _prefab = null;

        private Queue<GameObject> _pool = new Queue<GameObject>();

        private GameObjectPoolDelegate _factoryDelegate;

        private IGameObjectFactory _factoryObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="mtti.Gadgets.GameObjectPool"/> class with
        /// a factory method which will be called every time the pool needs to create a new object.
        /// </summary>
        /// <param name="factory">Callback to create new GameObjects when the pool is empty.</param>
        public GameObjectPool(GameObjectPoolDelegate factory)
        {
            _factoryDelegate = factory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="mtti.Gadgets.GameObjectPool"/> class with
        /// a prefab, which will be cloned every time the pool needs to create a new object.
        /// </summary>
        /// <param name="prefab">Prefab.</param>
        public GameObjectPool(GameObject prefab)
        {
            _prefab = prefab;
            _factoryDelegate = InstantiatePrefab;
        }

        public GameObjectPool(IGameObjectFactory factory)
        {
            _factoryObject = factory;
            _factoryDelegate = InstantiateWithFactory;
        }

        /// <summary>
        /// Retrieve a GameObject from the pool, or create a new one using the factory delegate if
        /// the pool is empty.
        /// </summary>
        public GameObject Claim()
        {
            GameObject obj = null;

            // Destroyed GameObjects will equal to null, so keep trying until we get a non-null
            // object or the pool runs out.
            while (_pool.Count > 0 && obj == null)
            {
                obj = _pool.Dequeue();
            }

            PooledObject pooledObject;

            // If we don't have a valid GameObject by now, create one.
            if (obj == null)
            {
                obj = Create();

                pooledObject = obj.GetComponent<PooledObject>();
                if (pooledObject == null)
                {
                    pooledObject = obj.AddComponent<PooledObject>();
                }
                pooledObject.Pool = this;
            }

            obj.SetActive(false);

            pooledObject = obj.GetComponent<PooledObject>();
            if (pooledObject != null)
            {
                pooledObject.OnClaimedFromPool();
            }

            return obj;
        }

        public T Claim<T>() where T : Component
        {
            GameObject obj = Claim();
            if (obj == null)
            {
                return null;
            }

            return obj.GetComponent<T>();
        }

        /// <summary>
        /// Destroy all GameObjects currently in the pool.
        /// </summary>
        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                if (obj == null)
                {
                    continue;
                }
                UnityEngine.Object.Destroy(obj);
            }
        }

        /// <summary>
        /// Release a GameObject back into this pool.
        /// </summary>
        /// <param name="obj">The GameObject to release.</param>
        internal void Release(GameObject obj)
        {
            obj.SetActive(false);

            var pooledObject = obj.GetComponent<PooledObject>();
            if (pooledObject != null)
            {
                pooledObject.OnReleasedToPool();
            }

            _pool.Enqueue(obj);
        }

        protected virtual GameObject Create()
        {
            return _factoryDelegate();
        }

        /// <summary>
        /// Used in place of a user-provided factory method when a prefab is provided instead.
        /// </summary>
        /// <returns>A copy of the prefab.</returns>
        private GameObject InstantiatePrefab()
        {
            return GameObject.Instantiate(_prefab);
        }

        private GameObject InstantiateWithFactory()
        {
            return _factoryObject.CreateGameObject();
        }
    }
}
