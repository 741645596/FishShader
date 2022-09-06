#if !DISABLE_ILRUNTIME_FOR_FILE
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MonoBehaviourAdapter : CrossBindingAdaptor
{
    public override System.Type BaseCLRType
    {
        get{ return typeof(MonoBehaviour); }
    }

    public override System.Type AdaptorType
    {
        get { return typeof(Adaptor); }
    }

    public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
    {
        return new Adaptor(appdomain, instance);
    }

    public class Adaptor : MonoBehaviour, CrossBindingAdaptorType
    {
        public string RemoteClass;
        private ILRuntime.Runtime.Enviorment.AppDomain m_Appdomain;
        private ILTypeInstance m_Instance;
        private IMethod m_OnEnableMethod;
        private IMethod m_OnDisableMethod;

        public Adaptor() { }

        public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            m_Appdomain = appdomain;
            m_Instance = instance;           
        }

        public ILTypeInstance ILInstance
        {
            get{  return m_Instance; }
            set
            {
                m_Instance = value;
                m_OnEnableMethod = null;
                m_OnDisableMethod = null;
            }
        }

        public ILRuntime.Runtime.Enviorment.AppDomain AppDomain
        {
            get { return m_Appdomain; }
            set { m_Appdomain = value; }
        }

        public void Awake()
        {
            if (m_Instance != null)
            {
                var awakeMethod = m_Instance.Type.GetMethod("Awake", 0);
                if (awakeMethod != null)
                {
                    m_Appdomain.Invoke(awakeMethod, m_Instance, null);
                }
            }
        }

        public void OnEnable()
        {
            if (m_Instance != null)
            {
                if (m_OnEnableMethod == null)
                {
                    m_OnEnableMethod = m_Instance.Type.GetMethod("OnEnable", 0);
                }

                if (m_OnEnableMethod != null)
                {
                    m_Appdomain.Invoke(m_OnEnableMethod, m_Instance, null);
                }
            }
        }

        public void Start()
        {
            RemoteClass = m_Instance.Type.FullName;
            var startMethod = m_Instance.Type.GetMethod("Start", 0);
            if (startMethod != null)
            {
                m_Appdomain.Invoke(startMethod, m_Instance, null);
            }
        }

        public void OnDisable()
        {
            if (m_OnDisableMethod == null)
            {
                m_OnDisableMethod = m_Instance.Type.GetMethod("OnDisable", 0);
            }

            if (m_OnDisableMethod != null)
            {
                m_Appdomain.Invoke(m_OnDisableMethod, m_Instance, null);
            }
        }

        public void OnDestroy()
        {
            var destroyMethod = m_Instance.Type.GetMethod("OnDestroy", 0);
            if (destroyMethod != null)
            {
                m_Appdomain.Invoke(destroyMethod, m_Instance, null);
            }
            m_OnDisableMethod = null;
            m_OnEnableMethod = null;
        }
    }
}
#endif