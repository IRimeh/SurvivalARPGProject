using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public static int ChunkSize = 25;
    public static int ChunkHeight = 10;
    public static int ObjectsRowCount = 5;

    [SerializeField] 
    private ChunkPrefabLibrary _chunkPrefabLibrary;
    [SerializeField] 
    private ObjectDataLibrary _objectDataLibrary;
    [SerializeField] 
    private ItemManager _itemManager;
    [SerializeField] 
    private ItemDataLibrary _itemDataLibrary;

    private bool _chunkInitialized = false;
    private bool _chunkSpawned = false;
    private ChunkData _chunkData;
    private GameObject _terrain;
    private ObjectView[,] _objects;
    private List<ItemView> _items = new List<ItemView>();

    public bool ChunkSpawned => _chunkSpawned;
    public ChunkData Data => _chunkData;
    

    public void InitChunk(ChunkData chunkData)
    {
        _objects = new ObjectView[ObjectsRowCount, ObjectsRowCount];
        _chunkData = chunkData;
        SetHeight();
        _chunkInitialized = true;
    }

    public void GenerateChunk()
    {
        GenerateObjects();
    }

    public void SpawnChunk()
    {
        SpawnWalls();
        LoadObjects();
        LoadItems();
        _chunkSpawned = true;
    }

    public void DestroyChunk()
    {
        if (!_chunkSpawned)
            return;
        
        Destroy(_terrain);

        SaveObjects();
        SaveItems();
        DestroyObjects();
        DestroyItems();
        
        _chunkSpawned = false;
    }

    private void DestroyObjects()
    {
        for (int y = _objects.GetLength(1) - 1; y >= 0; y--)
        {
            for (int x = _objects.GetLength(0) - 1; x >= 0; x--)
            {
                if (_objects[x, y] != null)
                {
                    Destroy(_objects[x, y].gameObject);
                    _objects[x, y] = null;
                }
            }
        }
    }

    private void DestroyItems()
    {
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            _itemManager.ReturnToPool(_items[i]);
        }
        _items.Clear();
    }

    private void GenerateObjects()
    {
        for (int y = 0; y < _objects.GetLength(1); y++)
        {
            for (int x = 0; x < _objects.GetLength(0); x++)
            {
                float rand = Random.Range(0, 24);
                if (rand > 22)
                {
                    if(Random.Range(0.0f, 1.0f) > .4f)
                        continue;

                    if (_chunkData.NeighborForwardHeight > _chunkData.Height ||
                        _chunkData.NeighborBackHeight > _chunkData.Height ||
                        _chunkData.NeighborRightHeight > _chunkData.Height ||
                        _chunkData.NeighborLeftHeight > _chunkData.Height)
                        continue;
                    
                    ObjectData data = _objectDataLibrary.GetObjectFromID(1);
                    ObjectView treeView = Instantiate(_objectDataLibrary.GetObjectFromID(1).Prefab, transform);
                    treeView.InitObjectData(data, this, new Vector2Int(x, y));
                    _objects[x, y] = treeView;
                }
                else if (rand > 20)
                {
                    ObjectData data = _objectDataLibrary.GetObjectFromID(0);
                    ObjectView rockView = Instantiate(_objectDataLibrary.GetObjectFromID(0).Prefab, transform);
                    rockView.InitObjectData(data, this, new Vector2Int(x, y));
                    _objects[x, y] = rockView;
                }
            }
        }
    }

    private void SpawnWalls()
    {
        bool wallRight = Data.NeighborRightHeight < Data.Height;
        bool wallLeft = Data.NeighborLeftHeight < Data.Height;
        bool wallFwd = Data.NeighborForwardHeight < Data.Height;
        bool wallBack = Data.NeighborBackHeight < Data.Height;
        int wallCount = (wallRight ? 1 : 0) + 
                        (wallLeft ? 1 : 0) +
                        (wallFwd ? 1 : 0) +
                        (wallBack ? 1 : 0);
        
        GameObject prefab = _chunkPrefabLibrary.Walls0Prefab;
        Quaternion rot = Quaternion.identity;
        switch (wallCount)
        {
            case 1:
                prefab = _chunkPrefabLibrary.Walls1Prefab;
                if (wallBack)
                    rot = Quaternion.Euler(0, 90, 0);
                else if(wallLeft)
                    rot = Quaternion.Euler(0, 180, 0);
                else if(wallFwd)
                    rot = Quaternion.Euler(0, 270, 0);
                break;
            case 2:
                if ((wallLeft && wallRight) || (wallFwd && wallBack))
                {
                    prefab = _chunkPrefabLibrary.Walls2OppositePrefab;
                    if(wallFwd || wallBack)
                        rot = Quaternion.Euler(0, 90, 0); 
                }
                else
                {
                    prefab = _chunkPrefabLibrary.Walls2Prefab;
                    if (wallRight && wallBack)
                        rot = Quaternion.Euler(0, 90, 0);
                    else if(wallBack && wallLeft)
                        rot = Quaternion.Euler(0, 180, 0);
                    else if(wallLeft && wallFwd)
                        rot = Quaternion.Euler(0, 270, 0);
                }
                break;
            case 3:
                prefab = _chunkPrefabLibrary.Walls3Prefab;
                if (!wallBack)
                    rot = Quaternion.Euler(0, 90, 0);
                else if(!wallLeft)
                    rot = Quaternion.Euler(0, 180, 0);
                else if(!wallFwd)
                    rot = Quaternion.Euler(0, 270, 0);
                break;
            case 4:
                prefab = _chunkPrefabLibrary.Walls4Prefab;
                break;
            default:
                prefab = _chunkPrefabLibrary.Walls0Prefab;
                break;
        }
        _terrain = Instantiate(prefab, transform.position, rot, transform);
    }

    private void LoadObjects()
    {
        for (int i = 0; i < _chunkData.Objects.Count; i++)
        {
            ObjectSaveData objectSaveData = _chunkData.Objects[i];
            ObjectData data = _objectDataLibrary.GetObjectFromID(objectSaveData.ID);
            ObjectView view = Instantiate(data.Prefab, transform);
            view.InitObjectData(data, this, objectSaveData.Pos);
            view.ApplyObjectSaveData(objectSaveData);
            _objects[objectSaveData.Pos.x, objectSaveData.Pos.y] = view;
        }
    }

    private void LoadItems()
    {
        for (int i = 0; i < _chunkData.Items.Count; i++)
        {
            ItemSaveData itemSaveData = _chunkData.Items[i];
            _itemManager.SpawnItem(
                _itemDataLibrary.GetItemFromID(itemSaveData.ID),
                itemSaveData.Amount, 
                itemSaveData.Position, 
                0,
                this);
        }
    }

    public void SaveObjects()
    {
        _chunkData.Objects.Clear();
        for (int y = 0; y < _objects.GetLength(1); y++)
        {
            for (int x = 0; x < _objects.GetLength(0); x++)
            {
                if (_objects[x, y] != null)
                {
                    var obj = _objects[x, y];
                    _chunkData.Objects.Add(new ObjectSaveData()
                    {
                        ID = obj.ObjectData.Id,
                        Pos = obj.RelativePos,
                        ObjectBehaviourSaveDatas = obj.GetObjectBehaviourSaveDatas()
                    });
                }
            }
        }
    }

    public void SaveItems()
    {
        _chunkData.Items.Clear();
        for (int i = 0; i < _items.Count; i++)
        {
            var itemView = _items[i];
            _chunkData.Items.Add(new ItemSaveData()
            {
                ID = itemView.Item.Id,
                Position = itemView.transform.position,
                Amount = itemView.Item.Amount
            });
        }
    }

    private void SetHeight()
    {
        transform.position = new Vector3(transform.position.x, Data.Height * Chunk.ChunkHeight, transform.position.z);
    }

    public void AddItem(ItemView itemView)
    {
        _items.Add(itemView);
    }

    public void RemoveItem(ItemView itemView)
    {
        _items.Remove(itemView);
    }
}
