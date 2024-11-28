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

    private bool _chunkInitialized = false;
    private bool _chunkSpawned = false;
    private ChunkData _chunkData;
    private GameObject _terrain;
    private ObjectView[,] _objects;

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
        _chunkSpawned = true;
    }

    public void DestroyChunk()
    {
        if (!_chunkSpawned)
            return;
        
        Destroy(_terrain);

        SaveObjects();
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
        
        _chunkSpawned = false;
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

    private void SetHeight()
    {
        transform.position = new Vector3(transform.position.x, Data.Height * Chunk.ChunkHeight, transform.position.z);
    }
}
