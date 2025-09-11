using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private PlayerInventoryLocator _playerInventoryLocator;
        [SerializeField] private int _inventorySlotIndex;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _amount;

        private InventorySlot _inventorySlot;

        private void Start()
        {
            _inventorySlot = _playerInventoryLocator.Value.Inventory[_inventorySlotIndex];
            UpdateInventorySlotUI(_inventorySlot);
            _inventorySlot.OnInventorySlotUpdated += UpdateInventorySlotUI;
        }

        private void OnDestroy()
        {
            _inventorySlot.OnInventorySlotUpdated -= UpdateInventorySlotUI;
        }

        private void UpdateInventorySlotUI(InventorySlot inventorySlot)
        {
            _iconImage.gameObject.SetActive(inventorySlot.HasItem);
            _amount.gameObject.SetActive(inventorySlot.HasItem);

            if (!inventorySlot.HasItem)
                return;
            
            _iconImage.sprite = inventorySlot.Item.ItemData.Sprite;
            _amount.text = inventorySlot.Item.Amount.ToString();
        }
    }
}
