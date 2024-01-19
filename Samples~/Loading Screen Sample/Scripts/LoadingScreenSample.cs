using UnityEngine;
using UnityEngine.SceneSystem;

namespace Demo01
{
    public class LoadingScreenSample : MonoBehaviour
    {
        public SceneLoader loadingScreenPrefab;
    
        public string scenePath;

        public void Load()
        {
            SceneLoader loadingScreen = Instantiate(loadingScreenPrefab);
            DontDestroyOnLoad(loadingScreen);
        
            Scenes.LoadSceneAsync(scenePath)
                .WithLoadingScreen(loadingScreen);
        }
    }
}
