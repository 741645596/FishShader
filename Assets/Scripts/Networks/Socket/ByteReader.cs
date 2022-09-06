
using System;
using System.Text;


public class ByteReader
{
    private Byte[] _buff;
    private int _buffLen;
    private int _position;

    public int Length
    {
        get { return _buffLen; }
    }

    public ByteReader(Byte[] buff)
    {
        _buff = buff;
        _buffLen = buff.Length;
        _position = 0;
    }

    public ByteReader()
    {
        _buff = new Byte[4];
        _buffLen = _buff.Length;
        _position = 0;
    }

    /// <summary>
    /// 重置buff
    /// </summary>
    /// <param name="buff"></param>
    /// <param name="offset"> buff的偏移量 </param>
    /// <param name="count"> buff的数量 </param>
    public void Reset(Byte[] buff, int offset, int count)
    {
        if (count > _buff.Length)
        {
            _buff = new Byte[count];
        }

        Array.Copy(buff, offset, _buff, 0, count);
        _buffLen = buff.Length;
        _position = 0;
    }

    /// <summary>
    /// 读取一个字节
    /// </summary>
    /// <returns></returns>
    public Byte ReadByte()
    {
        if (_position >= _buffLen)
        {
            System.Diagnostics.Debug.Assert(false, "错误提示：ReadByte已经读到末尾，请检查数据是否正确");
            return 0;
        }

        var res = _buff[_position];
        _position += 1;
        return res;
    }

    public int ReadInt()
    {
        var res = BitConverter.ToInt32(_buff, _position);
        _position += 4;
        return res;
    }

    /// <summary>
    /// 是否大小端转换
    /// </summary>
    /// <param name="isReverse"></param>
    /// <returns></returns>
    public int ReadInt(bool isReverse)
    {
        if (isReverse)
        {
            Array.Reverse(_buff, _position, 4);
        }
        return ReadInt();
    }

    public uint ReadUInt()
    {
        var res = BitConverter.ToUInt32(_buff, _position);
        _position += 4;
        return res;
    }

    /// <summary>
    /// 是否大小端转换
    /// </summary>
    /// <param name="isReverse"></param>
    /// <returns></returns>
    public uint ReadUInt(bool isReverse)
    {
        if (isReverse)
        {
            Array.Reverse(_buff, _position, 4);
        }
        return ReadUInt();
    }

    public long ReadLong()
    {
        var res = BitConverter.ToInt64(_buff, _position);
        _position += 8;
        return res;
    }

    /// <summary>
    /// 是否大小端转换
    /// </summary>
    /// <param name="isReverse"></param>
    /// <returns></returns>
    public long ReadLong(bool isReverse)
    {
        if (isReverse)
        {
            Array.Reverse(_buff, _position, 8);
        }
        return ReadLong();
    }

    public ulong ReadULong()
    {
        var res = BitConverter.ToUInt64(_buff, _position);
        _position += 8;
        return res;
    }

    /// <summary>
    /// 是否大小端转换
    /// </summary>
    /// <param name="isReverse"></param>
    /// <returns></returns>
    public ulong ReadULong(bool isReverse)
    {
        if (isReverse)
        {
            Array.Reverse(_buff, _position, 8);
        }
        return ReadULong();
    }

    public float ReadFloat()
    {
        var res = BitConverter.ToSingle(_buff, _position);
        _position += 4;
        return res;
    }

    /// <summary>
    /// 是否大小端转换
    /// </summary>
    /// <param name="isReverse"></param>
    /// <returns></returns>
    public float ReadFloat(bool isReverse)
    {
        if (isReverse)
        {
            Array.Reverse(_buff, _position, 4);
        }
        return ReadFloat();
    }

    public double ReadDouble()
    {
        var res = BitConverter.ToDouble(_buff, _position);
        _position += 8;
        return res;
    }

    /// <summary>
    /// 是否大小端转换
    /// </summary>
    /// <param name="isReverse"></param>
    /// <returns></returns>
    public double ReadDouble(bool isReverse)
    {
        if (isReverse)
        {
            Array.Reverse(_buff, _position, 8);
        }
        return ReadDouble();
    }

    public short ReadShort()
    {
        var res = BitConverter.ToInt16(_buff, _position);
        _position += 2;
        return res;
    }

    /// <summary>
    /// 是否大小端转换
    /// </summary>
    /// <param name="isReverse"></param>
    /// <returns></returns>
    public short ReadShort(bool isReverse)
    {
        if (isReverse)
        {
            Array.Reverse(_buff, _position, 2);
        }
        return ReadShort();
    }

    public ushort ReadUShort()
    {
        var res = BitConverter.ToUInt16(_buff, _position);
        _position += 2;
        return res;
    }

    public ushort ReadUShort(bool isReverse)
    {
        if (isReverse)
        {
            Array.Reverse(_buff, _position, 2);
        }
        return ReadUShort();
    }

    public bool ReadBool()
    {
        var res = BitConverter.ToBoolean(_buff, _position);
        _position += 1;
        return res;
    }

    public string ReadString()
    {
        var index = _GetStringEndIndex();
        if (index == -1)
        {
            System.Diagnostics.Debug.Assert(false, "错误提示：ReadString未找到字符串结尾'\0'字符");
            return "";
        }

        var count = index - _position;
        var str = Encoding.UTF8.GetString(_buff, _position, count);
        _position = index + 1;
        return str;
    }

    public string ReadStringA()
    {
        var index = _GetStringEndIndex();
        if (index == -1)
        {
            System.Diagnostics.Debug.Assert(false, "错误提示：ReadStringA未找到字符串结尾'\0'字符");
            return "";
        }

        var count = index - _position;
        var str = Encoding.ASCII.GetString(_buff, _position, count);
        _position = index + 1;
        return str;
    }

    /// <summary>
    /// 该方法会有new操作，如果考虑性能情况用其他方式读取
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public Byte[] ReadBytes(int count)
    {
        if (_position + count > _buff.Length)
        {
            LogUtils.W("错误提示：ReadBytes读取字符超过buff长度，请检查写入字符是否正确");
            return BitConverter.GetBytes(0);
        }

        var newBytes = new Byte[count];
        Array.Copy(_buff, _position, newBytes, 0, count);
        _position += count;
        return newBytes;
    }

    private int _GetStringEndIndex()
    {
        for (int i = _position; i < _buffLen; i++)
        {
            if (_buff[i] == 0)
            {
                return i;
            }
        }
        return -1;
    }
}

