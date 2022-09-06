#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[System.Reflection.Obfuscation(Exclude = true)]
public class ILRuntimeCLRBinding
{
    private const string GeneratePath = "Assets/Scripts/ILRuntime/Generated";
    private const string DllPath = "GameLib/Fishing.dll";
    /// <summary>
    /// 根据热更里面使用的类型进行添加的绑定
    /// 注：使用Unity ILRuntime编辑器工具所带功能进行CLR绑定后，跨域调用方法不再是反射方式，
    ///     而是一种类是劫持方式进行，运行效率高于反射很多倍, 并且没有GC开销
    /// <summary>
    [MenuItem("ILRuntime/Generate CLR Binding Code")]
    public static void GenerateCLRBinding()
    {
        List<Type> types = new List<Type>();

        types.Add(typeof(int));
        types.Add(typeof(float));
        types.Add(typeof(long));
        types.Add(typeof(object));
        types.Add(typeof(string));
        types.Add(typeof(Array));
        types.Add(typeof(Vector2));
        types.Add(typeof(Vector3));
        types.Add(typeof(Quaternion));
        types.Add(typeof(GameObject));
        types.Add(typeof(UnityEngine.Object));
        types.Add(typeof(Transform));
        types.Add(typeof(RectTransform));
        types.Add(typeof(Time));
        types.Add(typeof(Mathf));
        types.Add(typeof(Debug));
        //所有DLL内的类型的真实C#类型都是ILTypeInstance
        //types.Add(typeof(TCPClient));
        types.Add(typeof(AssetsMgr));
        //types.Add(typeof(Bezier));
        //types.Add(typeof(BezierPath));
        //types.Add(typeof(FishRoute));

        // unity
        types.Add(typeof(Physics2D));

        // pb
        types.Add(typeof(Google.Protobuf.CodedOutputStream));
        types.Add(typeof(Google.Protobuf.CodedInputStream));
        types.Add(typeof(List<ILRuntime.Runtime.Intepreter.ILTypeInstance>));

        ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(types, GeneratePath);
        AssetDatabase.Refresh();
    }

    //根据热更dll使用的类型，自动进行全部绑定(建议使用)
    [MenuItem("ILRuntime/Generate CLR Binding Code by Analysis")]
    public static void GenerateCLRBindingByAnalysis()
    {
        //用新的分析热更dll调用引用来生成绑定代码
        ILRuntime.Runtime.Enviorment.AppDomain domain = new ILRuntime.Runtime.Enviorment.AppDomain();
        FileStream fs = new FileStream(DllPath, FileMode.Open, FileAccess.Read);
        domain.LoadAssembly(fs);
        //Crossbind Adapter is needed to generate the correct binding code
        InitILRuntime(domain);
        ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(domain, GeneratePath);

        AssetDatabase.Refresh();
    }

    static void InitILRuntime(ILRuntime.Runtime.Enviorment.AppDomain domain)
    {
        //这里需要注册所有热更DLL中用到的跨域继承Adapter，否则无法正确抓取引用
        domain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
        domain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
    }
}
#endif
