
using System.Diagnostics;


/// <summary>
/// 节省内存，但是线程不安全
/// </summary>
public class ByteConverter
{
    // 如果是多线程用到GetBytes需要自行加锁
    public static object bitConverterLock = new object();

    private static byte[] bytes1 = new byte[1];
    private static byte[] bytes2 = new byte[2];
    private static byte[] bytes4 = new byte[4];
    private static byte[] bytes8 = new byte[8];

    /// <summary>
    /// 线程不安全
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] GetBytes(long value)
    {
        return GetBytes(bytes8, value);
    }

    /// <summary>
    /// 自己指定接收bytes
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] GetBytes(byte[] bytes, long value)
    {
        Debug.Assert(bytes.Length >= 8, $"错误提示：bytes消息长度必须>=8");

        bytes[0] = (byte)(value & 0xFF);
        bytes[1] = (byte)((value >> 8) & 0xFF);
        bytes[2] = (byte)((value >> 16) & 0xFF);
        bytes[3] = (byte)((value >> 24) & 0xFF);
        bytes[4] = (byte)((value >> 32) & 0xFF);
        bytes[5] = (byte)((value >> 40) & 0xFF);
        bytes[6] = (byte)((value >> 48) & 0xFF);
        bytes[7] = (byte)((value >> 56) & 0xFF);
        return bytes;
    }

    public static byte[] GetBytes(ulong value)
    {
        return GetBytes(bytes8, value);
    }

    public static byte[] GetBytes(byte[] bytes, ulong value)
    {
        Debug.Assert(bytes.Length >= 8, $"错误提示：bytes消息长度必须>=8");

        bytes[0] = (byte)(value & 0xFF);
        bytes[1] = (byte)((value >> 8) & 0xFF);
        bytes[2] = (byte)((value >> 16) & 0xFF);
        bytes[3] = (byte)((value >> 24) & 0xFF);
        bytes[4] = (byte)((value >> 32) & 0xFF);
        bytes[5] = (byte)((value >> 40) & 0xFF);
        bytes[6] = (byte)((value >> 48) & 0xFF);
        bytes[7] = (byte)((value >> 56) & 0xFF);
        return bytes;
    }

    public static byte[] GetBytes(int value)
    {
        return GetBytes(bytes4, value);
    }

    public static byte[] GetBytes(byte[] bytes, int value)
    {
        Debug.Assert(bytes.Length >= 4, $"错误提示：bytes消息长度必须>=4");

        bytes[0] = (byte)(value & 0xFF);
        bytes[1] = (byte)((value >> 8) & 0xFF);
        bytes[2] = (byte)((value >> 16) & 0xFF);
        bytes[3] = (byte)((value >> 24) & 0xFF);
        return bytes;
    }

    public static byte[] GetBytes(uint value)
    {
        return GetBytes(bytes4, value);
    }

    public static byte[] GetBytes(byte[] bytes, uint value)
    {
        Debug.Assert(bytes.Length >= 4, $"错误提示：bytes消息长度必须>=4");

        bytes[0] = (byte)(value & 0xFF);
        bytes[1] = (byte)((value >> 8) & 0xFF);
        bytes[2] = (byte)((value >> 16) & 0xFF);
        bytes[3] = (byte)((value >> 24) & 0xFF);
        return bytes;
    }

    public static byte[] GetBytes(short value)
    {
        return GetBytes(bytes2, value);
    }

    public static byte[] GetBytes(byte[] bytes, short value)
    {
        Debug.Assert(bytes.Length >= 2, $"错误提示：bytes消息长度必须>=2");

        bytes[0] = (byte)(value & 0xFF);
        bytes[1] = (byte)((value >> 8) & 0xFF);
        return bytes;
    }

    public static byte[] GetBytes(ushort value)
    {
        return GetBytes(bytes2, value);
    }

    public static byte[] GetBytes(byte[] bytes, ushort value)
    {
        Debug.Assert(bytes.Length >= 2, $"错误提示：bytes消息长度必须>=2");

        bytes[0] = (byte)(value & 0xFF);
        bytes[1] = (byte)((value >> 8) & 0xFF);
        return bytes;
    }

    public static byte[] GetBytes(byte value)
    {
        return GetBytes(bytes1, value);
    }

    public static byte[] GetBytes(byte[] bytes, byte value)
    {
        Debug.Assert(bytes.Length >= 1, $"错误提示：bytes消息长度必须>=1");

        bytes[0] = value;
        return bytes;
    }
}
