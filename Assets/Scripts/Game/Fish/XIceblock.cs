using System;
using UnityEngine;
using System.Collections.Generic;

// 冰块
class XIceBlock
{
    static int _Cutoff = Shader.PropertyToID("_Cutoff");
    List<Material> materials;
    List<GameObject> gameObjects;
    bool cutoff;
    bool enter;
    float currentTime = 0;

    const float CuteOffTime = 1.0f;

    public void Start()
    {
        enter = true;
        cutoff = false;
        currentTime = 0;
        SetEnable(true);
    }

    public void Stop()
    {
        enter = false;
        cutoff = false;
        currentTime = 0;
        SetEnable(false);
    }

    void SetEnable(bool bo)
    {
        if (gameObjects == null) return;
        for (int i = 0; i < gameObjects.Count; i++)
        {
            gameObjects[i].SetActive(bo);
        }
    }

    public void Clear()
    {
        if (materials != null)
        {
            materials.Clear();
            materials = null;
        }
    }

    public void Update(float remainTime, float dt)
    {
        if (materials == null)
        {
            return;
        }
        if (cutoff)
        {
            currentTime += dt;
            float ratio = (CuteOffTime - currentTime) / CuteOffTime;
            SetRatio(ratio);
        }
        else if (remainTime < CuteOffTime)
        {
            currentTime = 0;
            cutoff = true;
        }
        else if (enter)
        {
            currentTime += dt;
            float ratio = currentTime / CuteOffTime;
            enter = ratio < 1.0f;
            SetRatio(ratio);
        }
    }

    public void AddGameObject(GameObject go)
    {
        if (gameObjects == null)
        {
            gameObjects = new List<GameObject>();
        }
        gameObjects.Add(go);
    }

    public void AddMaterial(Material mat)
    {
        if (materials == null)
        {
            materials = new List<Material>();
        }
        materials.Add(mat);
    }

    public void SetRatio(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        for (int i = 0; i < materials.Count; i++)
        {
            materials[i].SetFloat(_Cutoff, ratio);
        }
    }
}
