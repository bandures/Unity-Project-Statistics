using UnityEngine;
using UnityEditor;
using OfficeOpenXml;
using System.IO;
using System;
using UnityEngine.Profiling;

public class UStatAudio : MonoBehaviour
{
    private static string rootDir;
    private static int progress = 0;
    private static string[] assets;

    private static int excelRow = 0;
    private static ExcelPackage excelFile = null;
    private static ExcelWorksheet excelWorksheet = null;

    [MenuItem("UTools/Statistics/Audio")]
    static void Run()
    {
        rootDir = Directory.GetParent(Application.dataPath).ToString();

        progress = 0;
        assets = AssetDatabase.FindAssets("t:AudioClip");

        FileInfo newFile = new FileInfo(rootDir + @"\stats-audio-" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".xlsx");
        excelRow = 2;
        excelFile = new ExcelPackage(newFile);
        excelWorksheet = excelFile.Workbook.Worksheets.Add("Content");
        excelWorksheet.SetValue(1, 1, "Name");
        excelWorksheet.SetValue(1, 2, "Load Type");
        excelWorksheet.SetValue(1, 3, "Compression");
        excelWorksheet.SetValue(1, 4, "Sample Rate");
        excelWorksheet.SetValue(1, 5, "Original Size");
        excelWorksheet.SetValue(1, 6, "Exported Size");
        excelWorksheet.SetValue(1, 7, "Asset Bundle");
        excelWorksheet.SetValue(1, 8, "Load In Background");
        excelWorksheet.SetValue(1, 9, "Profiler Size In Game");

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
        AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
        if (audioImporter == null)
            return;

        excelWorksheet.SetValue(excelRow, 1, path.Substring(6));
        excelWorksheet.SetValue(excelRow, 2, audioImporter.defaultSampleSettings.loadType.ToString());
        excelWorksheet.SetValue(excelRow, 3, audioImporter.defaultSampleSettings.compressionFormat.ToString());
        excelWorksheet.SetValue(excelRow, 4, audioImporter.defaultSampleSettings.sampleRateSetting.ToString());
        excelWorksheet.SetValue(excelRow, 7, audioImporter.assetBundleName);
        excelWorksheet.SetValue(excelRow, 8, audioImporter.loadInBackground);

        var originalSize = new FileInfo(Path.Combine(rootDir, path)).Length;
        var exppath = Path.Combine(rootDir, "Library/metadata/" + item.Substring(0, 2) + "/" + item);
        var exportedSize = new FileInfo(exppath).Length;
        excelWorksheet.SetValue(excelRow, 5, originalSize);
        excelWorksheet.SetValue(excelRow, 6, exportedSize);

        var audio = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        excelWorksheet.SetValue(excelRow, 9, Profiler.GetRuntimeMemorySizeLong(audio));

        ++excelRow;
    }
}
