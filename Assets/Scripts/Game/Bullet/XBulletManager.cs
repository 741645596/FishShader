using System;
using System.Collections.Generic;
using UnityEngine;

public class XBulletManager : MonoBehaviour
{
    static XBulletManager sInstance;
    public static XBulletManager Instance { get { return sInstance; } }
    List<XBullet> m_BulletList;
    Dictionary<int, XBullet> m_BulletDic;
    public int BulletCount;
    float m_CollisionTimer;
    float m_CollisionInterval = 1.0f / 11.0f;

    private void Start()
    {
        XBulletUtils.Init();
        sInstance = this;
        m_BulletList = new List<XBullet>();
        m_BulletDic = new Dictionary<int, XBullet>();
        m_CollisionInterval = 1.0f / 11.0f;
    }

    public void SetCollisionInterval(float interval)
    {
        m_CollisionInterval = interval;
    }

    private void UpdateAllBullets(float dt)
    {
        BulletCount = m_BulletList.Count;
        for (int i = m_BulletList.Count - 1; i >= 0; i--)
        {
            m_BulletList[i].UpdateBullet(dt);
        }
    }

    private void Update()
    {
        if (m_CollisionTimer > m_CollisionInterval)
        {
            m_CollisionTimer = 0;
            for (int i = m_BulletList.Count - 1; i >= 0; i--)
            {
                var bullet = m_BulletList[i];
                if (bullet.NeedCollision())
                {
                    var worldPos = bullet.GetWorldPosition();
                    if (XBulletUtils.BulletAndFish(worldPos, bullet.GetBulletRadius()))
                    {
                        int uid = bullet.GetUID();
                        XBulletUtils.GetCollisionFish(bullet.GetHitFishes(), worldPos, bullet.GetNetRadius());
                        bullet.OnHit();
                        bullet.Reset();
                        m_BulletDic.Remove(uid);
                        m_BulletList.RemoveAt(i);
                    }
                }
            }
        }
        else
        {
            m_CollisionTimer += Time.deltaTime;
        }
        UpdateAllBullets(Time.deltaTime);
    }

    public void SetBulletEnable(XBullet bullet)
    {
        int uid = bullet.GetUID();
        m_BulletList.Add(bullet);
        m_BulletDic[uid] = bullet;
    }

    public XBullet FindBullet(int uid)
    {
        XBullet bullet;
        if (!m_BulletDic.TryGetValue(uid, out bullet))
        {
            return null;
        }
        return bullet;
    }

    public void RemoveBullet(XBullet bullet)
    {
        int uid = bullet.GetUID();
        bullet.OnHit();
        bullet.Reset();
        m_BulletDic.Remove(uid);
        m_BulletList.Remove(bullet);
    }
}
