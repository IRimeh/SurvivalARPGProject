using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

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
    private List<int> _runningAsyncOperations = new List<int>();
    private TaskCompletionSource<bool> _isRunningAsyncOperations = new TaskCompletionSource<bool>();


    public bool ChunkSpawned => _chunkSpawned;
    public ChunkData Data => _chunkData;


    private bool _hasSetCompleted = false;
    

    public void InitChunk(ChunkData chunkData)
    {
        _isRunningAsyncOperations = new TaskCompletionSource<bool>();
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
        _hasSetCompleted = false;
        
        SpawnWalls();
        LoadObjects();
        LoadItems();
        _chunkSpawned = true;
    }

    public void DestroyChunk()
    {
        if (!_chunkSpawned)
            return;
        
        DestroyChunkAsync();
    }

    private async void DestroyChunkAsync()
    {
        if (_runningAsyncOperations.Count > 0)
            await _isRunningAsyncOperations.Task;

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
                    SpawnObjectViewAsync(data.Prefab, transform, data, this, new Vector2Int(x, y));
                }
                else if (rand > 20)
                {
                    ObjectData data = _objectDataLibrary.GetObjectFromID(0);
                    SpawnObjectViewAsync(data.Prefab, transform, data, this, new Vector2Int(x, y));
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
        
        SpawnGameObjectAsync(prefab, transform.position, rot, transform, spawnedTerrain =>
        {
            _terrain = spawnedTerrain;
        });
    }

    private void LoadObjects()
    {
        for (int i = 0; i < _chunkData.Objects.Count; i++)
        {
            ObjectSaveData objectSaveData = _chunkData.Objects[i];
            ObjectData data = _objectDataLibrary.GetObjectFromID(objectSaveData.ID);
            SpawnObjectViewAsync(data.Prefab, transform, data, this, objectSaveData.Pos, objectView =>
            {
                objectView.ApplyObjectSaveData(objectSaveData);
            });
        }
    }
    
    
    
    private async void SpawnObjectViewAsync(ObjectView toSpawn, Transform parent, ObjectData objectData, Chunk chunk, Vector2Int relativePos, Action<ObjectView> onObjectViewInstantiated = null)
    {
        var asyncInstantiation = AsyncInstantiation();
        int hashCode = asyncInstantiation.GetHashCode();
        AddAsyncOperation(hashCode);
        
        var asyncResult = await asyncInstantiation;
        
        ObjectView spawnedObjectView = asyncResult[0];
        spawnedObjectView.InitObjectData(objectData, chunk, relativePos);
        _objects[relativePos.x, relativePos.y] = spawnedObjectView;
        onObjectViewInstantiated?.Invoke(spawnedObjectView);
        
        RemoveAsyncOperation(hashCode);
        
        AsyncInstantiateOperation<ObjectView> AsyncInstantiation()
        {
            var result = InstantiateAsync(toSpawn, parent);
            return result;
        }
    }

    private async void SpawnGameObjectAsync(GameObject toSpawn, Vector3 position, Quaternion rotation, Transform parent, Action<GameObject> onGameObjectInstantiated = null)
    {
        var asyncInstantiation = AsyncInstantiation();
        int hashCode = asyncInstantiation.GetHashCode();
        AddAsyncOperation(hashCode);
        
        var asyncResult = await asyncInstantiation;
        
        GameObject spawnedGameObject = asyncResult[0];
        onGameObjectInstantiated?.Invoke(spawnedGameObject);
        
        RemoveAsyncOperation(hashCode);
        
        AsyncInstantiateOperation<GameObject> AsyncInstantiation()
        {
            var result = InstantiateAsync(toSpawn, parent, position, rotation);
            return result;
        }
    }
    
    

    private void AddAsyncOperation(int hashCode)
    {
        _runningAsyncOperations.Add(hashCode);
    }

    private void RemoveAsyncOperation(int hashCode)
    {
        int countBeforeAdding = _runningAsyncOperations.Count;
        _runningAsyncOperations.Remove(hashCode);

        if (countBeforeAdding > 0 && _runningAsyncOperations.Count == 0)
        {
            _isRunningAsyncOperations.SetResult(true);
            _isRunningAsyncOperations = new TaskCompletionSource<bool>();
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

    public void RemoveObject(ObjectView objectView)
    {
        _objects[objectView.RelativePos.x, objectView.RelativePos.y] = null;
    }
}