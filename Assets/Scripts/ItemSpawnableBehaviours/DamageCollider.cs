using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemSpawnableBehaviours
{
    public class DamageCollider : MonoBehaviour
    {
        [SerializeField] private float _aliveTime = 1;

        private float _damage = 0;
        private List<Collider> _alreadyInteractedColliders = new();

        private void Awake()
        {
            StartCoroutine(DestroyAfterSeconds(_aliveTime));
        }

        public void SetDamage(float damage)
        {
            _damage = damage;
            _alreadyInteractedColliders.Clear();
        }

        private IEnumerator DestroyAfterSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_alreadyInteractedColliders.Contains(other))
                return;
            
            Debug.Log($"Collided with: {other.gameObject.name}");

            if (other.TryGetComponent(out ObjectView objectView))
            {
                DamageObjectView(objectView, other);
                return;
            }
            if (other.transform.parent != null && other.transform.parent.TryGetComponent(out objectView))
            {
                DamageObjectView(objectView, other);
                return;
            }
        }
        
        private void DamageObjectView(ObjectView objectView, Collider otherCollider)
        {
            if (objectView.TryGetObjectBehaviour(out HealthObjectBehaviour healthObjectBehaviour))
            {
                Debug.Log($"Dealt {_damage} damage to: {objectView.gameObject.name}");
                healthObjectBehaviour.TakeDamage(_damage);
            }
            _alreadyInteractedColliders.Add(otherCollider);
        }
    }
}
