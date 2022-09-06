public static class SocketStatusDefine
{
    #region 消息类型
    public const byte MT_NORMAL = 0;    // 普通消息 不需要响应的请求消息
    public const byte MT_REQUEST = 1;   // 请求消息	
    public const byte MT_RESPONSE = 2;  // 响应消息
    #endregion

    #region 消息标志位
    public const byte MF_NONE = 0;      // 无
    public const byte MF_ENCODE = 1;    // 混淆
    public const byte MF_COMPRESS = 2;  // 压缩
    public const byte MF_ROUTE = 4;     // 转发路由 客户端忽略
    public const byte MF_TRACE = 8;     // 跟踪链 客户端忽略
    public const byte MF_PACKAGE = 16;  // 分包标志 大于16k的情况需要分包
    #endregion

    #region Socket状态
    public const byte ST_NONE = 0;
    public const byte ST_CREATED = 1;   // 已经创建
    public const byte ST_CONNECTING = 2;// 正在连接
    public const byte ST_CONNECTED = 3; // 已经连接
    public const byte ST_DISCONNECT = 4;// 正在断开连接
    public const byte ST_CLOSED = 5;    // 连接已关闭
    #endregion
}
