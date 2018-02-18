using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class HierarchyStatistics : EditorWindow
{
    Vector2 m_Scroll;
    Dictionary<Type, int> m_Data;

    [MenuItem("UTools/Hierarchy Statistics")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow(typeof(HierarchyStatistics)) as HierarchyStatistics;
        window.Show();
    }

    private void OnSelectionChange()
    {
        m_Data = new Dictionary<Type, int>();
        foreach (var obj in Selection.gameObjects)
            AddStats(obj);

        this.Repaint();
    }

    private void AddStats(GameObject root)
    {
        Increment(root.GetType());

        var components = root.GetComponents<Component>();
        foreach (var comp in components)
        {
            if (comp == null)
                continue;

            Increment(comp.GetType());
        }

        for (int temp = 0; temp < root.transform.childCount; ++temp)
            AddStats(root.transform.GetChild(temp).gameObject);
    }

    private void Increment(Type type)
    {
        if (!m_Data.ContainsKey(type))
            m_Data[type] = 0;

        m_Data[type] += 1;
    }

    public void OnGUI()
    {
        GUILayout.Space(3);
        if (m_Data != null)
        {
            m_Scroll = GUILayout.BeginScrollView(m_Scroll);
            float width = EditorGUIUtility.currentViewWidth;
            foreach (var item in m_Data)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(item.Key.Name);
                EditorGUILayout.LabelField("" + item.Value);

                var style = GUI.skin.GetStyle("IN Title");
                Rect lineRect = GUILayoutUtility.GetRect(10, 4, style);
                Rect linePos = new Rect(0, lineRect.y, width, 1);
                Rect uv = new Rect(0, 1f, 1, 1f - 1f / style.normal.background.height);
                GUI.DrawTextureWithTexCoords(linePos, style.normal.background, uv);

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
    }
}