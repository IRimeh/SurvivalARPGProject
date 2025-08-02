using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldGenerator : MonoBehaviour
{
    public PlayerController _player;
    public Chunk _chunkPrefab;
    public int _worldSize = 10;
    public int _seed = 123456789;
    public bool _refreshSeedOnGeneration = false;
    public bool _generateOnAwake = false;
    public WorldGeneratorLocator _worldGeneratorLocator;

    [Header("Generator Variables")] 
    public float _noiseTiling = 100;
    public int _heightVariations = 3;
    
    private bool _worldGenerated = false;
    private FastNoise _noise;
    private Dictionary<Vector2Int, Chunk> _chunks = new ();
    private Dictionary<Vector2Int, Chunk> _currentLoadedChunks = new ();
    private List<Chunk> _chunksToSpawn = new();

    public int Seed => _seed;
    public Dictionary<Vector2Int, Chunk> Chunks => _chunks;

    private void Awake()
    {
        _worldGeneratorLocator.Assign(this);
        GenerateWorld();
    }

    private void Update()
    {
        Dictionary<Vector2Int, Chunk> _previouslyLoadedChunks = new Dictionary<Vector2Int, Chunk>(_currentLoadedChunks);
        
        for (int z = -_worldSize + 1; z < _worldSize; z++)
        {
            for (int x = -_worldSize + 1; x < _worldSize; x++)
            {
                Vector2Int position = new Vector2Int(x, z);

                if (position.magnitude > _worldSize)
                    continue;
                
                Vector3 playerPos = _player.transform.position / Chunk.ChunkSize;
                Vector2Int playerOffset = new Vector2Int(Mathf.RoundToInt(playerPos.x), Mathf.RoundToInt(playerPos.z));
                position += playerOffset;
                    
                if (_chunks.TryGetValue(position, out Chunk chunk))
                {
                    if (!chunk.ChunkSpawned)
                    {
                        chunk.SpawnChunk();
                        _currentLoadedChunks.Add(position, chunk);
                    }
                }
                else 
                    GenerateChunk(position);

                _previouslyLoadedChunks.Remove(position);
            }
        }

        foreach (KeyValuePair<Vector2Int,Chunk> outOfRangeChunk in _previouslyLoadedChunks)
        {
            outOfRangeChunk.Value.DestroyChunk();
            _currentLoadedChunks.Remove(outOfRangeChunk.Key);
        }
    }

    public bool TryGetChunkAtWorldPosition(Vector3 position, out Chunk chunk)
    {
        Vector3 playerPos = _player.transform.position / Chunk.ChunkSize;
        Vector2Int chunkPos = new Vector2Int(Mathf.RoundToInt(playerPos.x), Mathf.RoundToInt(playerPos.z));
        return _chunks.TryGetValue(chunkPos, out chunk);
    }

    [Button]
    private void GenerateWorld()
    {
        if (!Application.isPlaying)
            return;
        
        DestroyWorld();

        GetSeed();

        for (int z = -_worldSize + 1; z < _worldSize; z++)
        {
            for (int x = -_worldSize + 1; x < _worldSize; x++)
            {
                Vector2Int position = new Vector2Int(x, z);
                GenerateChunk(position);
            }
        }

        _worldGenerated = true;
    }

    private void GenerateChunk(Vector2Int pos)
    {
        int height = GetHeight(pos);
                
        ChunkData chunkData = new ChunkData()
        {
            Position = pos,
            Height = height,
            NeighborRightHeight = GetHeight(pos + Vector2Int.right),
            NeighborLeftHeight = GetHeight(pos + Vector2Int.left),
            NeighborForwardHeight = GetHeight(pos + Vector2Int.up),
            NeighborBackHeight = GetHeight(pos + Vector2Int.down),
            Objects = new List<ObjectSaveData>(),
            Items = new List<ItemSaveData>()
        };
                
        Vector3 chunkPos = new Vector3(pos.x * Chunk.ChunkSize, 0, pos.y * Chunk.ChunkSize);
        Chunk chunk = Instantiate(_chunkPrefab, chunkPos, Quaternion.identity, transform);
        chunk.InitChunk(chunkData);
        chunk.SpawnChunk();
        chunk.GenerateChunk();
        _chunks.Add(pos, chunk);
        _currentLoadedChunks.Add(pos, chunk);
    }

    private int GetHeight(Vector2Int pos)
    {
        float noise = _noise.GetPerlin(pos.x * _noiseTiling, pos.y * _noiseTiling);
        int heightVariation = Mathf.RoundToInt(noise * _heightVariations);
        return heightVariation;
    }

    private void GetSeed()
    {
        if (_refreshSeedOnGeneration)
            _seed = (int)Random.Range(10000000, 99999999);
        _noise = new FastNoise(_seed);
    }


    [Button]
    private void DestroyWorld()
    {
        if (!_worldGenerated)
            return;

        for (int i = _chunks.Count - 1; i >= 0; i--)
        {
            _chunks.ElementAt(i).Value.DestroyChunk();
            Destroy(_chunks.ElementAt(i).Value.gameObject);
        }
        _chunks.Clear();
        _currentLoadedChunks.Clear();

        _worldGenerated = false;
    }

    public void LoadWorldData(WorldData worldData)
    {
        DestroyWorld();

        _seed = worldData.Seed;
        _noise = new FastNoise(_seed);
        
        for (int i = 0; i < worldData.Chunks.Count; i++)
        {
            ChunkData chunkData = worldData.Chunks[i];
            
            Vector3 chunkPos = new Vector3(chunkData.Position.x * Chunk.ChunkSize, 0, chunkData.Position.y * Chunk.ChunkSize);
            Chunk chunk = Instantiate(_chunkPrefab, chunkPos, Quaternion.identity, transform);
            chunk.InitChunk(chunkData);
            _chunks.Add(new Vector2Int(chunkData.Position.x, chunkData.Position.y), chunk);
        }

        _worldGenerated = true;
    }

    public WorldData GetWorldSaveData()
    {
        foreach (KeyValuePair<Vector2Int,Chunk> chunk in Chunks)
        {
            chunk.Value.SaveItems();
        }
        
        WorldData worldData = new WorldData()
        {
            Seed = Seed,
            Chunks = Chunks.Values.Select(c => c.Data).ToList()
        };

        return worldData;
    }
}
