using System;
using System.Linq;
using AnnulusGames.SceneSystem;
using UnityEngine;

public class SceneLoader : LoadingScreen
{
    public enum ELoadStyle
    {
        Single,
        Additive
    }

    public ELoadStyle LoadStyle;
    public SceneReference MainScene;
    public SceneReference[] AdditiveScenes;

    private void Start()
    {

        switch (LoadStyle)
        {
            case ELoadStyle.Single:
                if (!string.IsNullOrEmpty(MainScene.Path))
                    Scenes.LoadSceneAsync(MainScene).WithLoadingScreen(this);
                break;
            case ELoadStyle.Additive:
                if (AdditiveScenes.Length > 0)
                {
                    if (AdditiveScenes.Any(sceneReference => string.IsNullOrEmpty(sceneReference.Path)))
                        return;

                    Scenes.LoadScenesAsync(AdditiveScenes).WithLoadingScreen(this);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
