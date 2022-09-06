using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameObjectUtils
{
    static GameObject s_ManagerRoot;
    public static GameObject CreateUniQueGameObject(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go != null)
        {
            GameObject.Destroy(go);
        }
        go = new GameObject(name);
        return go;
    }

    // 创建管理对象
    public static GameObject CreateManagerGameObject(string name)
    {
        if (s_ManagerRoot == null)
        {
            s_ManagerRoot = new GameObject();
            s_ManagerRoot.name = "Managers";
        }
        var tf = s_ManagerRoot.transform.Find(name);
        if (tf != null)
        {
            GameObject.Destroy(tf.gameObject);
        }
        var go = new GameObject(name);
        go.transform.SetParent(s_ManagerRoot.transform, false);
        return go;
    }

    // 设置渲染顺序
    public static void SetSortingOrderAndLayer(GameObject go, string layerName, int sortingOrder)
    {
        int layer = LayerMask.NameToLayer(layerName);
        var components = go.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < components.Length; i++)
        {
            components[i].gameObject.layer = layer;
            components[i].sortingOrder = sortingOrder;
        }
    }

    // 设置渲染顺序
    public static void SetSortingOrder(GameObject go, int sortingOrder)
    {
        var components = go.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < components.Length; i++)
        {
            components[i].sortingOrder = sortingOrder;
        }
    }

    // 增加渲染顺序
    public static void AddSortingOrder(GameObject go, int sortingOrder)
    {
        var components = go.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < components.Length; i++)
        {
            int baseOrder = components[i].sortingOrder;
            baseOrder = baseOrder % 100;
            var obj = components[i].gameObject;
            components[i].sortingOrder = baseOrder + sortingOrder;
        }
    }

    // 改变层级
    public static void ChangeLayer(GameObject obj, string layerName)
    {
        var trans = obj.GetComponentsInChildren<Transform>(true);
        int layer = LayerMask.NameToLayer(layerName);
        for (int i = 0; i < trans.Length; i++)
        {
            trans[i].gameObject.layer = layer;
        }
    }

    public static void ChangeLayer(GameObject obj, int layer)
    {
        var trans = obj.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < trans.Length; i++)
        {
            trans[i].gameObject.layer = layer;
        }
    }

    // 通过名字查找子节点
    public static Transform FindTransformByName(GameObject go, string transName)
    {
        var list = go.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i].name == transName)
            {
                return list[i];
            }
        }
        return null;
    }

    public static void Bind(GameObject go, Dictionary<string, GameObject> dic)
    {
        var transforms = go.transform.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            var obj = transforms[i].gameObject;
            dic[obj.name] = obj;
        }
    }

    public static void BindAndLocalization(GameObject go, Dictionary<string, GameObject> dic)
    {
        var transforms = go.transform.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            var obj = transforms[i].gameObject;
            dic[obj.name] = obj;
        }
        RemoveFontBold(go);
        var lt = LanguageManager.Instance.GetLanguageType();
        if (lt == LanguageType.CHS)
        {
            return;
        }
        // 替换图片
        var images = go.transform.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            var img = images[i];
            if (img.MutilLanguage)
            {
                var path = AssetsMgr.FullPath(LanguageManager.Instance.ChangeLanguagePath(img.ImagePath));
                //LogUtils.V(path);
                img.sprite = AssetsMgr.Load<Sprite>(path);
                img.SetNativeSize();
            }
        }
        // 替换文本
        var texts = go.transform.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            int id = texts[i].GetLanguageID();
            if (id > 0)
            {
                var str = LanguageManager.Instance.Get(id);
                texts[i].text = str;
            }
        }
    }

    static void RemoveFontBold(GameObject go)
    {
        // 替换文本
        var texts = go.transform.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].fontStyle == FontStyle.Bold)
            {
                texts[i].fontStyle = FontStyle.Normal;
            }
        }
    }

    public static float CalcParticleSystemDuration(GameObject go)
    {
        float maxDuration = 0;
        var p = go.GetComponent<ParticleSystem>();
        if (p != null)
        {
            maxDuration = GetParticalDuration(p);
        }
        var array = go.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < array.Length; i++)
        {
            float life = GetParticalDuration(array[i]);
            maxDuration = Math.Max(maxDuration, life);
        }
        return maxDuration;
    }

    public static float GetParticalDuration(ParticleSystem particle, bool allowLoop = false)
    {
        if (!particle.emission.enabled) return 0f;
        if (particle.main.loop && !allowLoop)
        {
            return -1f;
        }
        if (particle.emission.rateOverTime.constantMin <= 0)
        {
            return particle.main.startDelay.constantMax + particle.main.startLifetime.constantMax;
        }
        else
        {
            return particle.main.startDelay.constantMax + Mathf.Max(particle.main.duration, particle.main.startLifetime.constantMax);
        }
    }
}

