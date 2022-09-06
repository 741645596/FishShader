using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;

public class FntToolWindow: EditorWindow
{
    [MenuItem("Tools/Fnt转FontSettings工具")]
    public static void DoIt()
    {
        var window = GetWindow<FntToolWindow>();
        window.minSize = new Vector2(400, 300);
        window.maxSize = new Vector2(400, 300);
        window.Show();

    }

    string inputPath;
    string outputPath;
    int spacing;
    private void OnGUI()
    {
        if (string.IsNullOrEmpty(inputPath))
        {
            inputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
            outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, ""));
        }
        EditorGUILayout.LabelField("fnt路径：");
        EditorGUILayout.BeginHorizontal();
        inputPath = EditorGUILayout.TextField(inputPath, GUILayout.MaxWidth(320), GUILayout.MaxHeight(20));
        if (GUILayout.Button("更改", GUILayout.MaxWidth(80)))
        {
            inputPath = EditorUtility.OpenFilePanel("选择目录", inputPath, "*.*");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("导出路径：");
        EditorGUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField(outputPath, GUILayout.MaxWidth(320), GUILayout.MaxHeight(20));
        if (GUILayout.Button("更改", GUILayout.MaxWidth(80)))
        {
            outputPath = EditorUtility.OpenFolderPanel("选择目录", outputPath, "");
        }
        EditorGUILayout.EndHorizontal();
        spacing = EditorGUILayout.IntField("字体间隔", spacing);
        if (GUILayout.Button("开始创建"))
        {
            CreateFont();
        }

    }

    private void CreateFont()
    {
        if (Path.GetExtension(inputPath) != ".fnt")
        {
            Debug.LogError("无效的fnt路径,请选择fnt为后缀的文件");
            return;
        }
        Debug.Log("开始创建");
        string fntFilePath = inputPath;
        string fntName = Path.GetFileName(inputPath).Replace(".fnt", "");
        string texturePath = Path.ChangeExtension(inputPath, ".png");
        string dstTexturePath = outputPath + "/" + fntName + ".png";
        File.Copy(texturePath, dstTexturePath, true);
        dstTexturePath = dstTexturePath.Replace(Application.dataPath, "Assets");
        Debug.Log(dstTexturePath);
        AssetDatabase.Refresh();

        var list = new List<CharacterInfo>();

        // 适合不是xml格式文本模式
        var file = new FileStream(fntFilePath, FileMode.Open);
        StreamReader reader = new StreamReader(file);
        List<CharacterInfo> charList = new List<CharacterInfo>();
        Regex reg = new Regex(@"char  id=(?<id>\d+)\s+x=(?<x>\d+)\s+y=(?<y>\d+)\s+width=(?<width>\d+)\s+height=(?<height>\d+)\s+xoffset=(?<xoffset>(-|\d)+)\s+yoffset=(?<yoffset>(-|\d)+)\s+xadvance=(?<xadvance>\d+)\s+");
        string line = reader.ReadLine();
        int lineHeight = 65;
        int texWidth = 512;
        int texHeight = 512;
        while (line != null)
        {
            if (line.IndexOf("char  id=") != -1)
            {
                Match match = reg.Match(line);
                if (match != Match.Empty)
                {
                    var id = System.Convert.ToInt32(match.Groups["id"].Value);
                    var x = System.Convert.ToInt32(match.Groups["x"].Value);
                    var y = System.Convert.ToInt32(match.Groups["y"].Value);
                    var width = System.Convert.ToInt32(match.Groups["width"].Value);
                    var height = System.Convert.ToInt32(match.Groups["height"].Value);
                    var xoffset = System.Convert.ToInt32(match.Groups["xoffset"].Value);
                    var yoffset = System.Convert.ToInt32(match.Groups["yoffset"].Value);
                    var xadvance = System.Convert.ToInt32(match.Groups["xadvance"].Value);
                    //Debug.Log("ID" + id);
                    CharacterInfo info = new CharacterInfo();
                    info.index = id;

                    float uvx = 1f * x / texWidth;
                    float uvy = 1 - (1f * y / texHeight);
                    float uvw = 1f * width / texWidth;
                    float uvh = -1f * height / texHeight;

                    info.uvBottomLeft = new Vector2(uvx, uvy);
                    info.uvBottomRight = new Vector2(uvx + uvw, uvy);
                    info.uvTopLeft = new Vector2(uvx, uvy + uvh);
                    info.uvTopRight = new Vector2(uvx + uvw, uvy + uvh);
                    info.minX = xoffset;
                    info.minY = yoffset + height / 2;
                    info.glyphWidth = width;
                    info.glyphHeight = -height;
                    info.advance = xadvance + spacing;

                    list.Add(info);
                }
            }
            else if (line.IndexOf("scaleW=") != -1)
            {
                Regex reg2 = new Regex(@"common lineHeight=(?<lineHeight>\d+)\s+.*scaleW=(?<scaleW>\d+)\s+scaleH=(?<scaleH>\d+)");
                Match match = reg2.Match(line);
                if (match != Match.Empty)
                {
                    lineHeight = System.Convert.ToInt32(match.Groups["lineHeight"].Value);
                    texWidth = System.Convert.ToInt32(match.Groups["scaleW"].Value);
                    texHeight = System.Convert.ToInt32(match.Groups["scaleH"].Value);
                }
            }
            line = reader.ReadLine();
        }
        file.Dispose();

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(dstTexturePath);
        if (tex == null)
        {
            Debug.LogError($"未找到fnt字体纹理{dstTexturePath}");
            return;
        }
        Material mat = new Material(Shader.Find("GUI/Text Shader"));
        mat.SetTexture("_MainTex", tex);
        Font font = new Font();
        font.material = mat;
        AssetDatabase.CreateAsset(mat, Path.ChangeExtension(dstTexturePath, ".mat"));
        AssetDatabase.CreateAsset(font, Path.ChangeExtension(dstTexturePath, ".fontsettings"));
        font.characterInfo = list.ToArray();

        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("创建成功！");
    }
}
