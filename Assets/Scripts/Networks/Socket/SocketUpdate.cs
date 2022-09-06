using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketUpdate : MonoBehaviour
{
#if UNITY_EDITOR
    public bool ConnectStatus;
    public int ReciveMessageCount;
    public int SendMessageCount;
    public bool TestCloseSocket;
#endif
    SocketEventHandler m_Socket;

    float m_Interval = -1;
    float m_UpdateTimer = 0;
    Action m_UpdateHandler = null;
    float m_Time;
    public float m_UpdateTimeout = -1;
    Action m_UpdateTimeoutHandler = null;

    private void Start()
    {
        m_Time = Time.realtimeSinceStartup;
    }

    public void SetUpdateSocket(SocketEventHandler socket)
    {
        m_Socket = socket;
        m_Time = Time.realtimeSinceStartup;
    }

    public void SetUpdateTimeout(float timeout, Action handler)
    {
        m_UpdateTimeoutHandler = handler;
        m_UpdateTimeout = timeout;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Time.deltaTime == 0) return;
#endif
        if (m_Socket != null)
        {
            if (m_UpdateTimeout > 0)
            {
                float newTime = Time.realtimeSinceStartup;
                float offset = newTime - m_Time;
                m_Time = newTime;
                //LogUtils.V(offset);
                if (offset > m_UpdateTimeout)
                {
                    //LogUtils.V($"Socket Update Timeout {offset}");
                    m_UpdateTimeoutHandler?.Invoke();
                    return;
                }
            }
            float dt = Time.deltaTime;
            try
            {
                m_Socket.Update();
            }
            catch (Exception e)
            {
                Debug.LogError("SocketUpdate Error: ");
                throw e;
            }
            if (m_Interval > 0)
            {
                m_UpdateTimer += dt;
                if (m_UpdateTimer > m_Interval)
                {
                    m_UpdateTimer -= m_Interval;
                    m_UpdateHandler?.Invoke();
                }
            }
        }
#if UNITY_EDITOR
        if (m_Socket != null)
        {
            ReciveMessageCount = m_Socket.m_ReciveMessageCount;
            SendMessageCount = m_Socket.m_SendMessageCount;
            ConnectStatus = m_Socket.IsConnected();
            if (TestCloseSocket)
            {
                m_Socket.CloseSocket();
                TestCloseSocket = false;
            }
        }
        else
        {
            ConnectStatus = false;
        }
#endif
    }

    public void SetUpdateHandler(float interval, Action cb)
    {
        m_Interval = interval;
        m_UpdateHandler = cb;
    }
}
