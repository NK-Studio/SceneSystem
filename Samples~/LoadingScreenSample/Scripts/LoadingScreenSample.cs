using UnityEngine;
using UnityEngine.SceneSystem;


public sealed class LoadingScreenSample : MonoBehaviour
{
    public SceneLoader loadingScreenPrefab;

#if USE_SCENE_REFERENCE
    public SceneReference sceneReference;
#else
    public string scenePath;
#endif


    public void Load()
    {
        SceneLoader loadingScreen = Instantiate(loadingScreenPrefab);
        DontDestroyOnLoad(loadingScreen);

#if USE_SCENE_REFERENCE
        Scenes.LoadSceneAsync(sceneReference).WithLoadingScreen(loadingScreen);
#else
        Scenes.LoadSceneAsync(scenePath).WithLoadingScreen(loadingScreen);
#endif
    }
}
