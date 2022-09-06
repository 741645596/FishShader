using System.Collections.Generic;
using UnityEngine;

public class XBulletTest : XBullet
{
    static int TestUID = 0;
    protected override void Awake()
    {
        base.Awake();
    }

    protected void Start()
    {
        SetUID(TestUID++);
        if (worldSpace)
        {
            Launch(30, new Vector3(0, 0, 100), 7.00f);
        }
        else
        {
            Launch(30, new Vector3(0, 0, 0), 700f);
        }
        XBulletManager.Instance.SetBulletEnable(this);
    }

    private void Update()
    {
        UpdateBullet(Time.deltaTime);
    }
}
