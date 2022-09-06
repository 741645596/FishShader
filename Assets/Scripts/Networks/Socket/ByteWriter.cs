
using System;
using System.Text;



public class ByteWriter
{
    // 初始buff大小
    public const ushort Default_Buff_Size = 256;

    // buff
    public Byte[] buff;

    // 当前写入buff位置
    public int position;

    // buff总大小
    public int count;

    // 当前容器内容长度
    public int Length
    {
        get { return position;  }
    }

    public ByteWriter(ushort buffSize)
    {
        position = 0;
        count = buffSize;
        buff = new Byte[buffSize];
    }

    public ByteWriter()
    {
        position = 0;
        count = Default_Buff_Size;
        buff = new Byte[Default_Buff_Size];
    }

    /// <summary>
    /// 写入一个字节
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public ByteWriter WriteByte(Byte b)
    {
        _EnsureEnoughSize(1);

        buff[position] = b;
        position += 1;

        return this;
    }

    /// <summary>
    /// 写入多个字节
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public ByteWriter WriteBytes(Byte[] bytes)
    {
        _EnsureEnoughSize(bytes.Length);

        for (int i = 0; i < bytes.Length; i++)
        {
            buff[position] = bytes[i];
            position++;
        }

        return this;
    }

    /// <summary>
    /// 写入多个字节
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="offset"> bytes的偏移量 </param>
    /// <param name="count"> 写入数量 </param>
    /// <returns></returns>
    public ByteWriter WriteBytes(Byte[] bytes, int offset, int count)
    {
        _EnsureEnoughSize(count);

        for (int i = 0; i < count; i++)
        {
            buff[position] = bytes[offset + i];
            position++;
        }

        return this;
    }

    /// <summary>
    /// 写入32位的整数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ByteWriter WriteInt(int value)
    {
        var bytes = ByteConverter.GetBytes(value);
        return WriteBytes(bytes);
    }

    /// <summary>
    /// 写入无符号32位整数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ByteWriter WriteUInt(uint value)
    {
        var bytes = ByteConverter.GetBytes(value);
        return WriteBytes(bytes);
    }

    /// <summary>
    /// 写入64位整数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ByteWriter WriteLong(long value)
    {
        var bytes = ByteConverter.GetBytes(value);
        return WriteBytes(bytes);
    }

    /// <summary>
    /// 无符号64位整数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ByteWriter WriteULong(ulong value)
    {
        var bytes = ByteConverter.GetBytes(value);
        return WriteBytes(bytes);
    }

    /// <summary>
    /// 32位浮点数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ByteWriter WriteFloat(float value)
    {
        var bytes = BitConverter.GetBytes(value);
        return WriteBytes(bytes);
    }

    /// <summary>
    /// 64位浮点数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ByteWriter WriteDouble(double value)
    {
        var bytes = BitConverter.GetBytes(value);
        return WriteBytes(bytes);
    }

    /// <summary>
    /// 16位整数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ByteWriter WriteShort(short value)
    {
        var bytes = ByteConverter.GetBytes(value);
        return WriteBytes(bytes);
    }

    /// <summary>
    /// 8位
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ByteWriter WriteBool(bool value)
    {
        var bytes = BitConverter.GetBytes(value);
        return WriteBytes(bytes);
    }

    /// <summary>
    /// 宽字符串，如中文字符。提醒：字符串不能包含'\0'否则失败
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public ByteWriter WriteString(string str)
    {
        System.Diagnostics.Debug.Assert(str.Contains("\0") == false, "错误提示：字符串禁止包含'\0'");

        var bytes = Encoding.UTF8.GetBytes(str);
        WriteBytes(bytes);

        // 规范：字符串需要以'\0'结尾
        WriteByte(0);

        return this;
    }

    /// <summary>
    /// ASCII字符串，不允许包含中文字符，可以节省点数据
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public ByteWriter WriteStringA(string str)
    {
        System.Diagnostics.Debug.Assert(str.Contains("\0") == false, "错误提示：字符串禁止包含'\0'");

        var bytes = Encoding.ASCII.GetBytes(str);
        WriteBytes(bytes);

        // 规范：字符串需要以'\0'结尾
        WriteByte(0);

        return this;
    }

    /// <summary>
    /// 清除buff 
    /// </summary>
    public void Clear()
    {
        Array.Clear(buff, 0, position);
        position = 0;
    }

    // 确保buff足够插入
    private void _EnsureEnoughSize(int writeCount)
    {
        if (_IsEnough(writeCount) == false)
        {
            _NewBuff(writeCount);
        }
    }

    // 是否有足够的buff插入
    private bool _IsEnough(int writeCount)
    {
        var res = position + writeCount;
        return res <= count;
    }

    // 将要插入的buff超过现有大小，则扩展新的buff
    private void _NewBuff(int writeCount)
    {
        // 额外增加256个
        var newSize = position + writeCount + 256;
        var newBuff = new Byte[newSize];
        buff.CopyTo(newBuff, 0);
        buff = newBuff;
        count = newSize;
    }
}
