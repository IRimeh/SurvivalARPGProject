using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class IndividualItemUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _shadowIconImage;
        [SerializeField] private TextMeshProUGUI _amountText;
        
        private InventorySlot _inventorySlot;
        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            RectTransform = transform as RectTransform;
        }

        public void Show(Item item)
        {
            _canvasGroup.alpha = 1;
            _iconImage.sprite = item.ItemData.Sprite;
            _shadowIconImage.sprite = item.ItemData.Sprite;
            _amountText.SetText(item.Amount.ToString());
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0;
        }
    }
}
