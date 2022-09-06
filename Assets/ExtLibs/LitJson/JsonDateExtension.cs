using System.Collections;

namespace LitJson
{
    public static class JsonDateExtension
    {
        public static bool IsNull(this JsonData target, string key)
        {
            if(target.IsObject)
            {
                var dic = (IDictionary)target;
                if(dic.Contains(key))
                {
                    if(dic[key] == null)
                    {
                        return true;
                    }
                    return false;
                }
            }
            return true;
        }

        public static string GetString(this JsonData target, string key, string defultValue = "")
        {
            if(target.IsNull(key) || target[key] == null)
            {
                return defultValue;
            }

            return target[key].ToString();
        }

        public static int GetInt(this JsonData target, string key, int defultValue = 0)
        {
            if (target.IsNull(key) || target[key] == null)
            {
                return defultValue;
            }
            int.TryParse(target[key].ToString(), out int result);
            return result;
        }

        public static long GetLong(this JsonData target, string key, long defultValue = 0)
        {
            if (target.IsNull(key) || target[key] == null)
            {
                return defultValue;
            }
            long.TryParse(target[key].ToString(), out long result);
            return result;
        }

        public static bool GetBool(this JsonData target, string key, bool defultValue = false)
        {
            if (target.IsNull(key) || target[key] == null)
            {
                return defultValue;
            }
            bool.TryParse(target[key].ToString(), out bool result);
            return result;
        }

        public static float GetFloat(this JsonData target, string key, float defultValue = 0)
        {
            if (target.IsNull(key) || target[key] == null)
            {
                return defultValue;
            }
            float.TryParse(target[key].ToString(), out float result);
            return result;
        }

        public static float GetFloat(this JsonData target, int index, float defultValue = 0)
        {
            if(index >= target.Count)
            {
                return defultValue;
            }
            float.TryParse(target[index].ToString(), out float resutl);
            return resutl;
        }
    }
}