using UnityEditor;
using UnityEngine;
using UnityEditor.Build;

[InitializeOnLoad]
public static class StopPlayOnChange
{
    static StopPlayOnChange()
    {
        EditorApplication.playmodeStateChanged += ProjectSetup;

//        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
//        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
    }

    public static void ProjectSetup()
    {
        System.Console.WriteLine("TEST");
        Debug.Log("PlayMode changed");
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        Debug.Log("Script reloaded 2");
//        if (EditorApplication.isPlaying)
//            EditorApplication.isPlaying = false;
    }

    public static void OnBeforeAssemblyReload()
    {
        Debug.Log("Before Assembly Reload 2");
        if (EditorApplication.isPlaying)
            EditorApplication.isPlaying = false;
    }

    public static void OnAfterAssemblyReload()
    {
        Debug.Log("After Assembly Reload 2");
    }
}


/*
public class MyWindow : EditorWindow
{
    [MenuItem("Test/Show My Window")]
    static void Init()
    {
        GetWindow<MyWindow>();
    }

    void OnEnable()
    {
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
    }

    void OnDisable()
    {
        AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
        AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
    }

    public void OnBeforeAssemblyReload()
    {
        Debug.Log("Before Assembly Reload");
    }

    public void OnAfterAssemblyReload()
    {
        Debug.Log("After Assembly Reload");
    }
}
*/
