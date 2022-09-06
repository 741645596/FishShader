
using System;
using System.Collections.Generic;



/// <summary>
/// 对象池工具类
/// 备注：ILRuntime会裁剪代码，如果是热更工程在发布版本后，新增了对象池那代码会被裁剪掉，导致崩溃
/// </summary>
/// <typeparam name="T"></typeparam>
public static class CommonPool<T> where T : new()
{
    private static Stack<T> _pool = new Stack<T>(30);

    /// <summary>
    /// 获得object，没有就new一个
    /// </summary>
    /// <returns></returns>
    public static T Get()
    {
        if (_pool.Count == 0)
        {
            return new T();
        }
        return _pool.Pop();
    }

    /// <summary>
    /// 回收object
    /// </summary>
    /// <param name="t"></param>
    public static void Reclaim(T t)
    {
        _pool.Push(t);
    }

    /// <summary>
    /// 清除缓存
    /// </summary>
    public static void Clear()
    {
        _pool.Clear();
    }

    /// <summary>
    /// 清除缓存前先遍历下数据，可以重置啥的
    /// </summary>
    /// <param name="cb"></param>
    public static void Clear(Action<T> cb)
    {
        Foreach(cb);

        Clear();
    }

    /// <summary>
    /// 遍历缓存数据
    /// </summary>
    /// <param name="cb"></param>
    public static void Foreach(Action<T> cb)
    {
        foreach (var obj in _pool)
        {
            cb(obj);
        }
    }

    public static int Count()
    {
        return _pool.Count;
    }
}