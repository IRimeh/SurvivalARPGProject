using UnityEngine;

public class Locator<T> : BaseLocator where T : MonoBehaviour
{
    public T Value { get; private set; }
    public void AssignLocator(T value)
    {
        Value = value;
    }
    public override void Assign(MonoBehaviour monoBehaviour)
    {
        AssignLocator(monoBehaviour as T);
    }
}

public abstract class BaseLocator : ScriptableObject
{
    public abstract void Assign(MonoBehaviour monoBehaviour);
}