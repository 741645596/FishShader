using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

//#define DIRECT_LOAD_DLL

namespace Utils {
    public class AppDomainUtil {

        public static object InvokeInstanceMethod(string className, string funName, params object[] list)
        {
            object[] objects = list;
#if DIRECT_LOAD_DLL
            AppDomain domain = AppDomain.CurrentDomain;
            return DirectInvoke(domain, className, funName, objects);
#else
            var appDomain = ILRuntimeManager.Instance.ILRunAppDomain;
            var startup = appDomain.Instantiate(className);
            return appDomain.Invoke(className, funName, startup, objects);
#endif
        }

        public static object InvokeStaticMethod(string strStaticMethod, params object[] list) {
            var strings = strStaticMethod.Split('.');
            var str_lst = strings.ToList();

            var last = str_lst.Count - 1;

            var funName = str_lst[last];
            str_lst.RemoveAt(last);

            var className = String.Join(".", str_lst);
            
#if DIRECT_LOAD_DLL
                AppDomain domain = AppDomain.CurrentDomain;
                return DirectInvoke(domain, className, funName, list);
#else
            var appDomain = ILRuntimeManager.Instance.ILRunAppDomain;
            var rslt = appDomain.Invoke(className, funName, null, list);
            return rslt;
#endif   
        }

        //InvokeStaticMethod
        public static object HandleCallBack(string className, string funName, string result) {
            return InvokeStaticMethod(className + "." + funName, result);
        }

        public static IEnumerator load(string dllPath, string pdbPath){
#if DIRECT_LOAD_DLL //直接加载DLL不走ILRuntime
            LogUtils.I("DIRECT_LOAD_DLL");
            MemoryStream dllstream = null;
            yield return AssetsMgr.LoadDll(dllPath, (stream) => { dllstream = stream; });

            byte[] getDllBytes(MemoryStream dllstream)
            {
                if (dllstream == null)
                    return null;
                dllstream.Seek(0, 0);
                byte[] dllBytes = new byte[dllstream.Length];
                dllstream.Read(dllBytes, 0, (int)dllstream.Length);
                return dllBytes;
            }

            byte[] dllBytes = getDllBytes(dllstream);
            byte[] pdbBytes = null;
            if (pdbPath != "")
            {
                MemoryStream pdbstream = null;
                yield return AssetsMgr.LoadDll(pdbPath, (stream) => { pdbstream = stream; });
                pdbBytes = getDllBytes(pdbstream);
            }

            var appDomain = AppDomain.CurrentDomain;
            if (pdbBytes == null)
                appDomain.Load(dllBytes);
            else
                appDomain.Load(dllBytes, pdbBytes);
#else
            yield return ILRuntimeManager.Instance.LoadFishLogicAssembly(dllPath, pdbPath);
#endif
        }

        private static object DirectInvoke(AppDomain appDomain, string className, string method, params object[] p) {
            Assembly[] assemblies = appDomain.GetAssemblies();
            var type = getAsbType(assemblies, className);
            if (type != null) {
                var isStatic = false;
                var methodInfos = type.GetMethods();
                for (int i = 0; i < methodInfos.Length; i++) {
                    var methodInfo = methodInfos[i];
                    if (methodInfo.IsStatic && methodInfo.Name == method) {
                        isStatic = true;
                        break;
                    }
                }

                if (isStatic)
                    return type.InvokeMember(method, BindingFlags.InvokeMethod, null, null, p);
                else
                    return invokeInstMethod(type, method, p);
            }
            return null;
        }

        private static object invokeInstMethod(Type type, string method, params object[] p) {
            ConstructorInfo ci = type.GetConstructor(new Type[] { });
            if (ci != null) {
                System.Object obj = ci.Invoke(null);
                MethodInfo mi = type.GetMethod(method);
                return mi.Invoke(obj, p);
            }
            return null;
        }

        public static Type getAsbType(Assembly[] assemblies, string className) {
            Type type = null;
            for (int i = 0; i < assemblies.Length; i++) {
                Assembly ass = assemblies[i];
                type = ass.GetType(className, false, false);
                if (type != null) {
                    break;
                }
            }

            return type;
        }
        
        public static object InvokeHostStatic(string strStaticMethod, params object[] paramlist) {
            var strings = strStaticMethod.Split('.');
            var str_lst = strings.ToList();

            var last = str_lst.Count - 1;
            
            var funName = str_lst[last];
            str_lst.RemoveAt(last);

            var className = String.Join(".", str_lst);

            var gameType = Type.GetType(className);
            if (gameType == null) {
                LogUtils.I(className + "不存在");
                return null;
            }

            var method = gameType.GetMethod(funName);
            if (method == null) {
                LogUtils.I(strStaticMethod + " 方法不存在");
                return null;
            }

            var ret = method.Invoke(null, paramlist);
            return ret;
        }
    }
}