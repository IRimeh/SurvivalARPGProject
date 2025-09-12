using Events;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private Transform _inventoryParentTransform;
        [SerializeField] private Transform _hotbarParentTransform;
        [SerializeField] private IndividualItemUI _holdingItemUI;
        [SerializeField] private InputActionReference _shiftAction;
        [SerializeField] private InputActionReference _mousePosition;
        [SerializeField] private PlayerInventoryLocator _playerInventoryLocator;
        [SerializeField] private ItemManager _itemManager;

        [Space]
        [SerializeField] private InventorySlotEvent _onLMBInventorySlotEvent;
        [SerializeField] private InventorySlotEvent _onRMBInventorySlotEvent;

        private RectTransform _rectTransform;
        private bool _isInventoryOpen = false;
        
        private Item _holdingItem;
        private bool _isHoldingItem = false;

        private void Start()
        {
            _rectTransform = transform as RectTransform;
            _onLMBInventorySlotEvent.Register(LMBInventorySlot);
            _onRMBInventorySlotEvent.Register(RMBInventorySlot);
        }

        private void OnDestroy()
        {
            _onLMBInventorySlotEvent.Unregister(LMBInventorySlot);
            _onRMBInventorySlotEvent.Unregister(RMBInventorySlot);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if(_isInventoryOpen)
                    CloseInventory();
                else
                    OpenInventory();
            }

            UpdateHoldingItemUIPosition();
        }

        private void OpenInventory()
        {
            _playerInventoryLocator.Value.SetIsUIOpen(true);
            _inventoryParentTransform.gameObject.SetActive(true);
            _hotbarParentTransform.gameObject.SetActive(false);
            _isInventoryOpen = true;
        }

        private void CloseInventory()
        {
            _playerInventoryLocator.Value.SetIsUIOpen(false);
            _inventoryParentTransform.gameObject.SetActive(false);
            _hotbarParentTransform.gameObject.SetActive(true);
            _isInventoryOpen = false;

            if (_isHoldingItem)
            {
                if (!_playerInventoryLocator.Value.TryAddItem(_holdingItem))
                    _itemManager.SpawnItem(_holdingItem, _playerInventoryLocator.Value.transform.position);
                
                _isHoldingItem = false;
                _holdingItem = null;
                UpdateHoldingItemUIState();
            }
        }
        
        
        private void UpdateHoldingItemUIState()
        {
            if (_isHoldingItem)
                _holdingItemUI.Show(_holdingItem);
            else
                _holdingItemUI.Hide();
        }

        private void UpdateHoldingItemUIPosition()
        {
            if (!_isHoldingItem)
                return;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, _mousePosition.action.ReadValue<Vector2>(), Camera.main, out Vector2 localPos);
            _holdingItemUI.RectTransform.anchoredPosition = _mousePosition.action.ReadValue<Vector2>();
        }

        private void LMBInventorySlot(InventorySlot inventorySlot)
        {
            if (!_isHoldingItem)
            {
                _isHoldingItem = inventorySlot.TryTakeItemFromSlot(out _holdingItem);
                UpdateHoldingItemUIState();
                return;
            }
            
            inventorySlot.AddToInventorySlot(_holdingItem, out Item leftOverItem);
            _holdingItem = leftOverItem;
            _isHoldingItem = leftOverItem.Amount > 0;
            UpdateHoldingItemUIState();
        }
        
        private void RMBInventorySlot(InventorySlot inventorySlot)
        {
            if (!_isHoldingItem)
            {
                _isHoldingItem = inventorySlot.TryTakeHalfOfItemsFromSlot(out _holdingItem);
                UpdateHoldingItemUIState();
                return;
            }

            if (_holdingItem.Amount == 1)
            {
                LMBInventorySlot(inventorySlot);
                return;
            }

            Item singleItem = new Item(_holdingItem);
            singleItem.Amount = 1;
            _holdingItem.Amount--;
            inventorySlot.AddToInventorySlot(singleItem, out Item leftOver);
            _holdingItem.Amount += leftOver.Amount;
            UpdateHoldingItemUIState();
        }
    }
}
