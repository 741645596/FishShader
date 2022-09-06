using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using System.IO;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using System;
using System.Text.RegularExpressions;
using ILRuntime.Mono.Cecil.Pdb;
using UnityEngine.Networking;

public class ILRuntimeManager
{
    static ILRuntimeManager sInstance;
    public static ILRuntimeManager Instance { get { if (sInstance == null) { sInstance = new ILRuntimeManager(); } return sInstance; } }

    public System.Action<string> DelegateAction;

    private ILRuntime.Runtime.Enviorment.AppDomain m_AppDomain;
    public ILRuntime.Runtime.Enviorment.AppDomain ILRunAppDomain
    {
        get { return m_AppDomain; }
    }

    private Dictionary<int, long> dic1 = null;
    private Dictionary<int, Vector3> dic2 = null;
    private Dictionary<int, bool> dic3 = null;
    private Dictionary<int, float> dic5;
    private Dictionary<int, Color> dic6;

    public void NewTypeForProto3()
    {
        dic1 = new Dictionary<int, long>();
        dic2 = new Dictionary<int, Vector3>();
        dic3 = new Dictionary<int, bool>();
        dic5 = new Dictionary<int, float>();
        dic6 = new Dictionary<int, Color>();
    }

    public object PType_CreateInstance(string typeName)
    {
        return ILRunAppDomain.Instantiate(typeName);
    }

    public Type PType_GetRealType(object o)
    {
        var type = o.GetType();
        if (type.FullName == "ILRuntime.Runtime.Intepreter.ILTypeInstance")
        {
            var ilo = o as ILRuntime.Runtime.Intepreter.ILTypeInstance;
            type = ProtoBuf.PType.FindType(ilo.Type.FullName);
        }
        return type;
    }

    //public void Init()
    //{
    //    LoadFishLogicAssembly();
    //}

    public IEnumerator LoadFishLogicAssembly(string dllfile, string pdbfile)
    {
        //整个工程只有一个ILRuntime的AppDomain
        m_AppDomain = new ILRuntime.Runtime.Enviorment.AppDomain();
        LogUtils.I("LoadFishLogicAssembly:" + dllfile);
        // 读取dll
        MemoryStream dllstream = null;
        yield return AssetsMgr.LoadDll(dllfile, (stream) =>
        {
            dllstream = stream;
        });

        // 读取pdb
        MemoryStream pdbstream = null;
#if UNITY_EDITOR
        if (!String.IsNullOrEmpty(pdbfile))
        {
            yield return AssetsMgr.LoadDll(pdbfile, (stream) =>
            {
                pdbstream = stream;
            });
        }
#endif

        m_AppDomain.LoadAssembly(dllstream, pdbstream, new PdbReaderProvider());

        InitializeIlRuntime();

        if (Application.isEditor)
        {
            StartDebugService();
        }
        yield return null;
    }

    public void StartDebugService()
    {
        m_AppDomain.DebugService.StartDebugService(56000);
    }

    public void StoptartDebugService()
    {
        m_AppDomain.DebugService.StopDebugService();
    }

    void InitializeIlRuntime()
    {
        //默认委托注册仅仅支持系统自带的Action以及Function
        m_AppDomain.DelegateManager.RegisterMethodDelegate<bool>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<int>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<float>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<string>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<int, bool>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<System.Object[]>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<global::Message>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<Spine.TrackEntry>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<System.Int32[]>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<LitJson.JsonData>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<System.String, System.Boolean, System.String>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<System.String, UnityEngine.Object, System.Object>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<System.String, UnityEngine.Object>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Networking.UnityWebRequest>();
        m_AppDomain.DelegateManager.RegisterFunctionDelegate<int, string>();
		m_AppDomain.DelegateManager.RegisterMethodDelegate<UnityEngine.GameObject, System.Int32>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<UnityEngine.EventSystems.BaseEventData>(); 
        m_AppDomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Texture2D>();   
        m_AppDomain.DelegateManager.RegisterFunctionDelegate<System.Text.RegularExpressions.Match, System.String>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<UnityEngine.GameObject>();        m_AppDomain.DelegateManager.RegisterFunctionDelegate<System.Collections.Generic.KeyValuePair<System.Int32, System.Double>, System.Int32>();        m_AppDomain.DelegateManager.RegisterMethodDelegate<System.Int64>();        m_AppDomain.DelegateManager.RegisterMethodDelegate<System.Int32, UnityEngine.RectTransform>();        m_AppDomain.DelegateManager.RegisterFunctionDelegate<System.Int32>();        m_AppDomain.DelegateManager.RegisterFunctionDelegate<System.Int32, UnityEngine.Vector2>();        m_AppDomain.DelegateManager.RegisterMethodDelegate<UnityEngine.AudioClip>();
        //自定义委托或Unity委托注册
        m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<bool>>((action)=> 
        {
            return new UnityEngine.Events.UnityAction<bool>((a)=> 
            {
                ((System.Action<bool>)action)(a);
            });
        });

        m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((action) =>
        {
            return new UnityEngine.Events.UnityAction(() =>
            {
                ((System.Action)action)();
            });
        });
        m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<System.String>>((act) =>
        {
            return new UnityEngine.Events.UnityAction<System.String>((arg0) =>
            {
                ((Action<System.String>)act)(arg0);
            });
        });

        m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData>>((act) =>
        {
            return new UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData>((arg0) =>
            {
                ((Action<UnityEngine.EventSystems.BaseEventData>)act)(arg0);
            });
        });
        m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<UnityEngine.Texture2D>>((action) =>
        {
            return new UnityEngine.Events.UnityAction<UnityEngine.Texture2D>((a) =>
            {
                ((System.Action<UnityEngine.Texture2D>)action)(a);
            });
        });
        m_AppDomain.DelegateManager.RegisterDelegateConvertor<MatchEvaluator>((match) =>
        {
            return new MatchEvaluator((a) =>
            {
                return ((Func<System.Text.RegularExpressions.Match, System.String>)match)(a);
            });
        });

        m_AppDomain.DelegateManager.RegisterDelegateConvertor<DG.Tweening.TweenCallback>((act) =>
        {
            return new DG.Tweening.TweenCallback(() =>
            {
                ((Action)act)();
            });
        });
        m_AppDomain.DelegateManager.RegisterDelegateConvertor<Spine.AnimationState.TrackEntryDelegate>((act) =>
        {
            return new Spine.AnimationState.TrackEntryDelegate((trackEntry) =>
            {
                ((Action<Spine.TrackEntry>)act)(trackEntry);
            });
        });
        m_AppDomain.DelegateManager.RegisterFunctionDelegate<ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>();
        m_AppDomain.DelegateManager.RegisterDelegateConvertor<System.Comparison<ILRuntime.Runtime.Intepreter.ILTypeInstance>>((act) =>
        {
            return new System.Comparison<ILRuntime.Runtime.Intepreter.ILTypeInstance>((x, y) =>
            {
                return ((Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)act)(x, y);
            });
        });

        m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<System.Single>>((act) =>
        {
            return new UnityEngine.Events.UnityAction<System.Single>((arg0) =>
            {
                ((Action<System.Single>)act)(arg0);
            });
        });


        //跨域继承的注册
        m_AppDomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());        //协程适配器
        m_AppDomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());    //Mono适配器

        SetupCLRAddCompontent();
        SetUpCLRGetCompontent();

        // 注册json
        LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(ILRuntimeManager.Instance.ILRunAppDomain);

        // 关于pb3
        m_AppDomain.DelegateManager.RegisterMethodDelegate<Google.Protobuf.CodedOutputStream>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<Google.Protobuf.CodedInputStream>();

        m_AppDomain.DelegateManager.RegisterFunctionDelegate<System.Int32, System.Int32, System.Int32>();
        m_AppDomain.DelegateManager.RegisterDelegateConvertor<System.Comparison<System.Int32>>((act) =>
        {
            return new System.Comparison<System.Int32>((x, y) =>
            {
                return ((Func<System.Int32, System.Int32, System.Int32>)act)(x, y);
            });
        });


        ProtoBuf.PType.RegisterFunctionCreateInstance(PType_CreateInstance);
        ProtoBuf.PType.RegisterFunctionGetRealType(PType_GetRealType);

        //绑定注册 (最后执行)
#if !UNITY_EDITOR
        //ILRuntime.Runtime.Generated.CLRBindings.Initialize(m_AppDomain);
#endif
    }

    unsafe void SetUpCLRGetCompontent()
    {
        var arr = typeof(GameObject).GetMethods();
        foreach (var i in arr)
        {
            if (i.Name == "GetCompontent" && i.GetGenericArguments().Length == 1)
            {
                m_AppDomain.RegisterCLRMethodRedirection(i, GetCompontent);
            }
        }
    }

    private unsafe StackObject* GetCompontent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
    {
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

        var ptr = __esp - 1;
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
            throw new System.NullReferenceException();

        __intp.Free(ptr);

        var genericArgument = __method.GenericArguments;
        if (genericArgument != null && genericArgument.Length == 1)
        {
            var type = genericArgument[0];
            object res = null;
            if (type is CLRType)
            {
                res = instance.GetComponent(type.TypeForCLR);
            }
            else
            {
                //Debug.Log("TTTTT: " + type.BaseType.FullName);

                var clrInstances = instance.GetComponents<MonoBehaviourAdapter.Adaptor>();
                foreach (var clrInstance in clrInstances)
                {
                    if (clrInstance.ILInstance != null)
                    {
                        if (clrInstance.ILInstance.Type == type)
                        {
                            res = clrInstance.ILInstance;
                            break;
                        }
                    }
                }
            }

            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        return __esp;
    }

    unsafe void SetupCLRAddCompontent()
    {
        var arr = typeof(GameObject).GetMethods();

        foreach (var i in arr)
        {
            if (i.Name == "AddComponent" && i.GetGenericArguments().Length == 1)
            {
                m_AppDomain.RegisterCLRMethodRedirection(i, AddCompontent);
            }
        }
    }

    private unsafe StackObject* AddCompontent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
    {
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

        var ptr = __esp - 1;
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
        {
            throw new System.NullReferenceException();
        }
        __intp.Free(ptr);

        var genericArgument = __method.GenericArguments;
        if (genericArgument != null && genericArgument.Length == 1)
        {
            var type = genericArgument[0];
            object res;
            if (type is CLRType)    //CLRType表示这个类型是Unity工程里的类型   //ILType表示是资源检查dll里面的类型
            {
                //Unity主工程的类，不需要做处理
                res = instance.AddComponent(type.TypeForCLR);
            }
            else
            {
                //创建出来MonoTest
                var ilInstance = new ILTypeInstance(type as ILType, false);

                var clrInstance = instance.AddComponent<MonoBehaviourAdapter.Adaptor>();

                clrInstance.ILInstance = ilInstance;
                clrInstance.AppDomain = __domain;

                //这个实例默认创建的CLRInstance不是通过AddCompontent出来的有效实例，所以要替换
                ilInstance.CLRInstance = clrInstance;

                res = clrInstance.ILInstance;

                //补掉Awake
                clrInstance.Awake();

                //补 OnEnable
                clrInstance.OnEnable();
            }

            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        return __esp;
    }
}
