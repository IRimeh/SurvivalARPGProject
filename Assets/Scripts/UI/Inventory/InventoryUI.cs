using System.Collections.Generic;
using Events;
using Locators;
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
        [SerializeField] private PlayerInventoryLocator _playerInventoryLocator;
        [SerializeField] private PlayerControllerLocator _playerControllerLocator;
        [SerializeField] private ItemManager _itemManager;
        [SerializeField] private List<InventorySlotUI> _hotbarInventorySlotUI;

        [Header("Events")]
        [SerializeField] private InventorySlotEvent _onLMBInventorySlotEvent;
        [SerializeField] private InventorySlotEvent _onRMBInventorySlotEvent;
        
        [Header("Input")]
        [SerializeField] private InputActionReference _shiftAction;
        [SerializeField] private InputActionReference _controlAction;
        [SerializeField] private InputActionReference _mousePosition;
        [SerializeField] private InputActionReference _scrollAction;
        [SerializeField] private InputActionReference _action1;
        [SerializeField] private InputActionReference _action2;
        [SerializeField] private InputActionReference _action3;
        [SerializeField] private InputActionReference _action4;
        [SerializeField] private InputActionReference _action5;
        [SerializeField] private InputActionReference _action6;
        [SerializeField] private InputActionReference _action7;
        [SerializeField] private InputActionReference _action8;
        [SerializeField] private InputActionReference _action9;

        private RectTransform _rectTransform;
        private bool _isInventoryOpen = false;
        
        private Item _holdingItem;
        private bool _isHoldingItem = false;

        private InventorySlot _selectedHotbarSlot = null;
        private int _selectedHotbarIndex = -1;

        private void Start()
        {
            _rectTransform = transform as RectTransform;
            _onLMBInventorySlotEvent.Register(LMBInventorySlot);
            _onRMBInventorySlotEvent.Register(RMBInventorySlot);
            
            _scrollAction.action.performed += OnScrollActionPerformed;
            _action1.action.performed += OnNumberKeyPerformed;
            _action2.action.performed += OnNumberKeyPerformed;
            _action3.action.performed += OnNumberKeyPerformed;
            _action4.action.performed += OnNumberKeyPerformed;
            _action5.action.performed += OnNumberKeyPerformed;
            _action6.action.performed += OnNumberKeyPerformed;
            _action7.action.performed += OnNumberKeyPerformed;
            _action8.action.performed += OnNumberKeyPerformed;
            _action9.action.performed += OnNumberKeyPerformed;
            
            SelectHotbarSlot(0);
        }

        private void OnDestroy()
        {
            _onLMBInventorySlotEvent.Unregister(LMBInventorySlot);
            _onRMBInventorySlotEvent.Unregister(RMBInventorySlot);
            
            _scrollAction.action.performed -= OnScrollActionPerformed;
            _action1.action.performed -= OnNumberKeyPerformed;
            _action2.action.performed -= OnNumberKeyPerformed;
            _action3.action.performed -= OnNumberKeyPerformed;
            _action4.action.performed -= OnNumberKeyPerformed;
            _action5.action.performed -= OnNumberKeyPerformed;
            _action6.action.performed -= OnNumberKeyPerformed;
            _action7.action.performed -= OnNumberKeyPerformed;
            _action8.action.performed -= OnNumberKeyPerformed;
            _action9.action.performed -= OnNumberKeyPerformed;
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

        
        
        private void OnScrollActionPerformed(InputAction.CallbackContext obj)
        {
            if (_controlAction.action.IsPressed())
                return;
            
            var inventory = _playerInventoryLocator.Value;
            float scroll = _scrollAction.action.ReadValue<Vector2>().y;
            int newHotbarIndex = _selectedHotbarIndex - (int)Mathf.Sign(scroll);
            
            if (newHotbarIndex < 0)
                newHotbarIndex = inventory.HotbarSize - 1;

            if (newHotbarIndex >= inventory.HotbarSize)
                newHotbarIndex = 0;

            SelectHotbarSlot(newHotbarIndex);
        }

        private void OnNumberKeyPerformed(InputAction.CallbackContext obj)
        {
            int key = int.Parse(obj.action.name);
            SelectHotbarSlot(key - 1);
        }

        private void SelectHotbarSlot(int hotbarSlotIndex)
        {
            if (_selectedHotbarIndex == hotbarSlotIndex)
                return;

            if (_selectedHotbarSlot != null)
                _selectedHotbarSlot.OnInventorySlotUpdated -= OnSelectedHotbarSlotUpdated;
            
            if(_selectedHotbarIndex >= 0)
                _hotbarInventorySlotUI[_selectedHotbarIndex].Select(false);
            
            _selectedHotbarIndex = hotbarSlotIndex;
            _selectedHotbarSlot = _playerInventoryLocator.Value.Inventory[hotbarSlotIndex];
            _selectedHotbarSlot.OnInventorySlotUpdated += OnSelectedHotbarSlotUpdated;
            _hotbarInventorySlotUI[hotbarSlotIndex].Select(true);
            OnSelectedHotbarSlotUpdated(_selectedHotbarSlot);
        }

        private void OnSelectedHotbarSlotUpdated(InventorySlot inventorySlot)
        {
            _playerControllerLocator.Value.SetHoldingItem(inventorySlot);
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
                if (_shiftAction.action.IsPressed())
                {
                    ShiftLMBInventorySlot(inventorySlot);
                }
                else
                {
                    _isHoldingItem = inventorySlot.TryTakeItemFromSlot(out _holdingItem);
                    UpdateHoldingItemUIState();
                }
                return;
            }
            
            inventorySlot.AddToInventorySlot(_holdingItem, out Item leftOverItem);
            _holdingItem = leftOverItem;
            _isHoldingItem = leftOverItem.Amount > 0;
            UpdateHoldingItemUIState();
        }

        private void ShiftLMBInventorySlot(InventorySlot inventorySlot)
        {
            var inventory = _playerInventoryLocator.Value;
            if (inventorySlot.TryTakeItemFromSlot(out Item item))
            {
                if (inventory.IsInventorySlotPartOfHotbar(inventorySlot))
                    inventory.TryAddItem(item, inventory.HotbarSize);
                else
                    inventory.TryAddItem(item);
            }
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
