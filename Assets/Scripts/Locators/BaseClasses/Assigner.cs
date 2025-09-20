using UnityEngine;

public class Assigner : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _value;
    [SerializeField] private BaseLocator _locator;
    private void Awake()
    {
        _locator.Assign(_value);
    }
}