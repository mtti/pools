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
    public interface IPooledObjectListener
    {
        void OnReleasedToPool(PooledObject obj);

        void OnClaimedFromPool(PooledObject obj);
    }

    /// <summary>
    /// Do not add this component to GameObjects yourself. This is added automatically to all
    /// GameObjects created with <see cref="mtti.Pools.GameObjectPool"/> to allow them to be
    /// released back into the same object pool they were created with by caling
    /// <c>yourGameObject.GetComponent<mtti.Gadgets.PooledObject>().Release()</c>.
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        /// <summary>
        /// Convenience method for releasing pooled GameObjects. If the object has a PooledObject
        /// component, it is released using that component. If not, the object is simply destroyed,
        /// immediately in edit mode and normally using GameObject.Destroy during play mode.
        /// Calls with a null parameter are ignored.
        /// </summary>
        public static void Release(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            PooledObject pooledObject = obj.GetComponent<PooledObject>();
            if (pooledObject != null)
            {
                pooledObject.Release();
                return;
            }

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                GameObject.DestroyImmediate(obj);
                return;
            }
#endif

            GameObject.Destroy(obj);
        }

        /// <summary>
        /// Release the parent GameObject of a Unity component.
        /// </summary>
        public static void Release(Component component)
        {
            if (component == null)
            {
                return;
            }
            Release(component.gameObject);
        }

        internal GameObjectPool Pool;

        private List<IPooledObjectListener> _listeners = null;

        public void Release()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                GameObject.DestroyImmediate(this.gameObject);
                return;
            }
#endif

            if (Pool == null)
            {
                this.gameObject.SetActive(false);
                GameObject.Destroy(this.gameObject);
            }
            else
            {
                Pool.Release(this.gameObject);
            }
        }

        public void AddListener(IPooledObjectListener listener)
        {
            if (_listeners != null && _listeners.Contains(listener))
            {
                return;
            }

            if (_listeners == null)
            {
                _listeners = new List<IPooledObjectListener>();
            }

            _listeners.Add(listener);
        }

        public void RemoveListener(IPooledObjectListener listener)
        {
            if (_listeners == null || !_listeners.Contains(listener))
            {
                return;
            }
            _listeners.Remove(listener);
        }

        /// <summary>
        /// Raise the released to pool event. Called by <see cref="mtti.Gadgets.GameObjectPool"/>.
        /// </summary>
        internal void OnReleasedToPool()
        {
            if (_listeners == null)
            {
                return;
            }
            for (int i = 0, count = _listeners.Count; i < count; i++)
            {
                _listeners[i].OnReleasedToPool(this);
            }
        }

        /// <summary>
        /// Raise the claimed from pool event. Called by <see cref="mtti.Gadgets.GameObjectPool"/>.
        /// </summary>
        internal void OnClaimedFromPool()
        {
            if (_listeners == null)
            {
                return;
            }
            for (int i = 0, count = _listeners.Count; i < count; i++)
            {
                _listeners[i].OnClaimedFromPool(this);
            }
        }
    }
}
