using Game.BaseClasses;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SceneManagement.ScriptableObjects
{
    public class SceneSO : BaseScriptableObject
    {
        public AssetReference sceneReference;
        public SceneType sceneType;
        
        public enum SceneType
        {
            Level,
            Manager
        }
    }
}