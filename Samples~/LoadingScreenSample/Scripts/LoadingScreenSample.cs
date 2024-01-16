using UnityEngine;
using UnityEngine.SceneSystem;


public sealed class LoadingScreenSample : MonoBehaviour
{
    public SceneLoader loadingScreenPrefab;
    public SceneReference sceneReference;

    public void Load()
    {
        SceneLoader loadingScreen = Instantiate(loadingScreenPrefab);
        DontDestroyOnLoad(loadingScreen);

        Scenes.LoadSceneAsync(sceneReference)
            .WithLoadingScreen(loadingScreen);
    }
}
