#if USE_SCENE_REFERENCE
using System.IO;
using UnityEngine;

namespace UnityEditor.SceneSystem
{
    internal static class EditorIconUtility
    {
        public static Texture2D LoadIconResource(string name, string path)
        {
            var iconPath = "";

            if (EditorGUIUtility.isProSkin && !string.IsNullOrEmpty(path))
                iconPath = Path.Combine(path, "d_" + name);
            else
                iconPath = Path.Combine(path, name);

            if (EditorGUIUtility.pixelsPerPoint > 1.0f)
            {
                var icon2x = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath + "@2x.png");
                if (icon2x != null)
                    return icon2x;
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath + ".png");
        }
    }
}
#endif