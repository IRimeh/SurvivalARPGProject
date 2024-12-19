using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectData", menuName = "ScriptableObjects/Objects/ObjectData", order = 0)]
public class ObjectData : ScriptableObject
{
    [ReadOnly]
    public int Id;
    public ObjectView Prefab;
    public List<ObjectBehaviour> Behaviours = new List<ObjectBehaviour>();
}

[Serializable]
public struct ObjectSaveData
{
    public int ID;
    public Vector2Int Pos;
    public List<ObjectBehaviourSaveData> ObjectBehaviourSaveDatas;
}
