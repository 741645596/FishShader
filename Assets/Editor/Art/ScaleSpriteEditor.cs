using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ScaleSpriteEditor : EditorWindow
{

    [MenuItem("Tools/图片尺寸工具")]
    public static void ShowWindow()
    {
        var window = GetWindow<ScaleSpriteEditor>();
        window.titleContent = new GUIContent("缩放图片工具");
        window.Show();
        window.Focus();
    }

    private string folderName = "FishTextures";
    private string magickPath = "";

    private float[] checkLabels = new float[5] { 128, 256, 512, 1024, 2048 };
    private bool[] checkValues = new bool[5] {true, true, true, true, false };

    private void OnGUI()
    {
        folderName = EditorGUILayout.TextField("贴图目录名", folderName);
        magickPath = EditorGUILayout.TextField("Magick路径", magickPath);

        EditorGUILayout.LabelField("需要生成的图片尺寸:");
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < 5; i++)
        {
            checkValues[i] = EditorGUILayout.ToggleLeft($"{checkLabels[i]}", checkValues[i], GUILayout.Width(60));
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("生成图片"))
        {
            if (string.IsNullOrEmpty(magickPath))
            {
                EditorUtility.DisplayDialog("提示", "还没有设置magick路径", "确定");
                return;
            }
            var resRoot = Path.Combine(Application.dataPath, "GameData");
            var spriteDirs = Directory.GetDirectories(resRoot, folderName, SearchOption.AllDirectories);

            var exts = new string[] { "*.png", "*.tga", "*.jpg" };

            for (int i = 0; i < spriteDirs.Length; i++)
            {
                var files = new List<string>();
                for (int e = 0; e < exts.Length; e++)
                {
                    files.AddRange(Directory.GetFiles(spriteDirs[i], exts[e], SearchOption.TopDirectoryOnly));
                }
                foreach (var file in files)
                {

                    if (file.Contains("@"))
                    {
                        continue;
                    }

                    for (int j = 0; j < checkLabels.Length; j++)
                    {
                        if (checkValues[j] == false)
                        {
                            continue;
                        }
                        var size = checkLabels[j];
                        var info = new FileInfo(file);
                        var ext = info.Extension;

                        Process.Start(magickPath, $"{file} -resize '{size}x{size}' {file.Replace($"{ext}", $"@{size}{ext}")}");
                    }
                }

                EditorUtility.DisplayProgressBar("正在处理图片尺寸...", spriteDirs[i], i / (float)spriteDirs.Length);
            }
            EditorUtility.ClearProgressBar();
        }

        if (GUILayout.Button("删除生成的图片"))
        {
            var resRoot = Path.Combine(Application.dataPath, "GameData");
            var spriteDirs = Directory.GetDirectories(resRoot, folderName, SearchOption.AllDirectories);

            for (int i = 0; i < spriteDirs.Length; i++)
            {
                var files = Directory.GetFiles(spriteDirs[i], "*.png|*.tag|*.jpg", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    if (file.Contains("@"))
                    {
                        File.Delete(file);
                    }
                }
            }
        }
    }
}
