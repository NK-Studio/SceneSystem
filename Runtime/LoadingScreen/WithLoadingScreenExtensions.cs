#if USE_SCENE_REFERENCE

namespace UnityEngine.SceneSystem
{
    public static class WithLoadingScreenExtensions
    {
        public static LoadSceneOperationHandle WithLoadingScreen(this LoadSceneOperationHandle self, SceneLoader screen)
        {
            screen.Show(self);
            return self;
        }
    }
}
#endif