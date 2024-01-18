using System;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM && SCENESYSTEM_SUPPORT_INPUTSYSTEM
using UnityEngine.InputSystem;
#endif

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace UnityEngine.SceneSystem
{
    [ExecuteInEditMode]
    [AddComponentMenu("Scene System/Scene Loader")]
    public class SceneLoader : MonoBehaviour
    {
        /// <summary>
        /// The scene loading style.
        /// </summary>
        public LoadSceneMode LoadStyle;

        /// <summary>
        /// Represents whether asynchronous operations should be used. (Additive Only)
        /// </summary>
        public bool UseAsync;

#if UNITY_EDITOR
        /// <summary>
        /// Indicates whether the editor should load it automatically. (Editor Only)
        /// </summary>
        [Tooltip("Indicates whether the editor should load it automatically. (Editor Only)")]
        [SerializeField]
        private bool editorAutoLoad;
#endif

#if USE_SCENE_REFERENCE
        /// <summary>
        /// Scene to load.
        /// </summary>
        [SerializeField]
        private SceneReference loadScene;

        /// <summary>
        /// Additive scenes to load.
        /// </summary>
        [SerializeField]
        private SceneReference[] additiveScenes;
#else
        /// <summary>
        /// Scene to load.
        /// </summary>
        [SerializeField]
        private string loadScene;

        /// <summary>
        /// Additive scenes to load.
        /// </summary>
        [SerializeField]
        private string[] additiveScenes;
#endif

        /// <summary>
        /// Represents the skip mode for a loading action.
        /// </summary>
        public LoadingActionSkipMode SkipMode;

        [Range(0f, 10f)]
        [Tooltip("During the minimum loading time, the loading screen will remain visible even after loading is complete.")]
        public float MinimumLoadingTime;

        [Tooltip("If true, it will be automatically deleted upon completion.")]
        public bool DestroyOnCompleted;

        [Space(5), Tooltip("Called during loading. (MinimumLoadingTime must be greater than 0.)")]
        public UnityEvent<float> onLoading;
        [Space(5), Tooltip("Called when the loading is complete.")]
        public UnityEvent onLoadCompleted;
        [Space(5), Tooltip("Called when the loading screen is completed.")]
        public UnityEvent onCompleted;

        private LoadSceneOperationHandle _handle;
        private bool _callOnCompleted;
        private bool _allowCompletion;
        private float _startTime;

        private Action _onCompletedInternal;

        public void AllowCompletion()
        {
            _allowCompletion = true;
        }

        private void Start()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                OpenSceneEditorAutoLoad();
                return;
            }

            switch (LoadStyle)
            {
                case LoadSceneMode.Single:
#if USE_SCENE_REFERENCE
                    if (!string.IsNullOrEmpty(loadScene.Path))
                        Scenes.LoadSceneAsync(loadScene).WithLoadingScreen(this);
#else
                    if (!string.IsNullOrEmpty(loadScene))
                        Scenes.LoadSceneAsync(loadScene).WithLoadingScreen(this);
#endif
                    break;
                case LoadSceneMode.Additive:
                    if (additiveScenes.Length > 0)
                    {
                        bool preCheck = false;
                        if (Application.isEditor && Application.isPlaying)
                            preCheck = CheckAllLoadAdditiveScene();

#if USE_SCENE_REFERENCE
                        if (additiveScenes.Any(sceneReference => string.IsNullOrEmpty(sceneReference.Path)))
                            return;

                        if (!preCheck)
                        {
                            if (UseAsync)
                                Scenes.LoadScenesAsync(additiveScenes).WithLoadingScreen(this);
                            else
                                Scenes.LoadScenes(additiveScenes);
                        }
                        else
                        {
#if UNITY_EDITOR
                            if (onLoading.GetPersistentEventCount() > 0)
                            {
                                string msg = Application.systemLanguage == SystemLanguage.Korean ?
                                    "Editor Auto Load가 켜져있는 상태에서는 OnLoading 이벤트가 호출되지 않습니다." :
                                    "OnLoading event is not called when Editor Auto Load is turned on.";
                                Debug.LogWarning(msg);
                            }

                            if (onLoadCompleted.GetPersistentEventCount() > 0)
                            {
                                string msg = Application.systemLanguage == SystemLanguage.Korean ?
                                    "Editor Auto Load가 켜져있는 상태에서는 OnLoadCompleted 이벤트가 호출되지 않습니다." :
                                    "OnLoadCompleted event is not called when Editor Auto Load is turned on.";
                                Debug.LogWarning(msg);
                            }

                            if (onCompleted.GetPersistentEventCount() > 0)
                            {
                                string msg = Application.systemLanguage == SystemLanguage.Korean ?
                                    "Editor Auto Load가 켜져있는 상태에서는 OnCompleted 이벤트가 호출되지 않습니다." :
                                    "OnCompleted event is not called when Editor Auto Load is turned on.";
                                Debug.LogWarning(msg);
                            }
#endif
                        }
#else
                        if (additiveScenes.Any(string.IsNullOrEmpty))
                            return;

                        if (!preCheck)
                        {
                            if (UseAsync)
                                Scenes.LoadScenesAsync(additiveScenes).WithLoadingScreen(this);
                            else
                                Scenes.LoadScenes(additiveScenes);
                        }
                        else
                        {
#if UNITY_EDITOR
                            if (onLoading.GetPersistentEventCount() > 0)
                            {
                                string msg = Application.systemLanguage == SystemLanguage.Korean ?
                                    "Editor Auto Load가 켜져있는 상태에서는 OnLoading 이벤트가 호출되지 않습니다." :
                                    "OnLoading event is not called when Editor Auto Load is turned on.";
                                Debug.LogWarning(msg);
                            }
                            
                            if (onLoadCompleted.GetPersistentEventCount() > 0)
                            {
                                string msg = Application.systemLanguage == SystemLanguage.Korean ?
                                    "Editor Auto Load가 켜져있는 상태에서는 OnLoadCompleted 이벤트가 호출되지 않습니다." :
                                    "OnLoadCompleted event is not called when Editor Auto Load is turned on.";
                                Debug.LogWarning(msg);
                            }
                            
                            if (onCompleted.GetPersistentEventCount() > 0)
                            {
                                string msg = Application.systemLanguage == SystemLanguage.Korean ?
                                    "Editor Auto Load가 켜져있는 상태에서는 OnCompleted 이벤트가 호출되지 않습니다." :
                                    "OnCompleted event is not called when Editor Auto Load is turned on.";
                                Debug.LogWarning(msg);
                            }
#endif
                        }
#endif
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Update()
        {
            if (!Application.isPlaying)
                return;

            if (!_callOnCompleted)
            {
                float time = Time.realtimeSinceStartup - _startTime;
                float progress = 0f;
                float requestProgress = Mathf.InverseLerp(0f, 0.9f, _handle.Progress);

                if (MinimumLoadingTime <= 0f)
                {
                    progress = requestProgress;
                }
                else
                {
                    progress = Math.Min(
                        requestProgress, // Real
                        Mathf.InverseLerp(0f, MinimumLoadingTime, time) // Fake
                    );
                }

                onLoading.Invoke(progress);

                if (!_callOnCompleted && progress >= 1f)
                {
                    _callOnCompleted = true;

                    onLoadCompleted?.Invoke();

                    if (SkipMode == LoadingActionSkipMode.InstantComplete)
                    {
                        AllowCompletion();
                    }
                }
            }

            if (!_callOnCompleted) return;

            if (SkipMode == LoadingActionSkipMode.AnyKey)
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                if (Input.anyKeyDown) AllowCompletion();
#endif
#if ENABLE_INPUT_SYSTEM && SCENESYSTEM_SUPPORT_INPUTSYSTEM
                if (Keyboard.current != null &&
                    Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    AllowCompletion();
                }
                if (Mouse.current != null &&
                    (Mouse.current.leftButton.wasPressedThisFrame || 
                     Mouse.current.rightButton.wasPressedThisFrame ||
                     Mouse.current.middleButton.wasPressedThisFrame))
                {
                    AllowCompletion();
                }
                if (Gamepad.current != null &&
                    (Gamepad.current.buttonNorth.wasPressedThisFrame ||
                    Gamepad.current.buttonSouth.wasPressedThisFrame ||
                    Gamepad.current.buttonWest.wasPressedThisFrame ||
                    Gamepad.current.buttonEast.wasPressedThisFrame))
                {
                    AllowCompletion();
                }
                if (Touchscreen.current != null &&
                    Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    AllowCompletion();
                }
#endif
            }

            if (_allowCompletion)
            {
                _handle.AllowSceneActivation(true);
                _handle.onCompleted += () => {
                    CallOnCompletedEvent();
                };
            }
        }

        /// <summary>
        /// Returns the SceneReference to be loaded based on the LoadStyle.
        /// If the LoadStyle is LoadSceneMode.Single, returns the loadScene.
        /// If the LoadStyle is not LoadSceneMode.Single, returns null.
        /// </summary>
        /// <returns>The SceneReference to be loaded or null.</returns>
#if USE_SCENE_REFERENCE
        public SceneReference GetLoadScene()
#else
        public string GetLoadScene()
#endif
        {
            if (LoadStyle == LoadSceneMode.Single)
                return loadScene;

            return null;
        }

        /// <summary>
        /// Gets an array of SceneReference objects representing the scenes to be loaded.
        /// </summary>
        /// <returns>An array of SceneReference objects if the LoadStyle is set to LoadSceneMode.Additive; otherwise, returns null.</returns>
#if USE_SCENE_REFERENCE
        public SceneReference[] GetLoadScenes()
#else
        public string[] GetLoadScenes()
#endif
        {
            if (LoadStyle == LoadSceneMode.Additive)
                return additiveScenes;

            return null;
        }

        internal void Show(LoadSceneOperationHandle handle)
        {
            _callOnCompleted = false;
            _allowCompletion = false;
            _startTime = Time.realtimeSinceStartup;

            _handle = handle;
            _handle.AllowSceneActivation(false);
        }

        private void CallOnCompletedEvent()
        {
            onCompleted?.Invoke();
            _onCompletedInternal?.Invoke();

            if (DestroyOnCompleted) Destroy(gameObject);
        }

        private void OpenSceneEditorAutoLoad()
        {
#if UNITY_EDITOR
            if (LoadStyle == LoadSceneMode.Single)
                return;

            if (editorAutoLoad)
            {
#if USE_SCENE_REFERENCE
                if (additiveScenes.Length > 0)
                {
                    for (int i = 0; i < additiveScenes.Length; i++)
                    {
                        string targetScene = additiveScenes[i].Path;

                        if (!SceneManager.GetSceneByPath(targetScene).isLoaded)
                            EditorSceneManager.OpenScene(targetScene, OpenSceneMode.Additive);
                    }
                }
#else
                if (additiveScenes.Length > 0)
                {
                    for (int i = 0; i < additiveScenes.Length; i++)
                    {
                        string targetScene = additiveScenes[i];
                        Scene foundAsset = SceneManager.GetSceneByPath(targetScene);

                        if (!foundAsset.isLoaded)
                            EditorSceneManager.OpenScene(targetScene, OpenSceneMode.Additive);
                    }
                }
#endif
            }
#endif
        }

        private bool CheckAllLoadAdditiveScene()
        {
#if UNITY_EDITOR

            if (LoadStyle == LoadSceneMode.Single)
                return false;

            // additiveScenes가 모두 로드되어있는지 체크
            bool[] isLoaded = new bool[additiveScenes.Length];

#if USE_SCENE_REFERENCE
            if (additiveScenes.Length > 0)
            {
                for (int i = 0; i < additiveScenes.Length; i++)
                {
                    string targetScene = additiveScenes[i].Path;

                    if (SceneManager.GetSceneByPath(targetScene).isLoaded)
                        isLoaded[i] = true;
                    else
                        isLoaded[i] = false;
                }
            }
#else
            if (additiveScenes.Length > 0)
            {
                for (int i = 0; i < additiveScenes.Length; i++)
                {
                    string targetScene = additiveScenes[i];
                    Scene foundAsset = SceneManager.GetSceneByPath(targetScene);

                    if (foundAsset.isLoaded)
                        isLoaded[i] = true;
                    else
                        isLoaded[i] = false;
                }
            }
#endif

            if (isLoaded.All(x => x))
                return true;

#endif
            return false;
        }
    }

    public enum LoadingActionSkipMode
    {
        InstantComplete,
        AnyKey,
        Manual
    }
}
