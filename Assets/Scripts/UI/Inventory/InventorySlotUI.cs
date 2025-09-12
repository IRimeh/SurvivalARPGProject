using System;
using Events;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private PlayerInventoryLocator _playerInventoryLocator;
        [SerializeField] private int _inventorySlotIndex;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _amount;
        [SerializeField] private Transform _hoverTransform;
        
        [Space]
        [SerializeField] private InventorySlotEvent _onLMBInventorySlotEvent;
        [SerializeField] private InventorySlotEvent _onRMBInventorySlotEvent;

        [Space] 
        [SerializeField] private InputActionReference _LMBInputAction;
        [SerializeField] private InputActionReference _RMBInputAction;

        private bool _isMouseOver = false;
        private InventorySlot _inventorySlot;
        
        private void Start()
        {
            _inventorySlot = _playerInventoryLocator.Value.Inventory[_inventorySlotIndex];
            UpdateInventorySlotUI(_inventorySlot);
            
            _inventorySlot.OnInventorySlotUpdated += UpdateInventorySlotUI;
            _LMBInputAction.action.performed += OnLMBInputAction;
            _RMBInputAction.action.performed += OnRMBInputAction;
        }

        private void OnDestroy()
        {
            _inventorySlot.OnInventorySlotUpdated -= UpdateInventorySlotUI;
            _LMBInputAction.action.performed -= OnLMBInputAction;
            _RMBInputAction.action.performed -= OnRMBInputAction;
        }

        private void OnDisable()
        {
            OnPointerExit(null);
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hoverTransform.gameObject.SetActive(true);
            _isMouseOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hoverTransform.gameObject.SetActive(false);
            _isMouseOver = false;
        }
        
        private void OnLMBInputAction(InputAction.CallbackContext obj)
        {
            if (!_isMouseOver)
                return;

            _onLMBInventorySlotEvent.Invoke(_inventorySlot);
        }
        
        private void OnRMBInputAction(InputAction.CallbackContext obj)
        {
            if (!_isMouseOver)
                return;
            
            _onRMBInventorySlotEvent.Invoke(_inventorySlot);
        }
    }
}
