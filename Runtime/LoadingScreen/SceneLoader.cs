#if USE_SCENE_REFERENCE
using System;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM && SCENESYSTEM_SUPPORT_INPUTSYSTEM
using UnityEngine.InputSystem;
#endif

namespace UnityEngine.SceneSystem
{
    [AddComponentMenu("Scene System/Scene Loader")]
    public class SceneLoader : MonoBehaviour
    {
        /// <summary>
        /// The scene loading style.
        /// </summary>
        public LoadSceneMode LoadStyle;

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
        public UnityEvent<float> onLoading = new UnityEvent<float>();
        [Space(5), Tooltip("Called when the loading is complete.")]
        public UnityEvent onLoadCompleted = new UnityEvent();
        [Space(5), Tooltip("Called when the loading screen is completed.")]
        public UnityEvent onCompleted = new UnityEvent();

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
            switch (LoadStyle)
            {
                case LoadSceneMode.Single:
                    if (!string.IsNullOrEmpty(loadScene.Path))
                        Scenes.LoadSceneAsync(loadScene).WithLoadingScreen(this);
                    break;
                case LoadSceneMode.Additive:
                    if (additiveScenes.Length > 0)
                    {
                        if (additiveScenes.Any(sceneReference => string.IsNullOrEmpty(sceneReference.Path)))
                            return;

                        Scenes.LoadScenesAsync(additiveScenes).WithLoadingScreen(this);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Update()
        {
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
        public SceneReference GetLoadScene()
        {
            if (LoadStyle == LoadSceneMode.Single)
                return loadScene;

            return null;
        }

        /// <summary>
        /// Gets an array of SceneReference objects representing the scenes to be loaded.
        /// </summary>
        /// <returns>An array of SceneReference objects if the LoadStyle is set to LoadSceneMode.Additive; otherwise, returns null.</returns>
        public SceneReference[] GetLoadScenes()
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
    }

    public enum LoadingActionSkipMode
    {
        InstantComplete,
        AnyKey,
        Manual
    }
}
#endif
