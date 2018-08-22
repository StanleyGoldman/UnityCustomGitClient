﻿using System;
using GitHub.Unity;
using UnityEditor;
using UnityEngine;

public class CustomGitEditor : EditorWindow
{
    [MenuItem("Window/Custom Git")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CustomGitEditor));
    }

    [NonSerialized] private bool initialized = false;
    [NonSerialized] private GitClient gitClient;
    [NonSerialized] private string unityApplication;

    public void OnEnable()
    {
        if (!initialized)
            InitGitClient();
    }

    private void InitGitClient()
    {
        Debug.Log("Custom Git Window Started");
        if(gitClient == null)
        {
            var cacheContainer = new CacheContainer();
            var defaultEnvironment = new DefaultEnvironment(cacheContainer);
            string unityAssetsPath = null;
            string unityApplicationContents = null;
            string unityVersion = null;
            NPath extensionInstallPath = default(NPath);
            if (unityApplication == null)
            {
                unityAssetsPath = Application.dataPath;
                unityApplication = EditorApplication.applicationPath;
                unityApplicationContents = EditorApplication.applicationContentsPath;
                extensionInstallPath = DetermineInstallationPath();
                unityVersion = Application.unityVersion;
            }

            defaultEnvironment.Initialize(unityVersion, extensionInstallPath, unityApplication.ToNPath(),
                unityApplicationContents.ToNPath(), unityAssetsPath.ToNPath());

            var taskManager = new TaskManager();
            var processEnvironment = new ProcessEnvironment(defaultEnvironment);
            var processManager = new ProcessManager(defaultEnvironment, processEnvironment, taskManager.Token);

            gitClient = new GitClient(defaultEnvironment, processManager, taskManager.Token);
        }
    }

    private NPath DetermineInstallationPath()
    {
        // Juggling to find out where we got installed
        var shim = CreateInstance<RunLocationShim>();
        var script = MonoScript.FromScriptableObject(shim);
        var scriptPath = Application.dataPath.ToNPath().Parent.Combine(AssetDatabase.GetAssetPath(script).ToNPath());
        DestroyImmediate(shim);
        return scriptPath.Parent;
    }

    class RunLocationShim : ScriptableObject
    {
    }

    public virtual void OnDisable()
    { }

    void OnGUI()
    {
        GUILayout.Label("Custom Git", EditorStyles.boldLabel);

        if (GUILayout.Button("Commit Stuff"))
        {
            gitClient.AddAll().Then(gitClient.Commit(DateTime.Now.ToString(), string.Empty)).Start();
        }
    }

    void Update () {
		
	}
}