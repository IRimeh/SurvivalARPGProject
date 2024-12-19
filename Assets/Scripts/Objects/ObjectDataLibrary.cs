using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectDataLibrary", menuName = "ScriptableObjects/Objects/ObjectDataLibrary", order = 0)]
public class ObjectDataLibrary : ScriptableObject
{
    [SerializeField] 
    private List<ObjectData> _objectDatas = new List<ObjectData>();

    public ObjectData GetObjectFromID(int ID)
    {
        return _objectDatas[ID];
    }

    [Button]
    private void SetIDs()
    {
        for (int i = 0; i < _objectDatas.Count; i++)
        {
            _objectDatas[i].Id = i;
        }
    }
    
    private void OnValidate()
    {
        SetIDs();
    }
}
