using System;
using UnityEngine;

public class ItemManagerView : MonoBehaviour
{
    [SerializeField] private ItemManager _itemManager;

    private void Awake()
    {
        _itemManager.SetItemManagerView(this);
    }
}
