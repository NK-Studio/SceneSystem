using System;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SceneSystem;
using UnityEngine.UIElements;

namespace UnityEditor.SceneSystem
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SceneLoader))]
    public class SceneLoaderInspector : Editor
    {
        private VisualElement _root;
        private VisualTreeAsset _visualTree;

        private SerializedProperty _editorAutoLoadProperty;
        private SerializedProperty _loadStyleProperty;
        private SerializedProperty _useAsyncProperty;
        private SerializedProperty _additiveScenesProperty;
        private SerializedProperty _skipModeProperty;
        private SerializedProperty _skipKeyProperty;

        private PropertyField _loadStylePropertyField;
        private PropertyField _useAsyncPropertyField;
        private PropertyField _mainScenePropertyField;
        private PropertyField _editorAutoLoadPropertyField;
        private PropertyField _additiveScenesPropertyField;
        private PropertyField _skipModePropertyField;
        private PropertyField _skipKeyPropertyField;
        private PropertyField _minimumLoadingTimePropertyField;
        private PropertyField _destroyOnCompletedPropertyField;
        private PropertyField _dontUseAsyncPropertyField;
        private Foldout _propertyEvents;

        private void FindProperties()
        {
            _editorAutoLoadProperty = serializedObject.FindProperty("editorAutoLoad");
            _additiveScenesProperty = serializedObject.FindProperty("additiveScenes");
            _loadStyleProperty = serializedObject.FindProperty("LoadStyle");
            _useAsyncProperty = serializedObject.FindProperty("UseAsync");
            _skipModeProperty = serializedObject.FindProperty("SkipMode");

#if ENABLE_LEGACY_INPUT_MANAGER
            _skipKeyProperty = serializedObject.FindProperty("SkipKey");
#elif ENABLE_INPUT_SYSTEM && SCENESYSTEM_SUPPORT_INPUTSYSTEM
            _skipKeyProperty = serializedObject.FindProperty("SkipAction");
#endif
        }

        private void InitElement()
        {
            string uxmlPath = AssetDatabase.GUIDToAssetPath("7524bf6ef64eb47bd98b6cb1b69ba7ef");
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            _root = new VisualElement();
            uxml.CloneTree(_root);

            _loadStylePropertyField = _root.Q<PropertyField>("property-LoadStyle");
            _useAsyncPropertyField = _root.Q<PropertyField>("property-UseAsync");
            _mainScenePropertyField = _root.Q<PropertyField>("property-LoadScene");
            _editorAutoLoadPropertyField = _root.Q<PropertyField>("property-EditorAutoLoad");
            _additiveScenesPropertyField = _root.Q<PropertyField>("property-AdditiveScenes");
            _skipModePropertyField = _root.Q<PropertyField>("property-SkipMode");
            _skipKeyPropertyField = _root.Q<PropertyField>("property-SkipKey");
            _minimumLoadingTimePropertyField = _root.Q<PropertyField>("property-MinimumLoadingTime");
            _destroyOnCompletedPropertyField = _root.Q<PropertyField>("property-DestroyOnCompleted");
            _dontUseAsyncPropertyField = _root.Q<PropertyField>("property-DontUseAsync");
            _propertyEvents = _root.Q<Foldout>("property-Events");

            _skipKeyPropertyField.BindProperty(_skipKeyProperty);

#if !USE_SCENE_REFERENCE
            _propertyMainScene.label = "Load Scene Path";
            _propertyAdditiveScenes.label = "Additive Scenes Path";
#endif
            string editorAutoLoadTooltip = Application.systemLanguage == SystemLanguage.Korean
                ? "자동으로 씬을 로드 하는지 여부를 트리거합니다. (Editor 전용)"
                : "Indicates whether the editor should load it automatically. (Editor Only)";
            _editorAutoLoadPropertyField.tooltip = editorAutoLoadTooltip;

            if (Application.isPlaying)
                _editorAutoLoadPropertyField.SetEnabled(false);
        }

        private void ChangeIcon()
        {
            string path = AssetDatabase.GUIDToAssetPath("a96560f7f90bb4a8ba13c91cbd976615");
            Texture2D iconTexture = EditorIconUtility.LoadIconResource("Scene Loader", $"{path}/");
            EditorGUIUtility.SetIconForObject(target, iconTexture);
        }

        public override VisualElement CreateInspectorGUI()
        {
            ChangeIcon();
            FindProperties();
            InitElement();

            UpdateLoadStyle(_loadStyleProperty);
            UpdateSkipMode(_skipModeProperty); 

            _root.schedule.Execute(() =>
            {
                _skipModePropertyField.RegisterValueChangeCallback(evt => UpdateSkipMode(evt.changedProperty));
                _loadStylePropertyField.RegisterValueChangeCallback(evt => UpdateLoadStyle(evt.changedProperty));
                _useAsyncPropertyField.RegisterValueChangeCallback(evt => UpdateUseAsync(evt.changedProperty));

                if (Application.isEditor && !Application.isPlaying)
                    _editorAutoLoadPropertyField.RegisterValueChangeCallback(evt =>
                        UpdateEditorAutoLoad(evt.changedProperty));
            });

            return _root;
        }

        private void UpdateSkipMode(SerializedProperty serializedProperty)
        {
            LoadingActionSkipMode skipMode = (LoadingActionSkipMode)serializedProperty.intValue;

            switch (skipMode)
            {
                case LoadingActionSkipMode.InstantComplete:
                    _skipKeyPropertyField.style.display = DisplayStyle.None;
                    break;
                case LoadingActionSkipMode.KeyDown:
                    _skipKeyPropertyField.style.display = DisplayStyle.Flex;
                    break;
                case LoadingActionSkipMode.AnyKey:
                    _skipKeyPropertyField.style.display = DisplayStyle.None;
                    break;
                case LoadingActionSkipMode.Manual:
                    _skipKeyPropertyField.style.display = DisplayStyle.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateLoadStyle(SerializedProperty serializedProperty)
        {
            LoadSceneMode loadStyle = (LoadSceneMode)serializedProperty.intValue;

            switch (loadStyle)
            {
                case LoadSceneMode.Single:
                    _mainScenePropertyField.style.display = DisplayStyle.Flex;
                    _additiveScenesPropertyField.style.display = DisplayStyle.None;
                    _useAsyncPropertyField.style.display = DisplayStyle.None;
                    _skipModePropertyField.style.display = DisplayStyle.Flex;
                    _minimumLoadingTimePropertyField.style.display = DisplayStyle.Flex;
                    _dontUseAsyncPropertyField.style.display = DisplayStyle.Flex;
                    _editorAutoLoadPropertyField.style.display = DisplayStyle.None;
                    _propertyEvents.style.display = DisplayStyle.Flex;
                    _destroyOnCompletedPropertyField.style.display = DisplayStyle.Flex;
                    break;
                case LoadSceneMode.Additive:
                    _mainScenePropertyField.style.display = DisplayStyle.None;
                    _additiveScenesPropertyField.style.display = DisplayStyle.Flex;
                    _useAsyncPropertyField.style.display = DisplayStyle.Flex;
                    _editorAutoLoadPropertyField.style.display = DisplayStyle.Flex;
                    _dontUseAsyncPropertyField.style.display = DisplayStyle.None;

                    if (_useAsyncProperty.boolValue)
                    {
                        _skipModePropertyField.style.display = DisplayStyle.Flex;
                        _minimumLoadingTimePropertyField.style.display = DisplayStyle.Flex;
                        _propertyEvents.style.display = DisplayStyle.Flex;
                        _destroyOnCompletedPropertyField.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        _skipModePropertyField.style.display = DisplayStyle.None;
                        _minimumLoadingTimePropertyField.style.display = DisplayStyle.None;
                        _propertyEvents.style.display = DisplayStyle.None;
                        _destroyOnCompletedPropertyField.style.display = DisplayStyle.None;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateUseAsync(SerializedProperty evtChangedProperty)
        {
            var useAsync = evtChangedProperty.boolValue;

            if (useAsync)
            {
                _skipModePropertyField.style.display = DisplayStyle.Flex;
                _minimumLoadingTimePropertyField.style.display = DisplayStyle.Flex;
                _destroyOnCompletedPropertyField.style.display = DisplayStyle.Flex;

                if (!_editorAutoLoadProperty.boolValue)
                    _propertyEvents.style.display = DisplayStyle.Flex;
                else
                    _propertyEvents.style.display = DisplayStyle.None;
            }
            else
            {
                _skipModePropertyField.style.display = DisplayStyle.None;
                _minimumLoadingTimePropertyField.style.display = DisplayStyle.None;
                _propertyEvents.style.display = DisplayStyle.None;
                _destroyOnCompletedPropertyField.style.display = DisplayStyle.None;
            }
        }

        private void UpdateEditorAutoLoad(SerializedProperty evtChangedProperty)
        {
            bool editorAutoLoad = evtChangedProperty.boolValue;

            if (editorAutoLoad)
            {
#if USE_SCENE_REFERENCE
                if (_additiveScenesProperty.arraySize > 0)
                {
                    for (int i = 0; i < _additiveScenesProperty.arraySize; i++)
                    {
                        string targetScene = _additiveScenesProperty.GetArrayElementAtIndex(i)
                            .FindPropertyRelative("path").stringValue;

                        if (!string.IsNullOrWhiteSpace(targetScene))
                        {
                            if (!SceneManager.GetSceneByPath(targetScene).isLoaded)
                                EditorSceneManager.OpenScene(targetScene, OpenSceneMode.Additive);
                        }
                    }
                }
#else
                if (_additiveScenesProperty.arraySize > 0)
                {
                    for (int i = 0; i < _additiveScenesProperty.arraySize; i++)
                    {
                        string targetScene = _additiveScenesProperty.GetArrayElementAtIndex(i).stringValue;
                        Scene foundAsset = SceneManager.GetSceneByPath(targetScene);

                        if (!foundAsset.isLoaded)
                            EditorSceneManager.OpenScene(targetScene, OpenSceneMode.Additive);
                    }
                }
#endif
                _propertyEvents.style.display = DisplayStyle.None;
            }
            else
            {
#if USE_SCENE_REFERENCE
                if (_additiveScenesProperty.arraySize > 0)
                {
                    for (int i = 0; i < _additiveScenesProperty.arraySize; i++)
                    {
                        string targetScene = _additiveScenesProperty.GetArrayElementAtIndex(i)
                            .FindPropertyRelative("path").stringValue;

                        if (SceneManager.GetSceneByPath(targetScene).isLoaded)
                            EditorSceneManager.CloseScene(SceneManager.GetSceneByPath(targetScene), true);
                    }
                }
#else
                if (_additiveScenesProperty.arraySize > 0)
                {
                    for (int i = 0; i < _additiveScenesProperty.arraySize; i++)
                    {
                        string targetScene = _additiveScenesProperty.GetArrayElementAtIndex(i).stringValue;
                        Scene sceneAsset = SceneManager.GetSceneByPath(targetScene);

                        if (sceneAsset.isLoaded)
                            EditorSceneManager.CloseScene(sceneAsset, true);
                    }
                }
#endif

                if (_useAsyncProperty.boolValue)
                    _propertyEvents.style.display = DisplayStyle.Flex;
            }
        }
    }
}