#if UNITY_2018_1_OR_NEWER
using System.IO;
using Google.Protobuf.Reflection;
using ProtoBuf.Reflection;
using UnityEditor;
using UnityEngine;

public class ProtoGenWindow : EditorWindow
{
    private string protosDirectory;
	private string outputPath;

    public void OnGUI()
	{
        if(string.IsNullOrEmpty(protosDirectory))
        {
		    protosDirectory = Path.Combine(Application.dataPath, "../Config/protos");
			outputPath = Path.Combine(Application.dataPath, "../Games/FishLogic/NetWork/MessageDefine");
        }

		EditorGUILayout.LabelField("输出路径：");

		EditorGUILayout.BeginHorizontal();
		outputPath = EditorGUILayout.TextField(outputPath, GUILayout.MaxWidth(320), GUILayout.MaxHeight(20));
		if (GUILayout.Button("更改", GUILayout.MaxWidth(80)))
		{
			outputPath = EditorUtility.OpenFolderPanel("选择目录", outputPath, "");
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Assets/Protos 文件列表：");

		DirectoryInfo info = new DirectoryInfo(protosDirectory);
		var fis = info.GetFiles("*.proto");
        for (int i = 0; i < fis.Length; i++)
        {
			var file = fis[i];
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(file.Name, GUILayout.MaxWidth(320), GUILayout.MaxHeight(20));
			if (GUILayout.Button("生成", GUILayout.MaxWidth(80)))
			{
				Generate(file.Directory.FullName, new string[] { file.Name }, outputPath);
			}
			EditorGUILayout.EndHorizontal();
		}
	}

	public static void Generate(string inpath, string[] inprotos, string outpath)
	{

		if (Directory.Exists(outpath) == false)
		{
			Debug.Log(outpath + " 目录不存在");
			return;
		}
		var set = new FileDescriptorSet();

		set.AddImportPath(inpath);
		foreach (var inproto in inprotos)
		{
			set.Add(inproto, true);
		}

		set.Process();
		var errors = set.GetErrors();

		CSharpCodeGenerator.ClearTypeNames();
		var files = CSharpCodeGenerator.Default.Generate(set);

		int idx = 1;
		foreach (var file in files)
		{
			EditorUtility.DisplayProgressBar("Generate", file.Name, idx / (1.0f * inprotos.Length));
			var path = Path.Combine(outpath, file.Name);
			File.WriteAllText(path, file.Text);

			Debug.Log($"generated: {path}");
		}
		EditorUtility.ClearProgressBar();
	}
}

public class Protogen
{
	[MenuItem("Tools/Generate Protocs")]
	public static void Test()
	{
		var window = ProtoGenWindow.GetWindow<ProtoGenWindow>();
		window.minSize = new Vector2(400, 450);
		window.maxSize = new Vector2(400, 450);
		window.Show();
	}
}
#endif