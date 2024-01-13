using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;

public class SceneSystemWizard : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset visualTreeAsset;

    private const string StateText = "State : ";
    private const string TargetPackage = "com.nkstudio.scenereference";

    private Label stateText;
    private Button installButton;

    [MenuItem("Tools/Scene System/Scene System Wizard")]
    public static void SceneSystemWizardWindow()
    {
        SceneSystemWizard wnd = GetWindow<SceneSystemWizard>();
        wnd.titleContent = new GUIContent("Scene System Wizard");
        wnd.minSize = new Vector2(275, 330);

        float max_x = wnd.minSize.x*1.3f;
        float max_y = wnd.minSize.y*1.3f;
        wnd.maxSize = new Vector2(max_x, max_y);
    }

    [InitializeOnLoadMethod]
    private static void ShowAtStartup()
    {
        if (EditorPrefs.GetBool(nameof(SceneSystemWizard)))
        {
            if (!HasOpenInstances<SceneSystemWizard>())
                SceneSystemWizardWindow();
        }
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        visualTreeAsset.CloneTree(root);

        installButton = root.Q<Button>("button-install");
        stateText = root.Q<Label>("text-install");
        var showAtStartupToggle = root.Q<Toggle>("toggle-ShowAtStartup");
        showAtStartupToggle.value = EditorPrefs.GetBool(nameof(SceneSystemWizard));
        HelpBox infoHelpBox = root.Q<HelpBox>("helpBox-info");
        infoHelpBox.text = "Scene System requires Scene Reference.";

        UpdateVersion(root);

        bool check = CheckGitUpmInstallation();
        stateText.text = StateText + (!check ? "Not Installed" : "Installed");
        installButton.SetEnabled(!check);
        installButton.clicked += Install;
        showAtStartupToggle.RegisterValueChangedCallback(evt => EditorPrefs.SetBool(nameof(SceneSystemWizard), evt.newValue));
    }

    private void Install()
    {
        stateText.text = StateText + "Installing...";
        installButton.SetEnabled(false);
        Client.Add("https://github.com/NK-Studio/SceneReference.git#UPM");
    }

    private bool CheckGitUpmInstallation()
    {
        ListRequest request = Client.List();
        while (!request.IsCompleted)
        {
            // 패키지 목록을 가져오는 동안 대기
            stateText.text = StateText + "Checking...";
            installButton.SetEnabled(false);
        }

        if (request.Status == StatusCode.Success)
        {
            foreach (var package in request.Result)
            {
                if (package.name == TargetPackage)
                    return true;
            }
        }

        return false;
    }

    private void UpdateVersion(VisualElement root)
    {
        Label versionLabel = root.Q<Label>("label-version");

        string path = AssetDatabase.GUIDToAssetPath("247393281fb1d493086b31cd293f0d27");

        TextAsset packageJson = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        PackageInfo info = JsonUtility.FromJson<PackageInfo>(packageJson.text);

        versionLabel.text = "Version : " + info.version;
    }
}

[Serializable]
internal class PackageInfo
{
    public string name;
    public string displayName;
    public string version;
    public string unity;
    public string description;
    public List<string> keywords;
    public string type;
}
