
using System;
using System.IO;


public class Message
{
    /// <summary>
    /// 消息头固定大小10
    /// len(2) + type(1) + flag(1) + reqID(4) + funcID(2) = 10
    /// </summary>
    private readonly ushort HEADER_SIZE = 10;
    /// <summary>
    /// 消息长度 长度2
    /// </summary>
    public ushort len;

    /// <summary>
    /// 消息类型 长度1
    /// <para>NetworkDefine.MT_NORMAL 普通消息 不需要响应的请求消息</para>
    /// <para>NetworkDefine.MT_REQUEST 请求消息</para>
    /// <para>NetworkDefine.MT_RESPONSE 响应消息</para>
    /// </summary>
    public byte type;

    /// <summary>
    /// 消息标志位 长度1
    ///<para>NetworkDefine.MF_ENCODE</para>
    ///<para>NetworkDefine.MF_COMPRESS</para>
    ///<para>NetworkDefine.MF_ROUTE</para>
    ///<para>NetworkDefine.MF_TRACE</para>
    ///<para>NetworkDefine.MF_PACKAGE</para>
    /// </summary>
    public byte flag;

    /// <summary>
    /// 请求ID（响应时回传）长度4
    /// </summary>
    public uint reqID;

    /// <summary>
    /// 功能ID 长度2
    /// </summary>
    public ushort funcID;

    /// <summary>
    /// 数据
    /// </summary>
    public byte[] body;

    private ByteReader _reader;

    /// <summary>
    /// Decode Message
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    //public static Message Decode(byte[] data)
    //{
    //    return new Message(data);
    //}

    /// <summary>
    /// Encode Message
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    //public static byte[] Encode(Message message)
    //{
    //    return message.GetBytes();
    //}

    /// <summary>
    /// 消息结构基类
    /// <see cref="https://doc.weiletest.com/share/ef90573f512d266529e0"/>
    /// </summary>
    public Message()
    {
        // 消息类型统一定义为 MT_NORMAL
        type = SocketStatusDefine.MT_NORMAL;
        // 标志位
        flag = SocketStatusDefine.MF_NONE;
        // type为MT_NORMAL时忽略reqID
        reqID = 0;
    }

    public Message(ushort msgID, byte[] body) : this()
    {
        funcID = msgID;
        SetData(body);
    }

    public void SetData(byte[] body)
    {
        this.body = body;

        len = HEADER_SIZE;
        if (body != null)
        {
            len += (ushort)body.Length;
        }
    }

    public void ReadMessage(byte[] rawData)
    {
        if(rawData.Length < HEADER_SIZE)
        {
            LogUtils.W("Mesage.ctor rawData length error.");
            return;
        }

        if (_reader == null)
            _reader = new ByteReader(rawData);
        else
            _reader.Reset(rawData, 0, rawData.Length);

        bool isReverse = BitConverter.IsLittleEndian;
        len = _reader.ReadUShort(isReverse);
        type = _reader.ReadByte();
        // 验证消息类型，只支持MT_NORMAL和MT_REQUEST
        if (type != SocketStatusDefine.MT_NORMAL
            && type != SocketStatusDefine.MT_REQUEST)
        {
            LogUtils.W("Message.Decode type error.");
            return;
        }

        flag = _reader.ReadByte();
        reqID = _reader.ReadUInt(isReverse);
        funcID = _reader.ReadUShort(isReverse);
        body = _reader.ReadBytes(rawData.Length - HEADER_SIZE);

        //using (var m = new MemoryStream(rawData))
        //{
        //    using (var reader = new BinaryReader(m))
        //    {
        //        var len_bytes = reader.ReadBytes(2);

        //        type = reader.ReadByte();

        //        // 验证消息类型，只支持MT_NORMAL和MT_REQUEST
        //        if (type != NetworkDefine.MT_NORMAL
        //            && type != NetworkDefine.MT_REQUEST)
        //        {
        //            WLDebug.LogWarning("Message.Decode type error.");
        //            return;
        //        }

        //        flag = reader.ReadByte();

        //        var reqID_bytes = reader.ReadBytes(4);
        //        var funcID_bytes = reader.ReadBytes(2);

        //        // 大端序转为小端序
        //        if (BitConverter.IsLittleEndian)
        //        {
        //            Array.Reverse(len_bytes);
        //            Array.Reverse(funcID_bytes);
        //            Array.Reverse(reqID_bytes);
        //        }

        //        len = BitConverter.ToUInt16(len_bytes, 0);
        //        funcID = BitConverter.ToUInt16(funcID_bytes, 0);
        //        reqID = BitConverter.ToUInt32(reqID_bytes, 0);

        //        body = reader.ReadBytes(rawData.Length - HEADER_SIZE);
        //    }
        //}
    }

    public void GetBytes(ByteWriter writer)
    {
        int size = HEADER_SIZE;
        if (body != null)
        {
            size += body.Length;
        }
        len = (ushort)size;

        bool isLittleEndian = BitConverter.IsLittleEndian;
        byte[] len_bytes = BitConverter.GetBytes(len);
        if (isLittleEndian) { Array.Reverse(len_bytes); }
        writer.WriteBytes(len_bytes);

        writer.WriteByte(type);
        writer.WriteByte(flag);

        byte[] reqID_bytes = BitConverter.GetBytes(reqID);
        if (isLittleEndian) { Array.Reverse(reqID_bytes);  }
        writer.WriteBytes(reqID_bytes);

        byte[] funcID_bytes = BitConverter.GetBytes(funcID);
        if (isLittleEndian) { Array.Reverse(funcID_bytes); }
        writer.WriteBytes(funcID_bytes);

        if (body != null)
        {
            writer.WriteBytes(body);
        }
    }

    public void Init(ushort msgID, byte[] data)
    {
        funcID = msgID;
        body = data;

        len = HEADER_SIZE;
        if (body != null)
        {
            len += (ushort)body.Length;
        }
    }
    public void Reset()
    {
        funcID = 0;
        reqID = 0;
        flag = 0;
        type = 0;
        body = null;
        len = 0;
    }
}
