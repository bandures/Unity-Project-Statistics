using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

/* ========================================================================================================
 * This script allows you to find all objects with "Missing Script" inside Scenes and Prefabs
 * As well it lets you to clean individual prefabs and scenes, as well as all prefbas and scenes in project
 * 
 * */
public class CleanMissingScripts : EditorWindow
{
    [Serializable]
    class Record
    {
        public enum Type
        {
            Scene,
            Prefab
        }

        public Type type;
        public bool state;
        public string name;
        public List<string> gameObjectNames;
    }

    [SerializeField]
    Vector2 m_Scroll;
    [SerializeField]
    List<Record> m_Records = new List<Record>();

    [MenuItem("UTools/Find Missing Scripts")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CleanMissingScripts));
    }

    public void OnGUI()
    {
        GUILayout.Space(3);
        if (GUILayout.Button("Search"))
        {
            m_Records = new List<Record>();
            SearchInScenes();
            SearchInPrefabs();
        }

        if (GUILayout.Button("Clean All"))
        {
            foreach(var record in m_Records)
            {
                if (record.type == Record.Type.Prefab)
                    CleanPrefab(record.name);
                else if (record.type == Record.Type.Scene)
                    CleanScene(record.name);
            }
        }

        m_Scroll = GUILayout.BeginScrollView(m_Scroll);
        foreach (var record in m_Records)
        {
            float width = EditorGUIUtility.currentViewWidth;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            record.state = EditorGUILayout.Foldout(record.state, record.name, true);
            if (GUILayout.Button("Select", GUILayout.Width(position.width / 4 - 10)))
            {
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(record.name);
            }
            if (GUILayout.Button("Clean", GUILayout.Width(position.width / 4 - 10)))
            {
                if (record.type == Record.Type.Prefab)
                    CleanPrefab(record.name);
                else if (record.type == Record.Type.Scene)
                    CleanScene(record.name);
            }
            GUILayout.EndHorizontal();
            if (record.state)
            {
                EditorGUILayout.BeginHorizontal();
                int numColumns = Mathf.CeilToInt(width / 100);
                int numRows = Mathf.CeilToInt((float)record.gameObjectNames.Count / numColumns);
                Rect rect = GUILayoutUtility.GetRect(width, numRows * 20);
                var content = new GUIContent[record.gameObjectNames.Count];
                for (int temp = 0; temp < record.gameObjectNames.Count; ++temp)
                    content[temp] = new GUIContent(record.gameObjectNames[temp]);
                GUI.SelectionGrid(rect, 0, content, numColumns);
                EditorGUILayout.Space();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            var style = GUI.skin.GetStyle("IN Title");
            Rect lineRect = GUILayoutUtility.GetRect(10, 4, style);
            Rect linePos = new Rect(0, lineRect.y, width, 1);
            Rect uv = new Rect(0, 1f, 1, 1f - 1f / style.normal.background.height);
            GUI.DrawTextureWithTexCoords(linePos, style.normal.background, uv);

        }
        GUILayout.EndScrollView();
    }

    private void SearchInScenes()
    {
        var scenesList = GetAllAssetsOfType("Scene");

        m_Records = new List<Record>();
        foreach (string scenePath in scenesList)
        {
            var info = new Record();
            info.type = Record.Type.Scene;
            info.name = scenePath;
            info.gameObjectNames = new List<string>();

            var scene = EditorSceneManager.OpenScene(scenePath);
            foreach (var root in scene.GetRootGameObjects())
                FindMissingScrips(root, ref info.gameObjectNames);

            if (info.gameObjectNames.Count > 0)
                m_Records.Add(info);
        }
    }

    private void SearchInPrefabs()
    {
        var prefabs = GetAllAssetsOfType("Prefab");
        foreach (string prefabPath in prefabs)
        {
            var prefabObj = AssetDatabase.LoadMainAssetAtPath(prefabPath);
            var rootObject = prefabObj as GameObject;
            if (rootObject == null)
                continue;

            var info = new Record();
            info.type = Record.Type.Prefab;
            info.name = prefabPath;
            info.gameObjectNames = new List<string>();
            FindMissingScrips(rootObject, ref info.gameObjectNames);

            if (info.gameObjectNames.Count > 0)
                m_Records.Add(info);
        }
    }

    private void CleanScene(string name)
    {
        var scene = EditorSceneManager.OpenScene(name);
        foreach (var root in scene.GetRootGameObjects())
            RemoveMissingScrips(root);

        EditorSceneManager.SaveScene(scene);
    }

    private void CleanPrefab(string name)
    {
        var prefabObj = AssetDatabase.LoadMainAssetAtPath(name);
        var rootObject = prefabObj as GameObject;
        if (rootObject == null)
            return;

        RemoveMissingScrips(rootObject);

        PrefabUtility.ReplacePrefab(
                     rootObject,
                     prefabObj,
                     ReplacePrefabOptions.ReplaceNameBased
                     );
    }

    private void FindMissingScrips(GameObject obj, ref List<string> list)
    {
        var serialized = new SerializedObject(obj);
        var prop = serialized.FindProperty("m_Component");

        Component[] components = obj.GetComponents<Component>();
        for (int temp = 0; temp < components.Length; ++temp)
        {
            var cmp = components[temp];
            if (cmp != null)
                continue;

            /*
            var printProp = prop.GetArrayElementAtIndex(temp);
            var printComp = serialized.FindProperty(printProp.propertyPath + ".component");
            if (printComp != null)
            {
                //Debug.Log("S: " + printComp.propertyType + " - " + printComp.propertyPath);
                var fileID = serialized.FindProperty(printComp.propertyPath + ".m_FileID");
                var pathID = serialized.FindProperty(printComp.propertyPath + ".m_PathID");
                if (pathID != null)
                    Debug.Log("S: " + pathID.propertyType + " - " + pathID.longValue);
                if (fileID != null)
                    Debug.Log("S: " + fileID.propertyType + " - " + fileID.longValue);
            }
            */

            list.Add(obj.name);
            break;
        }

        for(int temp = 0; temp < obj.transform.childCount; ++temp)
            FindMissingScrips(obj.transform.GetChild(temp).gameObject, ref list);
    }

    private void RemoveMissingScrips(GameObject obj)
    {
        var serialized = new SerializedObject(obj);
        var prop = serialized.FindProperty("m_Component");

        int removedCount = 0;
        Component[] components = obj.GetComponents<Component>();
        for (int temp = 0; temp < components.Length; ++temp)
        {
            if (components[temp] != null)
                continue;

            prop.DeleteArrayElementAtIndex(temp - removedCount);
            removedCount++;
        }

        serialized.ApplyModifiedProperties();

        for (int temp = 0; temp < obj.transform.childCount; ++temp)
            RemoveMissingScrips(obj.transform.GetChild(temp).gameObject);
    }

    public static string[] GetAllAssetsOfType(string type)
    {
        var ret = new List<string>();
        foreach (var guid in AssetDatabase.FindAssets("t:" + type))
            ret.Add(AssetDatabase.GUIDToAssetPath(guid));

        return ret.ToArray();
    }
}
