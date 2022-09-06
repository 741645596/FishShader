using System;
using System.Collections.Generic;
using UnityEngine;

public class SocketEventHandler
{
    protected FishSocket socket;
    public Queue<Message> m_msgQueue = new Queue<Message>();
    protected readonly Queue<Message> msgCacheQueue = new Queue<Message>();
    protected bool isProcessing = true;
    private readonly object thisLock = new object();

    Action<int> m_OnConnectHandler;
    Action<int> m_OnCloseHandler;
    Action<string> m_OnErrorHandler;
    Action<Message> m_OnMessageHandler;

    public int m_ReciveMessageCount;
    public int m_SendMessageCount;

    protected Dictionary<ushort, Action<Message>> pfnMsgProcess;

    private readonly uint SOCKET_OPEN = 1;
    private readonly uint SOCKET_CLOSE = 2;

    Message m_Message;

    public SocketEventHandler()
    {
        m_Message = new Message();
        pfnMsgProcess = new Dictionary<ushort, Action<Message>>();
    }

    public void UnregisterAllMsgProcess()
    {
        pfnMsgProcess.Clear();
    }

    public void RegisterMsgProcess(ushort wMsgID, Action<Message> callback)
    {
        pfnMsgProcess[wMsgID] = callback;
    }

    public Action<Message> GetRegisterMsgProcess(ushort wMsgID)
    {
        if (pfnMsgProcess.ContainsKey(wMsgID))
        {
            return pfnMsgProcess[wMsgID];
        }

        return null;
    }


    public bool OnProcessMessage(Message msg)
    {
        m_ReciveMessageCount++;
        m_OnMessageHandler?.Invoke(msg);
        return true;
    }

    public void SetOnConnect(Action<int> handler)
    {
        m_OnConnectHandler = handler;
    }

    public void SetOnClose(Action<int> handler)
    {
        m_OnCloseHandler = handler;
    }

    public void SetOnError(Action<string> handler)
    {
        m_OnErrorHandler = handler;
    }

    public void SetOnMessage(Action<Message> handler)
    {
        m_OnMessageHandler = handler;
    }

    public void PauseProcess()
    {
        isProcessing = false;
    }

    public void ResumeProcess()
    {
        isProcessing = true;
    }

    public void OnMessage(byte[] data)
    {
        lock (thisLock)
        {
            var msg = CommonPool<Message>.Get();
            msg.ReadMessage(data);
            m_msgQueue.Enqueue(msg);
        }
    }


    public void ClearMessageQueue()
    {
        lock (thisLock)
        {
            m_msgQueue.Clear();
        }
    }

    public void Update()
    {
        lock (thisLock)
        {
            int count = 0;
            while (m_msgQueue.Count > 0 && isProcessing)
            {
                Message msg = m_msgQueue.Dequeue();
                if (msg != null)
                {
                    // 回收
                    CommonPool<Message>.Reclaim(msg);

                    // 定义funcID == 0是特殊ID，用于处理Socket异步连接/关闭事件
                    if (msg.funcID == 0 && msg.reqID == SOCKET_OPEN)
                    {
                        var success = msg.body[0] == 1 ? true : false;
                        OnConnect(success);
                        continue;
                    }

                    if (msg.funcID == 0 && msg.reqID == SOCKET_CLOSE)
                    {
                        var code = BitConverter.ToInt32(msg.body, 0);
                        OnClose(code);
                        continue;
                    }

                    OnProcessMessage(msg);
                    count++;
                    if (count > 30)
                    {
                        break;
                    }
                }
            }
        }
    }

    public virtual void OnConnect(bool success)
    {
        m_OnConnectHandler?.Invoke(success ? 0 : -1);
    }

    public virtual void OnClose(int errorCode)
    {
        m_OnCloseHandler?.Invoke(errorCode);
    }

    public virtual void OnError(string reason)
    {
        m_OnErrorHandler?.Invoke(reason);
    }

    public void Reconnect(string url)
    {
        //socket = new ClientSocket(this);
        //socket.Connect(url);
        if (socket != null)
        {
            LogUtils.I("Reconnect Close");
            socket.Close();
            socket = null;
        }

        socket = new FishSocket(this);
        string[] token = url.Split(':');
        if(token.Length < 2)
        {
            LogUtils.I($"url解析错误 {url}");
            return;
        }
        socket.Connect(token[0], int.Parse(token[1]));
    }

    public void StartConnect(string url)
    {
        m_ReciveMessageCount = 0;
        m_SendMessageCount = 0;
        Reconnect(url);
    }

    public void SendData(ushort msgId, byte[] data)
    {
        if (IsConnected())
        {
            m_Message.Reset();
            m_Message.Init(msgId, data);
            m_SendMessageCount++;
            socket.SendData(m_Message);
        }
        else
        {
            LogUtils.W("发送失败，socket没有连接");
            if(socket == null)
            {
                LogUtils.W("socket is null.");
            }
            else
            {
                LogUtils.W(socket.GetState()) ;
            }
        }
    }

    public bool IsConnected()
    {
        if (socket == null)
        {
            return false;
        }
        return socket.IsConnected();
    }

    public void CloseSocket()
    {
        if (socket == null)
        {
            LogUtils.I("socket is null");
            return;
        }

        LogUtils.I("CloseSocket");
        socket.Close();
        socket = null;
    }

    public void CloseSocketAsync()
    {
        if (socket == null)
        {
            LogUtils.I("socket is null");
            return;
        }

        LogUtils.I("CloseSocketAsync");
        socket.Close();
        socket = null;
    }

    //add
    public byte GetState()
    {
        if (socket == null)
        {
            return SocketStatusDefine.ST_NONE;
        }
        return socket.GetState();
    }

    public void OnSocketOpen(bool success)
    {
        byte v = success ? (byte)1 : (byte)0;
        var msg = new Message(0, new byte[] { v });
        msg.reqID = SOCKET_OPEN;
        lock (thisLock)
        {
            m_msgQueue.Enqueue(msg);
        }
    }

    public void OnSocketClose(int errorCode)
    {
        var data = BitConverter.GetBytes(errorCode);
        var msg = new Message(0, data);
        msg.reqID = SOCKET_CLOSE;
        lock (thisLock)
        {
            m_msgQueue.Enqueue(msg);
        }
    }
}