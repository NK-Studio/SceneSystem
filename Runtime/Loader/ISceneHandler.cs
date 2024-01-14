using UnityEngine.SceneManagement;
using UnityEngine.SceneSystem.LoadSceneOperations;

namespace UnityEngine.SceneSystem
{
    public interface ISceneHandler
    {
        LoadSceneOperationHandle LoadAsync(string sceneName, LoadSceneMode loadSceneMode);
        LoadSceneOperationHandle UnloadAsync(string sceneName);
        void Load(string sceneName, LoadSceneMode loadSceneMode);
        void Unload(string sceneName);

        LoadSceneOperation GetLoadSceneOperation(string sceneName, LoadSceneMode loadSceneMode);
        LoadSceneOperation GetUnloadSceneOperation(string sceneName);
    }
}
