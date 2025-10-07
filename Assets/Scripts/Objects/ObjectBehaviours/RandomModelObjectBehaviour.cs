using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomModelBehaviour", menuName = "ScriptableObjects/Objects/ObjectBehaviours/RandomModelBehaviour", order = 0)]
public class RandomModelObjectBehaviour : ObjectBehaviour
{
    [SerializeField] 
    private List<GameObject> _modelPrefabs;
    [SerializeField] 
    private bool _randomYRotation = true;

    private GameObject _model = null;
    private bool _modelSpawned = false;
    private int _modelIndex;
    private float _rotation;
    
    public override void Init(ObjectData objectData, ObjectView objectView)
    {
        base.Init(objectData, objectView);
        _modelIndex = UnityEngine.Random.Range(0, _modelPrefabs.Count);
        _rotation = UnityEngine.Random.Range(0.0f, 360.0f);
        SpawnModel();
    }

    private void SpawnModel()
    {
        _model = Instantiate(_modelPrefabs[_modelIndex], _objectView.transform.position, Quaternion.Euler(0, _rotation, 0), _objectView.transform);
        _modelSpawned = true;
    }

    private void DestroyModel()
    {
        if (!_modelSpawned)
            return;

        Destroy(_model);
        _model = null;
        _modelSpawned = false;
    }

    public override void ApplySaveData(ObjectBehaviourSaveData objectBehaviourSaveData)
    {
        var saveData = (RandomModelBehaviourSaveData)objectBehaviourSaveData;
        _modelIndex = saveData.ModelIndex;
        _rotation = saveData.Rotation;
        DestroyModel();
        SpawnModel();
    }

    public override ObjectBehaviourSaveData GetSaveData()
    {
        return new RandomModelBehaviourSaveData(_modelIndex, _rotation);
    }
}

[Serializable]
public class RandomModelBehaviourSaveData : ObjectBehaviourSaveData
{
    public int ModelIndex;
    public float Rotation;

    public RandomModelBehaviourSaveData(int modelIndex, float rotation)
    {
        ModelIndex = modelIndex;
        Rotation = rotation;
    }
}
