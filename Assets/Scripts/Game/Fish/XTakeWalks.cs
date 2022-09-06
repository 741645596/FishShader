using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 散步离场动作
public class XTakeWalks
{
    GameObject gameObject;
    bool runAction;
    float totalTime;
    float currentTime;
    Vector3 walkSpeed;
    bool stay;

    public void SetGameObject(GameObject go)
    {
        gameObject = go;
    }

    public bool IsRunning()
    {
        return runAction;
    }

    public void Update(float dt)
    {
        if (!runAction) return;
        currentTime += dt;
        if (currentTime > totalTime)
        {
            runAction = false;
        }
        if (!stay)
        {
            gameObject.transform.Translate(walkSpeed * dt, Space.World);
        }
    }

    public void Play(float duration, float speed, bool stay)
    {
        runAction = true;
        walkSpeed = gameObject.transform.up.normalized * 9.0f;
        currentTime = 0;
        totalTime = duration;
        this.stay = stay;
    }

    public void StopAction()
    {
        runAction = false;
    }
}