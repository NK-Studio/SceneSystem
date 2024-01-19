using UnityEngine;
using UnityEngine.SceneSystem;

namespace Demo02
{
    public class LoadingScreenSample : MonoBehaviour
    {
        public SceneLoader loadingScreenPrefab;

#if USE_SCENE_REFERENCE
        public SceneReference SceneReference;
#endif

        public void Load()
        {
#if USE_SCENE_REFERENCE
            SceneLoader loadingScreen = Instantiate(loadingScreenPrefab);
            DontDestroyOnLoad(loadingScreen);

            Scenes.LoadSceneAsync(SceneReference)
                .WithLoadingScreen(loadingScreen);
#else
            Debug.Log("Scene Reference is not installed.");
#endif
        }
    }
}