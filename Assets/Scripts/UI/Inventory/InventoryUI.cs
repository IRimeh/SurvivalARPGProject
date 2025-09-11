using UnityEngine;

namespace UI.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private Transform _inventoryParentTransform;
        [SerializeField] private Transform _hotbarParentTransform;
        
        private bool _isInventoryOpen = false;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if(_isInventoryOpen)
                    CloseInventory();
                else
                    OpenInventory();
            }
        }

        private void OpenInventory()
        {
            _inventoryParentTransform.gameObject.SetActive(true);
            _hotbarParentTransform.gameObject.SetActive(false);
            _isInventoryOpen = true;
        }

        private void CloseInventory()
        {
            _inventoryParentTransform.gameObject.SetActive(false);
            _hotbarParentTransform.gameObject.SetActive(true);
            _isInventoryOpen = false;
        }
    }
}
