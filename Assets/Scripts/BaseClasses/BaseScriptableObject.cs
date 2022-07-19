using UnityEngine;

namespace BaseClasses
{
    // Base Scriptable Object with a description TextArea
    public class BaseScriptableObject : ScriptableObject
    {
        [SerializeField] [TextArea] private string description;
    }
}