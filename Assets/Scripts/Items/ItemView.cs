using System.Collections;
using NaughtyAttributes;
using UnityEngine;

public class ItemView : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private Rigidbody _rigidBody;
    [SerializeField] private float _pickupAbleDelay = 0.1f;
    [SerializeField] [ReadOnly] private Item _item;

    private bool _pickupAble = false;

    public Item Item => _item;
    public Rigidbody RigidBody => _rigidBody;
    public bool PickupAble => _pickupAble;
    public Chunk Chunk { get; private set; }

    public void SetItem(Item item)
    {
        _item = item;
        _meshRenderer.sharedMaterial = item.ItemData.ItemMaterial;
        _meshFilter.sharedMesh = item.ItemData.ItemMesh;
        _pickupAble = false;
        StartCoroutine(IBecomePickupAble());
    }

    public void SetChunk(Chunk chunk)
    {
        Chunk = chunk;
    }

    private IEnumerator IBecomePickupAble()
    {
        yield return new WaitForSeconds(_pickupAbleDelay);
        _pickupAble = true;
    }

    public void SetPickupAble(bool pickupAble)
    {
        _pickupAble = pickupAble;
    }
}
