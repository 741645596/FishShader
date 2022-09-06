

using System;
using System.Net.Sockets;
using UnityEngine;


public class StateDisconncetObject
{
    public Socket socket = null;
    public int errorCode = 0;
}
/// <summary>
/// 客户端TCP Socket封装
/// </summary>
public class FishSocket
{
    private class StateObject
    {
        public Socket socket = null;
        public const int BUFFER_SIZE = 256;
        public byte[] buffer = new byte[BUFFER_SIZE];
        public QueueBuffer stream = new QueueBuffer(81920);
    }

    private readonly SocketEventHandler socketEvent;
    private Socket m_Socket = null;
    private byte socketState = SocketStatusDefine.ST_NONE;
    private byte[] _sizeBuff;

    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="socketEvent">ISocketEvent</param>
    public FishSocket(SocketEventHandler socketEvent)
    {
        this.socketEvent = socketEvent;
        _sizeBuff = new byte[2];
    }

    /// <summary>
    /// 连接Socket
    /// </summary>
    /// <param name="url">地址</param>
    /// <param name="port">端口</param>
    public void Connect(string url, int port)
    {
        LogUtils.I("tcpsocket connect to:" + url + ":" + port);
        m_Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        socketState = SocketStatusDefine.ST_CREATED;

        try
        {
            m_Socket.BeginConnect(url, port, new AsyncCallback(OnConnect), m_Socket);
            socketState = SocketStatusDefine.ST_CONNECTING;
        }
        catch (Exception ex)
        {
            LogUtils.W(ex.ToString());
            socketEvent.OnSocketOpen(false);
            socketState = SocketStatusDefine.ST_CREATED;
        }
    }

    /// <summary>
    /// 获取Socket状态
    /// </summary>
    /// <returns></returns>
    public byte GetState()
    {
        return socketState;
    }


    public bool IsConnected()
    {
        return m_Socket != null && m_Socket.Connected && ((int)m_Socket.Handle > 0);
    }

    /// <summary>
    /// 关闭Socket
    /// </summary>
    public void Close(int errcode = (int)SocketError.Shutdown)
    {
        LogUtils.I("Call TCPSocket Close " + errcode);

        // 已经断开连接后 不再触发事件
        if (socketState == SocketStatusDefine.ST_CLOSED)
        {
            return;
        }

        if (IsConnected() == false)
        {
            socketState = SocketStatusDefine.ST_CLOSED;
            return;
        }

        try
        {
            // 关闭Socket前总是应该先调用 Shutdown() 方法。
            // 这能够确保在已连接的Socket关闭前，其上的所有数据都发送和接收完成。
            // 另外 Socket类的Connected属性只表示最后一次I/O操作的状态，如果这之后[连接的另一方]断开了，它还会一直返回true
            m_Socket.Shutdown(SocketShutdown.Both);
        }
        // 如果服务端先主动断开会触发下面异常
        catch (Exception ex)
        {
            Debug.LogWarning("Socket Shutdown Exception:" + ex.ToString());
        }
        // BeginDisconnect() 方法会等待两件事
        // 1. 所有已入列的数据被发送。
        // 2. 另一端确认零字节数据包（如果底层协议适用）
        if (m_Socket != null && m_Socket.Connected)
        {
            StateDisconncetObject obj = new StateDisconncetObject();
            obj.socket = m_Socket;
            obj.errorCode = errcode;
            socketState = SocketStatusDefine.ST_DISCONNECT;
            m_Socket.BeginDisconnect(false, new AsyncCallback(OnDisconnect), obj);
            m_Socket = null;
        }
        else
        {
            socketState = SocketStatusDefine.ST_CLOSED;
            socketEvent.OnSocketClose(errcode);
            m_Socket = null;
        }
    }

    /// <summary>
    /// 主动关闭连接事件
    /// </summary>
    /// <param name="ar"></param>
    private void OnDisconnect(IAsyncResult ar)
    {
        StateDisconncetObject obj = (StateDisconncetObject)ar.AsyncState;
        try
        {
            if (obj.socket != null)
            {
                obj.socket.EndDisconnect(ar);
            }
        }
        catch (Exception e)
        {
            Debug.Log("OnDisconnect Error: ");
            Debug.Log(e);
        }
        socketState = SocketStatusDefine.ST_CLOSED;
        socketEvent.OnSocketClose(obj.errorCode);
    }

    /// <summary>
    /// 发送Message
    /// </summary>
    /// <param name="msg"></param>
    public void SendData(Message msg)
    {
        if (IsConnected() == false)
        {
            return;
        }

        var writer = new ByteWriter();
        msg.GetBytes(writer);
        // 混淆数据
        SocketEncrptyUtils.PackBytes(writer.buff, 0, writer.Length);
        try
        {
            m_Socket.BeginSend(writer.buff, 0, writer.Length, SocketFlags.None, OnSend, null);
        }
        catch (SocketException ex)
        {
            Debug.Log("SocketException socketState:" + socketState);
            Debug.Log("SocketException msg funcID:" + msg.funcID);
            Debug.Log(ex.ToString());
            Close((int)SocketError.SocketError);
        }
    }

    /// <summary>
    /// 发送成功事件
    /// </summary>
    /// <param name="asyncResult"></param>
    private void OnSend(IAsyncResult asyncResult)
    {
        if (m_Socket == null)
        {
            Debug.Log("OnSend Socket 已经中断 ");
            return;
        }
        try
        {
            m_Socket.EndSend(asyncResult);
            //Debug.Log("send " + size + " bytes");
        }
        catch (Exception ex)
        {
            LogUtils.I("Socket OnSend Exception:" + ex.ToString());
            Close((int)SocketError.SocketError);
        }
    }

    /// <summary>
    /// 连接成功事件
    /// </summary>
    /// <param name="asyncResult"></param>
    private void OnConnect(IAsyncResult asyncResult)
    {
        Socket socket = (Socket)asyncResult.AsyncState;
        // 连接成功
        try
        {
            socket.EndConnect(asyncResult);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.ToString());
            socketEvent.OnSocketOpen(false);
            socketState = SocketStatusDefine.ST_CREATED;
            return;
        }
        LogUtils.I("tcp socket connected. ");

        socketState = SocketStatusDefine.ST_CONNECTED;
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 81920);

        // 开始接收消息
        var stateObj = new StateObject();
        stateObj.socket = socket;
        StartRecvive(stateObj);

        // 触发OnSocketOpen事件
        socketEvent.OnSocketOpen(true);
    }

    private void StartRecvive(StateObject stateObject)
    {
        try
        {
            m_Socket.BeginReceive(stateObject.buffer,
                0,
                StateObject.BUFFER_SIZE,
                SocketFlags.None,
                OnMessage,
                stateObject);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.ToString());
            Close((int)SocketError.SocketError);
        }
    }

    /// <summary>
    /// 消息数据流的拆包处理
    /// </summary>
    /// <param name="asyncResult"></param>
    private void OnMessage(IAsyncResult asyncResult)
    {
        var stateObj = asyncResult.AsyncState as StateObject;
        Socket socket = stateObj.socket;
        try
        {
            int readSize = socket.EndReceive(asyncResult);
            if (readSize > 0)
            {
                // 数据写入steam
                stateObj.stream.Enqueue(stateObj.buffer, readSize);
                while (stateObj.stream.Length >= 2)
                {
                    // 读取Message长度
                    stateObj.stream.Copy(_sizeBuff, 0, 2);

                    Array.Reverse(_sizeBuff);
                    SocketEncrptyUtils.UnPackBytes(_sizeBuff);
                    ushort msgSize = BitConverter.ToUInt16(_sizeBuff, 0);
                    if (msgSize > 0 && stateObj.stream.Length >= msgSize)
                    {
                        // 热更工程解析直接用buffer解析，无法保证热更工程和主工程同时更新，无法优化
                        var buffer = new byte[msgSize];
                        stateObj.stream.Dequeue(buffer, msgSize);

                        SocketEncrptyUtils.UnPackBytes(buffer);
                        socketEvent.OnMessage(buffer);
                        continue;
                    }

                    break;
                }

                // 继续接收数据
                StartRecvive(stateObj);
            }
            else
            {
                LogUtils.I("EOF");
                // 服务端主动断开连接
                Close(-2);
            }
        }
        catch (Exception e)
        {
            LogUtils.W($"tcp OnMessage触发异常：{e.ToString()}");
            Close((int)SocketError.SocketError);
        }
    }
}
