using UnityEngine;

namespace mtti.Pools
{
    /// <summary>
    /// A specialized <see cref="mtti.Pools.GameObjectPool" /> for particle
    /// systems. Every created GameObject instance is given a
    /// PooledParticleSystem component which automatically releases the
    /// GameObject back into the pool after the particle system has finished
    /// playing.
    /// </summary>
    public class ParticleSystemPool : GameObjectPool
    {
        public ParticleSystemPool(
            ParticleSystem prefab
        ) : base(prefab.gameObject) { }

        protected override GameObject Create()
        {
            GameObject obj = GameObject.Instantiate(_prefab.gameObject);
            obj.AddComponent<PooledParticleSystem>();
            return obj;
        }
    }
}
