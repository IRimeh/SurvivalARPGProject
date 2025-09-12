using Player;
using UnityEngine;

namespace Events
{
    [CreateAssetMenu(fileName = "InventorySlotEvent", menuName = "ScriptableObjects/Events/InventorySlotEvent")]
    public class InventorySlotEvent : ParameterEvent<InventorySlot>
    {

    }
}
