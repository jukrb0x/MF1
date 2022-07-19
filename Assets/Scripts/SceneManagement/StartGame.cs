using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    [SerializeField] private List<SceneAsset> _SceneAssets;

    private void Start()
    {
        foreach (SceneAsset sceneAsset in _SceneAssets)
        {
            SceneManager.LoadSceneAsync(sceneAsset.name, LoadSceneMode.Additive);
        }

        // Debug.Log(gameObject.scene.name);
        SceneManager.UnloadSceneAsync(0);
    }
}