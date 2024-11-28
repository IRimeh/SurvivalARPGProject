using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectData", menuName = "ScriptableObjects/ObjectData", order = 0)]
public class ObjectData : ScriptableObject
{
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
