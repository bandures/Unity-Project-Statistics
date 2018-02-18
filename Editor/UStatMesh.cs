using UnityEngine;
using UnityEditor;
using OfficeOpenXml;
using System.IO;
using System;
using UnityEngine.Profiling;

public class UStatMesh : MonoBehaviour
{
    private static readonly int N_BASE_COL = 13;

    private static string rootDir;
    private static int progress = 0;
    private static string[] assets;

    private static int excelRow = 0;
    private static ExcelPackage excelFile = null;
    private static ExcelWorksheet excelWorksheet = null;

    [MenuItem("UTools/Statistics/Meshes")]
    static void Run()
    {
        rootDir = Directory.GetParent(Application.dataPath).ToString();

        progress = 0;
        assets = AssetDatabase.FindAssets("t:Model");

        FileInfo newFile = new FileInfo(rootDir + @"\stats-models-" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".xlsx");
        excelRow = 2;
        excelFile = new ExcelPackage(newFile);
        excelWorksheet = excelFile.Workbook.Worksheets.Add("Content");
        excelWorksheet.SetValue(1, 1, "Name");
        excelWorksheet.SetValue(1, 2, "Verts");
        excelWorksheet.SetValue(1, 3, "Tris");
        excelWorksheet.SetValue(1, 4, "LODs");
        excelWorksheet.SetValue(1, 5, "Export animations");
        excelWorksheet.SetValue(1, 6, "Animations clips");
        excelWorksheet.SetValue(1, 7, "Mesh Compression");
        excelWorksheet.SetValue(1, 8, "Animation Compression");
        excelWorksheet.SetValue(1, 9, "AssetBundle");
        excelWorksheet.SetValue(1, 10, "R/W Enabled");
        excelWorksheet.SetValue(1, 11, "Size after export");
        excelWorksheet.SetValue(1, 12, "Animation type");
        excelWorksheet.SetValue(1, 13, "Transforms");
        excelWorksheet.SetValue(1, 14, "Human - human");
        excelWorksheet.SetValue(1, 15, "Human - skeleton");
        excelWorksheet.SetValue(1, 16, "Profiler Size In Game");

        EditorApplication.update += ProcessStep;
    }

    static void Stop()
    {
        EditorApplication.update -= ProcessStep;
        EditorUtility.ClearProgressBar();

        assets = null;
        progress = 0;

        excelFile.Save();
        excelFile = null;
        excelWorksheet = null;
    }

    static void ProcessStep()
    {
        if ((progress >= assets.Length) || EditorUtility.DisplayCancelableProgressBar("Collecting stats", "" + progress + "/" + assets.Length, (float)progress / assets.Length))
        {
            Stop();
            return;
        }

        var item = assets[progress];
        ++progress;

        var path = AssetDatabase.GUIDToAssetPath(item);
        ModelImporter modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;
        if (modelImporter == null)
            return;

        excelWorksheet.SetValue(excelRow, 1, path.Substring(6));
        excelWorksheet.SetValue(excelRow, 5, modelImporter.importAnimation ? "True" : "False");
        excelWorksheet.SetValue(excelRow, 6, modelImporter.clipAnimations.Length);
        excelWorksheet.SetValue(excelRow, 7, modelImporter.meshCompression.ToString());
        excelWorksheet.SetValue(excelRow, 8, modelImporter.animationCompression.ToString());
        excelWorksheet.SetValue(excelRow, 9, modelImporter.assetBundleName);
        excelWorksheet.SetValue(excelRow, 10, modelImporter.isReadable ? "True" : "False");

        var originalSize = new FileInfo(Path.Combine(rootDir, path)).Length;
        var exppath = Path.Combine(rootDir, "Library/metadata/" + item.Substring(0, 2) + "/" + item);
        var exportedSize = new FileInfo(exppath).Length;
        //excelWorksheet.SetValue(excelRow, 6, originalSize);
        excelWorksheet.SetValue(excelRow, 11, exportedSize);

        var model = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;

        int verts = 0;
        uint indices = 0;
        ReportGameObject(model, ref verts, ref indices);
        excelWorksheet.SetValue(excelRow, 2, verts);
        excelWorksheet.SetValue(excelRow, 3, indices);

        var loadGroup = model.GetComponent<LODGroup>();
        if (loadGroup)
            excelWorksheet.SetValue(excelRow, 4, loadGroup.lodCount);

        excelWorksheet.SetValue(excelRow, 12, modelImporter.animationType.ToString());
        if (modelImporter.transformPaths != null)
            excelWorksheet.SetValue(excelRow, 13, modelImporter.transformPaths.Length);
        if (modelImporter.humanDescription.human != null)
            excelWorksheet.SetValue(excelRow, 14, modelImporter.humanDescription.human.Length);
        if (modelImporter.humanDescription.skeleton != null)
            excelWorksheet.SetValue(excelRow, 15, modelImporter.humanDescription.human.Length);

        excelWorksheet.SetValue(excelRow, 16, Profiler.GetRuntimeMemorySizeLong(model));

        ++excelRow;
        if (excelRow % 100 == 0)
            Resources.UnloadUnusedAssets();
    }

    private static void ReportGameObject(GameObject root, ref int verts, ref uint indices)
    {
        var meshFilter = root.GetComponent<MeshFilter>();
        if (meshFilter)
        {
            var mesh = meshFilter.sharedMesh;
            verts += mesh.vertexCount;
            for (int temp = 0; temp < mesh.subMeshCount; ++temp)
                indices += mesh.GetIndexCount(temp);
        }

        for (int temp = 0; temp < root.transform.childCount; ++temp)
        {
            var child = root.transform.GetChild(temp).gameObject;
            ReportGameObject(child, ref verts, ref indices);
        }
    }
}
