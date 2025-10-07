using ItemSpawnableBehaviours;
using UnityEngine;

namespace Items.ItemBehaviours
{
    [CreateAssetMenu(fileName = "SwordBehaviour", menuName = "ScriptableObjects/Items/ItemBehaviours/SwordBehaviour", order = 0)]
    public class SwordBehaviour : ItemBehaviour
    {
        [SerializeField] private float _damageAmount = 10;
        [SerializeField] private DamageCollider _swordDamageCollider;
        
        public override void OnLeftClick(ItemBehaviourDTO itemBehaviourDTO)
        {
            DamageCollider damageCollider = Instantiate(_swordDamageCollider, itemBehaviourDTO.PlayerAlignedSpawnParent);
            damageCollider.SetDamage(_damageAmount);
        }
    }
}
