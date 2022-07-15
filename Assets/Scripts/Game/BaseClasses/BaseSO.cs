using UnityEngine;

namespace Game.BaseClasses
{
    // Base Scriptable Object with a description TextArea
    public class BaseSO : ScriptableObject
    {
        [SerializeField] [TextArea] private string description;
        

    }
}