using UnityEngine;

namespace Items.ItemBehaviours
{
    public abstract class ItemBehaviour : ScriptableObject
    {
        public struct ItemBehaviourDTO
        {
            public Transform StaticSpawnParent;
            public Transform PlayerAlignedSpawnParent;
            public Transform MouseDirectionAlignedSpawnParent;
        }
        
        public virtual void OnLeftClick(ItemBehaviourDTO itemBehaviourDTO) { }
        public virtual void OnRightClick(ItemBehaviourDTO itemBehaviourDTO) { }
        public virtual void OnQPress(ItemBehaviourDTO itemBehaviourDTO) { }
        public virtual void OnFPress(ItemBehaviourDTO itemBehaviourDTO) { }
    }
}
