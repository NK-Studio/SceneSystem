namespace UnityEngine.SceneSystem
{
    public static class WithLoadingScreenExtensions
    {
        public static LoadSceneOperationHandle WithLoadingScreen(this LoadSceneOperationHandle self, SceneLoadPlayer screen)
        {
            screen.Show(self);
            return self;
        }
    }
}