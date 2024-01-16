#if USE_SCENE_REFERENCE
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

        private PropertyField _propertyLoadStyle;
        private PropertyField _propertyMainScene;
        private PropertyField _propertyAdditiveScenes;
        
        private void FindProperties()
        {
            _loadStyleProperty = serializedObject.FindProperty("LoadStyle");
        }

        private void InitElement()
        {
            string path = AssetDatabase.GUIDToAssetPath("7524bf6ef64eb47bd98b6cb1b69ba7ef");
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            _root = new VisualElement();
            uxml.CloneTree(_root);

            _propertyLoadStyle = _root.Q<PropertyField>("property-LoadStyle");
            _propertyMainScene = _root.Q<PropertyField>("property-LoadScene");
            _propertyAdditiveScenes = _root.Q<PropertyField>("property-AdditiveScenes");

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
                    break;
                case LoadSceneMode.Additive:
                    _propertyMainScene.style.display = DisplayStyle.None;
                    _propertyAdditiveScenes.style.display = DisplayStyle.Flex;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
#endif
