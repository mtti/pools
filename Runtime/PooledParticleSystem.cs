using UnityEngine;

namespace mtti.Pools
{
    /// <summary>
    /// When added to a GameObject with both a ParticleSystem and PooledObject
    /// components, releases the GameObject back into PooledObject's pool when
    /// the particle system finishes playing.
    /// </summary>
    public class PooledParticleSystem : MonoBehaviour
    {
        private Transform _transform;

        private ParticleSystem _particleSystem;

        public void Initialize(Vector3 position, Vector3 direction)
        {
            _transform.position = position;
            _transform.rotation = Quaternion.LookRotation(direction);
            this.gameObject.SetActive(true);
        }

        private void Release()
        {
            GetComponent<PooledObject>().Release();
        }

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void Start()
        {
            _particleSystem.Play();
        }

        private void Update()
        {
            if (!_particleSystem.IsAlive())
            {
                Release();
            }
        }
    }
}
