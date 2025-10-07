using System;
using UnityEngine;

[CreateAssetMenu(fileName = "HealthBehaviour", menuName = "ScriptableObjects/Objects/ObjectBehaviours/HealthBehaviour", order = 0)]
public class HealthObjectBehaviour : ObjectBehaviour
{
    [SerializeField] 
    private float _startHealth = 100;

    private float _currentHealth;
    
    public override void Init(ObjectData objectData, ObjectView objectView)
    {
        base.Init(objectData, objectView);
        _currentHealth = _startHealth;
    }
    
    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        if(_currentHealth <= 0)
            _objectView.DestroyObject();
    }

    public override void ApplySaveData(ObjectBehaviourSaveData objectBehaviourSaveData)
    {
        var saveData = (HealthBehaviourSaveData)objectBehaviourSaveData;
        _currentHealth = saveData.CurrentHealth;
    }

    public override ObjectBehaviourSaveData GetSaveData()
    {
        return new HealthBehaviourSaveData(_currentHealth);
    }
}

[Serializable]
public class HealthBehaviourSaveData : ObjectBehaviourSaveData
{
    public float CurrentHealth;

    public HealthBehaviourSaveData(float currentHealth)
    {
        CurrentHealth = currentHealth;
    }
}
