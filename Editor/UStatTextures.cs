using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OfficeOpenXml;
using System.IO;
using System;
using UnityEngine.Profiling;

public class UStatTextures : MonoBehaviour
{
    private static string rootDir;
    private static int progress = 0;
    private static string[] assets;

    private static int excelRow = 0;
    private static ExcelPackage excelFile = null;
    private static ExcelWorksheet excelWorksheet = null;

    [MenuItem("UTools/Statistics/Textures")]
    static void Run()
    {
        rootDir = Directory.GetParent(Application.dataPath).ToString();

        progress = 0;
        assets = AssetDatabase.FindAssets("t:Texture");

        FileInfo newFile = new FileInfo(rootDir + @"\stats-textures-" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".xlsx");
        excelRow = 2;
        excelFile = new ExcelPackage(newFile);
        excelWorksheet = excelFile.Workbook.Worksheets.Add("Content");
        excelWorksheet.SetValue(1, 1, "Name");
        excelWorksheet.SetValue(1, 2, "Width");
        excelWorksheet.SetValue(1, 3, "Height");
        excelWorksheet.SetValue(1, 4, "Depth");
        excelWorksheet.SetValue(1, 5, "Format");
        excelWorksheet.SetValue(1, 6, "Original Size");
        excelWorksheet.SetValue(1, 7, "Exported Size");
        excelWorksheet.SetValue(1, 8, "Asset Bundle");
        excelWorksheet.SetValue(1, 9, "R/W Enabled");
        excelWorksheet.SetValue(1, 10, "Type");
        excelWorksheet.SetValue(1, 11, "Cranch");
        excelWorksheet.SetValue(1, 12, "Profiler Size In Game");

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
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        if (textureImporter == null)
            return;

        excelWorksheet.SetValue(excelRow, 1, path.Substring(6));
        excelWorksheet.SetValue(excelRow, 8, textureImporter.assetBundleName);
        excelWorksheet.SetValue(excelRow, 9, textureImporter.isReadable ? "True" : "False");
        excelWorksheet.SetValue(excelRow, 10, textureImporter.textureType.ToString());
        excelWorksheet.SetValue(excelRow, 11, textureImporter.crunchedCompression ? "True" : "False");

        var originalSize = new FileInfo(Path.Combine(rootDir, path)).Length;
        var exppath = Path.Combine(rootDir, "Library/metadata/" + item.Substring(0, 2) + "/" + item);
        var exportedSize = new FileInfo(exppath).Length;
        excelWorksheet.SetValue(excelRow, 6, originalSize);
        excelWorksheet.SetValue(excelRow, 7, exportedSize);

        var texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
        if (texture.dimension == UnityEngine.Rendering.TextureDimension.Tex2D)
        {
            var tex2D = texture as Texture2D;
            excelWorksheet.SetValue(excelRow, 2, tex2D.width);
            excelWorksheet.SetValue(excelRow, 3, tex2D.height);
            excelWorksheet.SetValue(excelRow, 5, tex2D.format.ToString());
        }
        else if (texture.dimension == UnityEngine.Rendering.TextureDimension.Tex3D)
        {
            var tex3D = texture as Texture3D;
            excelWorksheet.SetValue(excelRow, 2, tex3D.width);
            excelWorksheet.SetValue(excelRow, 3, tex3D.height);
            excelWorksheet.SetValue(excelRow, 4, tex3D.depth);
            excelWorksheet.SetValue(excelRow, 5, tex3D.format.ToString());
        }
        else if (texture.dimension == UnityEngine.Rendering.TextureDimension.Cube)
        {
            var cubemap = texture as Cubemap;
            excelWorksheet.SetValue(excelRow, 2, cubemap.width);
            excelWorksheet.SetValue(excelRow, 3, cubemap.height);
            excelWorksheet.SetValue(excelRow, 4, 6);
            excelWorksheet.SetValue(excelRow, 5, cubemap.format.ToString());
        }
        else
            Debug.Log("Unknown texture");

        excelWorksheet.SetValue(excelRow, 12, Profiler.GetRuntimeMemorySizeLong(texture));

        ++excelRow;
        Resources.UnloadAsset(texture);
    }
}
