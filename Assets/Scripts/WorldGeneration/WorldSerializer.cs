using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NaughtyAttributes;
using Newtonsoft.Json;
using UnityEngine;

public class WorldSerializer : MonoBehaviour
{
    [SerializeField] private WorldGenerator _worldGenerator;

    private static string SAVE_PATH = $"{Application.streamingAssetsPath}/save.json";

    [Button]
    public void SaveChunks()
    {
        foreach (KeyValuePair<Vector2Int,Chunk> chunk in _worldGenerator.Chunks)
        {
            chunk.Value.SaveObjects();
        }
        
        WorldData worldData = new WorldData()
        {
            Seed = _worldGenerator.Seed,
            Chunks = _worldGenerator.Chunks.Values.Select(c => c.Data).ToList()
        };
        
        JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, DefaultValueHandling = DefaultValueHandling.Ignore };
        string worldDataJson = JsonConvert.SerializeObject(worldData, settings);

        CreatePath(SAVE_PATH);
        StreamWriter writer = new StreamWriter(SAVE_PATH);
        writer.Write(worldDataJson);
        writer.Close();
    }

    [Button]
    public void LoadChunks()
    {
        if (!File.Exists(SAVE_PATH))
            return;
        
        StreamReader reader = new StreamReader(SAVE_PATH);
        string worldDataJson = reader.ReadToEnd();
        JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, DefaultValueHandling = DefaultValueHandling.Ignore };
        WorldData worldData = JsonConvert.DeserializeObject<WorldData>(worldDataJson, settings);
        reader.Close();
        
        _worldGenerator.LoadWorldData(worldData);
    }

    private void CreatePath(string path)
    {
        string[] splitPath = path.Split(new char[]
        {
            '/',
            '\\'
        });
        string directory = path.Replace(splitPath[^1], "");

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        if (!File.Exists(path))
        {
            FileStream fileStream = File.Create(path);
            fileStream.Close();
        }
    }
}

[Serializable]
public struct WorldData
{
    public int Seed;
    public List<ChunkData> Chunks;
}