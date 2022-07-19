using System.Collections.Generic;
using SceneManagement.ScriptableObjects;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace SceneManagement
{
    public class StartGameSceneLoader : MonoBehaviour
    {
        // [SerializeField] private List<SceneAsset> _SceneAssets;
        [SerializeField] private List<LevelSO> _levelsToLoad;
        [SerializeField] private ManagerSO _managerToLoad;

        private void Start()
        {
            LoadGameManager();
        }

        private void LoadGameManager()
        {
            _managerToLoad.sceneReference.LoadSceneAsync(LoadSceneMode.Additive).Completed += LoadLevels;
        }
        
        private void LoadLevels(AsyncOperationHandle<SceneInstance> obj)
        {
            foreach (LevelSO level in _levelsToLoad)
            {
                level.sceneReference.LoadSceneAsync(LoadSceneMode.Additive).Completed += UnloadInitialScenes;
            }
        }

        private void UnloadInitialScenes(AsyncOperationHandle<SceneInstance> obj)
        {
            SceneManager.UnloadSceneAsync(0);
        }
    }
}