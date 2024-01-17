using System;
using UnityEditor;
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

        private SerializedProperty _loadStyleProperty;
        private SerializedProperty _useAsyncProperty;

        private PropertyField _propertyLoadStyle;
        private PropertyField _propertyUseAsync;
        private PropertyField _propertyMainScene;
        private PropertyField _propertyAdditiveScenes;
        private PropertyField _propertySkipMode;
        private PropertyField _propertyMinimumLoadingTime;
        
        private void FindProperties()
        {
            _loadStyleProperty = serializedObject.FindProperty("LoadStyle");
            _useAsyncProperty = serializedObject.FindProperty("UseAsync");
        }

        private void InitElement()
        {
            string path = AssetDatabase.GUIDToAssetPath("7524bf6ef64eb47bd98b6cb1b69ba7ef");
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            _root = new VisualElement();
            uxml.CloneTree(_root);

            _propertyLoadStyle = _root.Q<PropertyField>("property-LoadStyle");
            _propertyUseAsync = _root.Q<PropertyField>("property-UseAsync");
            _propertyMainScene = _root.Q<PropertyField>("property-LoadScene");
            _propertyAdditiveScenes = _root.Q<PropertyField>("property-AdditiveScenes");
            _propertySkipMode = _root.Q<PropertyField>("property-SkipMode");
            _propertyMinimumLoadingTime = _root.Q<PropertyField>("property-MinimumLoadingTime");
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

            _propertyLoadStyle.RegisterValueChangeCallback(evt => UpdateLoadStyle(evt.changedProperty));
            _propertyUseAsync.RegisterValueChangeCallback(evt => UpdateUseAsync(evt.changedProperty));

            return _root;
        }

        private void UpdateLoadStyle(SerializedProperty serializedProperty)
        {
            LoadSceneMode loadStyle = (LoadSceneMode)serializedProperty.intValue;

            switch (loadStyle)
            {
                case LoadSceneMode.Single:
                    _propertyMainScene.style.display = DisplayStyle.Flex;
                    _propertyAdditiveScenes.style.display = DisplayStyle.None;
                    _propertyUseAsync.style.display = DisplayStyle.None;
                    _propertySkipMode.style.display = DisplayStyle.Flex;
                    _propertyMinimumLoadingTime.style.display = DisplayStyle.Flex;
                    break;
                case LoadSceneMode.Additive:
                    _propertyMainScene.style.display = DisplayStyle.None;
                    _propertyAdditiveScenes.style.display = DisplayStyle.Flex;
                    _propertyUseAsync.style.display = DisplayStyle.Flex;
                    
                    if (_useAsyncProperty.boolValue)
                    {
                        _propertySkipMode.style.display = DisplayStyle.Flex;
                        _propertyMinimumLoadingTime.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        _propertySkipMode.style.display = DisplayStyle.None;
                        _propertyMinimumLoadingTime.style.display = DisplayStyle.None;
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
                _propertySkipMode.style.display = DisplayStyle.Flex;
                _propertyMinimumLoadingTime.style.display = DisplayStyle.Flex;
            }
            else
            {
                _propertySkipMode.style.display = DisplayStyle.None;
                _propertyMinimumLoadingTime.style.display = DisplayStyle.None;
            }
        }
    }
}