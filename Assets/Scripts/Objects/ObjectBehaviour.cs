using System;
using UnityEngine;

public class ObjectBehaviour : ScriptableObject
{
    protected ObjectData _objectData;
    protected ObjectView _objectView;

    public virtual void Init(ObjectData objectData, ObjectView objectView)
    {
        _objectData = objectData;
        _objectView = objectView;
    }

    public virtual void UnInit() { }

    public virtual void Update() { }
    
    public virtual ObjectBehaviourSaveData GetSaveData()
    {
        return null;
    }

    public virtual void ApplySaveData(ObjectBehaviourSaveData objectBehaviourSaveData)
    {
    }
}

[Serializable]
public class ObjectBehaviourSaveData
{
}
