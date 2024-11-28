using System.Collections.Generic;
using UnityEngine;

public class ObjectView : MonoBehaviour
{
    private Chunk _chunk;
    public Chunk Chunk => _chunk;
    
    private Vector2Int _relativePos;
    public Vector2Int RelativePos => _relativePos;
    
    private ObjectData _objectData;
    public ObjectData ObjectData => _objectData;

    private List<ObjectBehaviour> _objectBehaviours = new();
    public List<ObjectBehaviour> ObjectBehaviours => _objectBehaviours;
    
    private static float _cellSize = (float)Chunk.ChunkSize / (float)Chunk.ObjectsRowCount;
    private static float _halfCellSize = _cellSize * .5f;
    
    public void InitObjectData(ObjectData objectData, Chunk chunk, Vector2Int relativePos)
    {
        _chunk = chunk;
        _objectData = objectData;
        _relativePos = relativePos;
        SetPos(relativePos);
        foreach (var objectBehaviour in objectData.Behaviours)
        {
            var behaviour = Instantiate(objectBehaviour);
            behaviour.Init(objectData, this);
            _objectBehaviours.Add(behaviour);
        }
    }
    
    public void ApplyObjectSaveData(ObjectSaveData objectSaveData)
    {
        for (int i = 0; i < _objectBehaviours.Count; i++)
        {
            _objectBehaviours[i].ApplySaveData(objectSaveData.ObjectBehaviourSaveDatas[i]);
        }
    }

    public List<ObjectBehaviourSaveData> GetObjectBehaviourSaveDatas()
    {
        List<ObjectBehaviourSaveData> saveDatas = new List<ObjectBehaviourSaveData>();
        foreach (var objBehaviour in ObjectBehaviours)
        {
            saveDatas.Add(objBehaviour.GetSaveData());
        }
        return saveDatas;
    }

    private void Update()
    {
        foreach (ObjectBehaviour objectBehaviour in _objectBehaviours)
        {
            objectBehaviour.Update();
        }
    }

    private void SetPos(Vector2Int relativePos)
    {
        Vector3 cornerPos = _chunk.transform.position - new Vector3((float)Chunk.ChunkSize * .5f, 0, (float)Chunk.ChunkSize * .5f);
        Vector3 offset = new Vector3(relativePos.x * _cellSize + _halfCellSize, 0, relativePos.y * _cellSize + _halfCellSize);
        transform.position = cornerPos + offset;
    }
}
