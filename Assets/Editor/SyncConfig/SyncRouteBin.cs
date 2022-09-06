using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

// 同步路径二进制文件
class SyncRouteBin
{
    static string RoutePath = "E:/TimelineEditor/Assets/GameAssets/RouteConfig/route.json";

    [MenuItem("Tools/同步配置/同步路径二进制文件")]
    public static void Sync()
    {
        string timelinePath = Path.GetFullPath(Application.dataPath + "/TimelineEditor/Config~/route.json");
        if (!File.Exists(timelinePath))
        {
            Debug.LogError($"不存在鱼线工程的路径配置 {timelinePath}");
            return;
        }
        RoutePath = timelinePath;
        
        var inst = new ConfigRoute();
        byte[] bytes = inst.ToBinary();
        Debug.Log(bytes.Length);
        string path = Path.GetFullPath(Application.dataPath + "/GameData/AppRes/DataBin/fish082310.bytes");
        File.WriteAllBytes(path, bytes);
        Debug.Log($"导出路径数据成功 {path}");
    }

    public class ConfigRoute
    {
        private Dictionary<int, CfgFishRouteDouble> m_DataDic;

        

        public ConfigRoute()
        {
            m_DataDic = new Dictionary<int, CfgFishRouteDouble>();
            string content = File.ReadAllText(RoutePath);
            List<CfgFishRouteDouble> list = LitJson.JsonMapper.ToObject<List<CfgFishRouteDouble>>(content);
            for (int i = 0; i < list.Count; i++)
            {
                m_DataDic[list[i].id] = list[i];
            }
        }

        public List<int> GetAllKeys()
        {
            var keys = new List<int>();
            foreach (var key in m_DataDic)
            {
                keys.Add(key.Key);
            }
            keys.Sort(delegate (int a, int b) { return a > b ? 1 : -1; });
            return keys;
        }

        public CfgFishRoute GetRoute(int id)
        {
            if (m_DataDic.ContainsKey(id))
            {
                var info = m_DataDic[id];
                CfgFishRoute ret = new CfgFishRoute();
                ret.id = info.id;
                ret.StartPos = new Vector2((float)info.x, (float)info.y);
                ret.RouteType = info.RouteType;
                ret.TotalTime = (float)info.TotalTime;
                ret.FadeAway = info.FadeAway;
                ret.SwordAction = info.SwordAction;
                ret.FishRotate = (float)info.FishRotate;
                ret.SwordFishTime = (float)info.SwordFishTime;
                ret.FishAngle = (float)info.FishAngle;
                ret.Slope = (float)info.Slope;
                ret.Description = info.Description;

                ret.PathInfos = new List<CfgRouteNodeInfo>();
                for (int i = 0; i < info.PathInfos.Count; i++)
                {
                    var pathInfo = new CfgRouteNodeInfo();
                    ret.PathInfos.Add(pathInfo);
                    var pathInfoDouble = info.PathInfos[i];
                    pathInfo.Position = new Vector2((float)pathInfoDouble.x, (float)pathInfoDouble.y);
                    pathInfo.MoveTime = (float)pathInfoDouble.MoveTime;
                    pathInfo.Type = pathInfoDouble.Type;
                    pathInfo.Rate = (float)pathInfoDouble.Rate;
                    pathInfo.Speed = (float)pathInfoDouble.Speed;
                    pathInfo.RotLerp = (float)pathInfoDouble.RotLerp;
                    pathInfo.playAni = pathInfoDouble.playAni;
                }
                return ret;
            }
            return null;
        }

        public byte[] ToBinary()
        {
            /*
            public int id;
            public Vector2 StartPos; // 起始位置
            public int RouteType; //
            public float TotalTime;
            public List<CfgRouteNodeInfo> PathInfos;
            public int FadeAway;
            public int SwordAction;
            public float FishRotate;
            public float SwordFishTime;
            public float FishAngle;
            public float Slope;
             * */
            var stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            var keys = GetAllKeys();
            writer.Write(keys.Count);
            for (int k = 0; k < keys.Count; k++)
            {
                CfgFishRoute info = GetRoute(keys[k]);
                writer.Write(info.id);
                writer.Write(info.StartPos.x);
                writer.Write(info.StartPos.y);
                writer.Write(info.RouteType);
                writer.Write(info.TotalTime);

                writer.Write(info.FadeAway);
                writer.Write(info.SwordAction);
                writer.Write(info.FishRotate);
                writer.Write(info.SwordFishTime);
                writer.Write(info.FishAngle);
                writer.Write(info.Slope);
                writer.Write(info.PathInfos.Count);
                /*
                public Vector2 Position; // 当前结点位置
                public float MoveTime; // 移动时间
                public int Type; // 路径类型 1直线 2贝塞尔曲线 3站在原地
                public float Rate; // 贝塞尔比率
                public float Speed;
                public float RotLerp; // 旋转插值
                public int playAni; // 是否播放动画
                */
                for (int i = 0; i < info.PathInfos.Count; i++)
                {
                    var node = info.PathInfos[i];
                    writer.Write(node.Position.x);
                    writer.Write(node.Position.y);

                    writer.Write(node.MoveTime);

                    writer.Write(node.Type);
                    writer.Write(node.Rate);
                    writer.Write(node.Speed);
                    writer.Write(node.RotLerp);
                    writer.Write(node.playAni);
                }
            }
            writer.Flush();
            return stream.ToArray();
        }

        public void AddRoute(CfgFishRoute info)
        {
            Debug.Log($"增加路径 {info.id}  {info.Description}");
            CfgFishRouteDouble ret = new CfgFishRouteDouble();
            m_DataDic[info.id] = ret;
            ret.id = info.id;
            ret.x = info.StartPos.x;
            ret.y = info.StartPos.y;
            ret.RouteType = info.RouteType;
            ret.TotalTime = (float)info.TotalTime;
            ret.FadeAway = info.FadeAway;
            ret.SwordAction = info.SwordAction;
            ret.FishRotate = (float)info.FishRotate;
            ret.SwordFishTime = (float)info.SwordFishTime;
            ret.FishAngle = (float)info.FishAngle;
            ret.Slope = (float)info.Slope;
            ret.Description = info.Description;

            ret.PathInfos = new List<CfgRouteNodeInfoDouble>();
            for (int i = 0; i < info.PathInfos.Count; i++)
            {
                var pathInfo = new CfgRouteNodeInfoDouble();
                ret.PathInfos.Add(pathInfo);
                var pathInfoDouble = info.PathInfos[i];
                pathInfo.x = pathInfoDouble.Position.x;
                pathInfo.y = pathInfoDouble.Position.y;

                pathInfo.MoveTime = pathInfoDouble.MoveTime;
                pathInfo.Type = pathInfoDouble.Type;
                pathInfo.Rate = pathInfoDouble.Rate;
                pathInfo.Speed = pathInfoDouble.Speed;
                pathInfo.RotLerp = pathInfoDouble.RotLerp;
                pathInfo.playAni = pathInfoDouble.playAni;
            }
        }

        public void MirrorRouteX(CfgFishRoute info)
        {
            Debug.Log($"左右镜像翻转路径 {info.id}  {info.Description}");
            CfgFishRouteDouble ret = new CfgFishRouteDouble();
            ret.id = info.id;
            ret.x = info.StartPos.x;
            ret.y = info.StartPos.y;
            ret.RouteType = info.RouteType;
            ret.TotalTime = (float)info.TotalTime;
            ret.FadeAway = info.FadeAway;
            ret.SwordAction = info.SwordAction;
            ret.FishRotate = (float)info.FishRotate;
            ret.SwordFishTime = (float)info.SwordFishTime;
            ret.FishAngle = (float)info.FishAngle;
            ret.Slope = (float)info.Slope;
            ret.Description = info.Description;

            ret.PathInfos = new List<CfgRouteNodeInfoDouble>();
            for (int i = 0; i < info.PathInfos.Count; i++)
            {
                var pathInfo = new CfgRouteNodeInfoDouble();
                ret.PathInfos.Add(pathInfo);
                var pathInfoDouble = info.PathInfos[i];
                pathInfo.x = pathInfoDouble.Position.x;
                pathInfo.y = pathInfoDouble.Position.y;

                pathInfo.MoveTime = pathInfoDouble.MoveTime;
                pathInfo.Type = pathInfoDouble.Type;
                pathInfo.Rate = pathInfoDouble.Rate;
                pathInfo.Speed = pathInfoDouble.Speed;
                pathInfo.RotLerp = pathInfoDouble.RotLerp;
                pathInfo.playAni = pathInfoDouble.playAni;
            }

            string path = "Assets/Mirror.txt";
            StringBuilder sb = new StringBuilder();
            LitJson.JsonWriter jr = new LitJson.JsonWriter(sb);
            jr.PrettyPrint = true;//设置为格式化模式，LitJson称其为PrettyPrint（美观的打印），在 Newtonsoft.Json里面则是 Formatting.Indented（锯齿状格式）
            jr.IndentValue = 4;//缩进空格个数
            LitJson.JsonMapper.ToJson(ret, jr);
            File.WriteAllText(path, sb.ToString());
            Debug.Log("左右镜像翻转路径 Save Route Json Success!");
        }

        public void Save()
        {
            List<CfgFishRouteDouble> list = new List<CfgFishRouteDouble>();
            var keys = GetAllKeys();
            for (int i = 0; i < keys.Count; i++)
            {
                list.Add(m_DataDic[keys[i]]);
            }
            string path = RoutePath;
            StringBuilder sb = new StringBuilder();
            LitJson.JsonWriter jr = new LitJson.JsonWriter(sb);
            jr.PrettyPrint = true;//设置为格式化模式，LitJson称其为PrettyPrint（美观的打印），在 Newtonsoft.Json里面则是 Formatting.Indented（锯齿状格式）
            jr.IndentValue = 4;//缩进空格个数
            LitJson.JsonMapper.ToJson(list, jr);
            File.WriteAllText(path, sb.ToString());
            Debug.Log("Save Route Json Success!");
        }
    }

    // 路径结点配置信息
    public class CfgRouteNodeInfo
    {
        public Vector2 Position; // 当前结点位置
        public float MoveTime; // 移动时间
        public int Type; // 路径类型 1直线 2贝塞尔曲线 3站在原地
        public float Rate; // 贝塞尔比率
        public float Speed;
        public float RotLerp; // 旋转插值
        public int playAni; // 是否播放动画

        public void Copy(CfgRouteNodeInfo other)
        {
            this.Position = new Vector2(other.Position.x, other.Position.y);
            this.MoveTime = other.MoveTime;
            this.Type = other.Type;
            this.Rate = other.Rate;
            this.Speed = other.Speed;
            this.RotLerp = other.RotLerp;
            this.playAni = other.playAni;
        }
    }

    // 单条路径配置信息
    public class CfgFishRoute
    {
        public int id;
        public Vector2 StartPos; // 起始位置
        public int RouteType; //
        public float TotalTime;
        public List<CfgRouteNodeInfo> PathInfos;
        public int FadeAway;
        public int SwordAction;
        public float FishRotate;
        public float SwordFishTime;
        public float FishAngle;
        public float Slope;
        public string Description;

        public float GetTotalTime()
        {
            float totalTime = 0;
            for (int i = 0; i < PathInfos.Count; i++)
            {
                totalTime += PathInfos[i].MoveTime;
            }
            return totalTime;
        }

        public float GetServerTotalTime()
        {
            return GetTotalTime() + 1.0f;
        }
    }

    // 路径结点配置信息
    public class CfgRouteNodeInfoDouble
    {
        public double x;
        public double y;
        public double MoveTime; // 移动时间
        public int Type; // 路径类型 1直线 2贝塞尔曲线 3站在原地
        public double Rate; // 贝塞尔比率
        public double Speed;
        public double RotLerp; // 旋转插值
        public int playAni; // 是否播放动画
    }

    // 单条路径配置信息
    public class CfgFishRouteDouble
    {
        public int id;
        public double x;
        public double y;
        public int RouteType; //
        public double TotalTime;
        public List<CfgRouteNodeInfoDouble> PathInfos;
        public int FadeAway;
        public int SwordAction;
        public double FishRotate;
        public double SwordFishTime;
        public double FishAngle;
        public double Slope;
        public string Description;
    }
}
