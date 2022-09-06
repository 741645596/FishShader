using System;
using System.Collections.Generic;

public class LanguageManager
{
    class LanguageUnit
    {
        public int id;
        public string chs;
        public string en;
    }
    Dictionary<int, LanguageUnit> m_DataDic;

    LanguageType language = LanguageType.CHS;
    string languageDir = "/CHS/";

    private static LanguageManager m_Instance;
    public static LanguageManager Instance { get { if (m_Instance == null) { m_Instance = new LanguageManager(); } return m_Instance; } }


    public void Reload(string text)
    {
        m_DataDic = new Dictionary<int, LanguageUnit>();
        List<LanguageUnit> list = LitJson.JsonMapper.ToObject<List<LanguageUnit>>(text);
        for (int i = 0; i < list.Count; i++)
        {
            m_DataDic[list[i].id] = list[i];
        }
    }

    public LanguageType GetLanguageType()
    {
        return language;
    }

    public void SetLanguageType(LanguageType t)
    {
        language = t;
        languageDir = $"/{t.ToString()}/";
    }

    public string Get(int id)
    {
        if (m_DataDic.ContainsKey(id))
        {
            string ret = "";
            switch (language)
            {
                case LanguageType.CHS:
                    ret = m_DataDic[id].chs;
                    break;

                case LanguageType.EN:
                    ret = m_DataDic[id].en;
                    break;
            }
            return ret;
        }
        LogUtils.E($"无效的Language id {id}");
        return "";
    }

    public string ChangeLanguagePath(string path)
    {
        if (LanguageType.CHS == language)
        {
            return path;
        }
        var ret = path.Replace("/CHS/", languageDir);
        return ret;
    }

}
