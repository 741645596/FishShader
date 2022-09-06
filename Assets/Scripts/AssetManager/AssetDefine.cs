using System;

public class ABMD5Base
{
    public int Group = 0;
    public string Name = "";
    public string Md5 = "";
    public long Size = 0;
}

public class ABMD5List
{
    public int version = 0;
    public ABMD5Base[] assets = null;
    public int time = 0;
    public int small = 0;
}