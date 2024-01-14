using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;

public class SceneSystemWizard : EditorWindow
{
    [SerializeField] private VisualTreeAsset visualTreeAsset;

    private const string StateText = "State : ";
    private const string TargetPackage = "com.nkstudio.scenereference";

    private static string Key =>
        Application.companyName + "." + Application.productName + "." + nameof(SceneSystemWizard);

    private Label _stateText;
    private Button _installButton;

    [MenuItem("Tools/Scene System/Scene System Wizard")]
    public static void SceneSystemWizardWindow()
    {
        SceneSystemWizard wnd = GetWindow<SceneSystemWizard>();
        wnd.titleContent = new GUIContent("Scene System Wizard");
        wnd.minSize = new Vector2(275, 330);

        float max_x = wnd.minSize.x * 1.3f;
        float max_y = wnd.minSize.y * 1.3f;
        wnd.maxSize = new Vector2(max_x, max_y);
    }

    [InitializeOnLoadMethod]
    private static void ShowAtStartup()
    {
        if (EditorPrefs.GetBool(Key, true))
            EditorApplication.update += ShowAtStartupTask;
    }

    private static void ShowAtStartupTask()
    {
        SceneSystemWizardWindow();
        EditorApplication.update -= ShowAtStartupTask;
    }

    public void CreateGUI()
    {
        // init root
        VisualElement root = rootVisualElement;
        visualTreeAsset.CloneTree(root);

        // load ui
        Label versionLabel = root.Q<Label>("label-version");
        Toggle showAtStartupToggle = root.Q<Toggle>("toggle-ShowAtStartup");
        HelpBox infoHelpBox = root.Q<HelpBox>("helpBox-info");
        _installButton = root.Q<Button>("button-install");
        _stateText = root.Q<Label>("text-install");

        // set ui
        showAtStartupToggle.value = EditorPrefs.GetBool(Key, true);
        infoHelpBox.text = "Scene System requires Scene Reference.";
        versionLabel.text = "Version : " + GetVersion();

        // 패키지 목록을 가져오는 동안 대기
        _stateText.text = StateText + "Checking...";
        _installButton.SetEnabled(false);

        root.schedule.Execute(() =>
        {
            bool check = CheckGitUpmInstallation();
            _stateText.text = StateText + (!check ? "Not Installed" : "Installed");
            _installButton.SetEnabled(!check);
        }).ExecuteLater(500);

        _installButton.clicked += Install;

        showAtStartupToggle.RegisterValueChangedCallback(evt => EditorPrefs.SetBool(Key, evt.newValue));
    }

    /// <summary>
    /// Scene Reference를 설치합니다.
    /// </summary>
    private void Install()
    {
        _stateText.text = StateText + "Installing...";
        _installButton.SetEnabled(false);
        Client.Add("https://github.com/NK-Studio/SceneReference.git#UPM");
    }

    /// <summary>
    /// Scene Reference가 설치되어 있는지 확인합니다.
    /// </summary>
    /// <returns></returns>
    private bool CheckGitUpmInstallation()
    {
        ListRequest request = Client.List();
        while (!request.IsCompleted)
        {
            // 패키지 목록을 가져오는 동안 대기
            _stateText.text = StateText + "Checking...";
            _installButton.SetEnabled(false);
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

    /// <summary>
    /// Scene System의 버전을 가져옵니다.
    /// </summary>
    /// <returns></returns>
    private string GetVersion()
    {
        string path = AssetDatabase.GUIDToAssetPath("247393281fb1d493086b31cd293f0d27");
        TextAsset packageJson = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        PackageInfo info = JsonUtility.FromJson<PackageInfo>(packageJson.text);

        return info.version;
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