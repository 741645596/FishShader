using System.Collections.Generic;
using UnityEngine;
using System;

public class XRouteBezierQuardatic: XRouteBase
{
    XCfgRoute m_Config;
    List<XRouteNodeInfo> m_RouteNodeList = new List<XRouteNodeInfo>();
    bool m_HasSetLineAngle;
    int fishAngle;
    float m_CurZRotate;
    float m_CurYRotate;

    bool m_InitAngle;
    float m_ZAngle;

    public void Reset(XCfgRoute config)
    {
        m_Config = config;
        fishAngle = (int)m_Config.angle;
        if (defaultAngle != -999)
        {
            fishAngle = defaultAngle;
        }
        Init(m_Config.startPos);
    }

    public void Reset(XCfgRoute config, Vector2 startPos)
    {
        m_Config = config;
        fishAngle = (int)m_Config.angle;
        if (defaultAngle != -999)
        {
            fishAngle = defaultAngle;
        }
        Init(startPos);
    }

    private void Init(Vector2 pos)
    {
        depth = 0;
        alive = true;
        curMovingTime = 0;
        curMovePathIndex = 0;
        m_HasSetLineAngle = false;
        CalcRouteNodeInfo(pos);
    }

    void UpdateCurrentZRotate()
    {
        //m_CurZRotate = IsLeftToRight() ? -zRotate : zRotate;
        m_CurZRotate = zRotate;
        m_CurYRotate = 0;
        if (yRotate > 0 && curMovePathIndex >= 0 && curMovePathIndex < m_RouteNodeList.Count)
        {
            var info = m_RouteNodeList[curMovePathIndex];
            float offy = info.endPos.x - info.startPos.x;
            if (Mathf.Abs(offy) > 0.00001f)
            {
                bool ret = info.startPos.x < info.endPos.x;
                ret = XRouteUtils.MirrorFlip ? !ret : ret;
                m_CurYRotate = ret ? -yRotate : yRotate;
            }
        }

    }

    public override bool IsLeftToRight()
    {
        if (curMovePathIndex >= 0 && curMovePathIndex < m_RouteNodeList.Count)
        {
            var info = m_RouteNodeList[curMovePathIndex];
            bool ret = info.startPos.x < info.endPos.x;
            ret = XRouteUtils.MirrorFlip ? !ret : ret;
            return ret;
        }
        return false;
    }

    // 不受镜像翻转影响
    public override bool IsAbsoluteLeftToRight()
    {
        if (curMovePathIndex >= 0 && curMovePathIndex < m_RouteNodeList.Count)
        {
            var info = m_RouteNodeList[curMovePathIndex];
            bool ret = info.startPos.x < info.endPos.x;
            return ret;
        }
        return false;
    }

    public float GetAniamtionSpeed()
    {
        if (curMovePathIndex >= 0 && curMovePathIndex < m_RouteNodeList.Count)
        {
            var info = m_RouteNodeList[curMovePathIndex];
            return info.animationSpeed;
        }
        return 1;
    }

    float CalcAngle(XRouteNodeInfo info)
    {
        float angleCount = 0;
        if (info.type == XRouteConsts.ROUTE_TYPE_BEZIRER) // 贝塞尔曲线
        {
            if (fishAngle == 9999)
            {
                angleCount = XRouteUtils.GetBezierAngleByPos(info, 1.0f);
                angleCount = 270 + angleCount;
                if (angleCount != 0)
                {
                    angleCount = angleCount % 360;
                    if (angleCount > 180)
                    {
                        angleCount -= 360;
                    }
                }
                if (XRouteUtils.MirrorFlip)
                {
                    angleCount += 180;
                }
            }
            else
            {
                angleCount = fishAngle;
            }

        }
        else if (info.type == XRouteConsts.ROUTE_TYPE_LINE) // 直线
        {
            if (fishAngle == 9999)
            {
                float angle = info.lineAngle;
                angleCount = 270 + angle;
                if (XRouteUtils.MirrorFlip)
                {
                    angleCount += 180;
                }
            }
            else
            {
                angleCount = fishAngle;
            }
        }
        return angleCount;
    }

    void OnNodeChange(XRouteNodeInfo node)
    {
        playAni = node.playAni;
        animationSpeed = node.animationSpeed;
        velocity = node.velocity;
        changeNodeCallback?.Invoke(this);
    }

    public override void GotoFrame(float bornTime)
    {
        m_InitAngle = false;
        alive = false;
        float angleCount = fishAngle;
        for (int i = 0; i < m_RouteNodeList.Count; i++)
        {
            var info = m_RouteNodeList[i];
            if (bornTime < info.moveTime)
            {
                curMovePathIndex = i;
                curMovingTime = bornTime;
                alive = true;
                UpdateCurrentZRotate();
                OnNodeChange(info);
                break;
            }
            else
            {
                angleCount = CalcAngle(info);
                UpdateAngle(angleCount);
                bornTime -= info.moveTime;
            }
        }
        if (!alive)
        {
            return;
        }
        m_HasSetLineAngle = false;
        UpdatePosition();

    }

    public override void UpdateRoute(float dt)
    {
        if (curMovePathIndex >= m_RouteNodeList.Count)
        {
            alive = false;
            return;
        }
        curMovingTime += dt;
        while (true)
        {
            UpdatePosition();
            XRouteNodeInfo info = m_RouteNodeList[curMovePathIndex];
            if (curMovingTime >= info.moveTime)
            {
                curMovePathIndex++;
                if (curMovePathIndex >= m_RouteNodeList.Count)
                {
                    alive = false;
                    break;
                }
                else
                {
                    UpdateCurrentZRotate();
                    float t = curMovingTime;
                    curMovingTime = 0;
                    OnNodeChange(info);
                    curMovingTime = t - info.moveTime;
                    m_HasSetLineAngle = false;
                }
            }
            else
            {
                break;
            }
        }
    }

    private void UpdateAngle(float angle)
    {
        m_ZAngle = angle;
        localEulerAngles = new Vector3(m_CurZRotate, m_CurYRotate, angle);
    }

    private void UpdatePosition()
    {
        if (curMovePathIndex >= m_RouteNodeList.Count)
        {
            alive = false;
            return;
        }
        XRouteNodeInfo info = m_RouteNodeList[curMovePathIndex];
        if (info.type == XRouteConsts.ROUTE_TYPE_BEZIRER) // 贝塞尔曲线
        {
            float ratio = Mathf.Clamp01(curMovingTime / info.moveTime);
            Vector3 newPos = XRouteUtils.GetBezierPos(info, ratio);
            if (fishAngle == 9999)
            {
                float angleCount = XRouteUtils.GetBezierAngleByPos(info, ratio);
                angleCount = 270 + angleCount;
                if (angleCount != 0)
                {
                    angleCount = angleCount % 360;
                    if (angleCount > 180)
                    {
                        angleCount -= 360;
                    }
                }
                if (XRouteUtils.MirrorFlip)
                {
                    angleCount += 180;
                }
                UpdateLocalPositionAndRotation(newPos, angleCount);
            }
            else
            {
                UpdateLocalPosition(newPos);
            }
            if (!m_HasSetLineAngle)
            {
                m_HasSetLineAngle = true;
            }
        }
        else if (info.type == XRouteConsts.ROUTE_TYPE_LINE) // 直线
        {
            float ratio = Mathf.Clamp01(curMovingTime / info.moveTime);
            Vector3 newPos = XRouteUtils.GetLinePos(info, ratio);
            UpdateLocalPosition(newPos);
            if (!m_HasSetLineAngle)
            {
                m_HasSetLineAngle = true;
                if (fishAngle != 9999)
                {
                    UpdateAngle(fishAngle);
                }
                else
                {
                    float angle = info.lineAngle;
                    float angleCount = 270 + angle;
                    if (XRouteUtils.MirrorFlip)
                    {
                        angleCount += 180;
                    }
                    UpdateAngle(angleCount);
                }
            }
        }
        else if (info.type == XRouteConsts.ROUTE_TYPE_STANDING) // 站在原地
        {
            if (!m_HasSetLineAngle)
            {
                m_HasSetLineAngle = true;
                UpdateLocalPosition(info.endPos);
                if (yRotate > 0)
                {
                    localEulerAngles.y = m_CurYRotate;
                }

                if (m_CurZRotate > 0)
                {
                    localEulerAngles.x = m_CurZRotate;
                }
            }
        }
    }

    // 计算节点信息
    private void CalcRouteNodeInfo(Vector2 startPos)
    {
        for (int i = m_RouteNodeList.Count - 1; i >= m_Config.pathInfos.Count; i--)
        {
            m_RouteNodeList.RemoveAt(i);
        }
        int count = m_RouteNodeList.Count;
        float angle = 0;
        for (int i = 0; i < m_Config.pathInfos.Count; i++)
        {
            XRouteNodeInfo info = null;
            if (i >= count)
            {
                info = new XRouteNodeInfo();
                m_RouteNodeList.Add(info);
            }
            else
            {
                info = m_RouteNodeList[i];
            }
            var src = m_Config.pathInfos[i];
            info.startPos = XRouteUtils.ConvertToWorldPosition(startPos.x, startPos.y);
            info.endPos = XRouteUtils.ConvertToWorldPosition(src.position.x, src.position.y);
            info.middlePos = XRouteUtils.GetBezierMiddlePosByRate(info.startPos, info.endPos, src.rate);
            info.moveTime = src.time;
            if (src.type == XRouteConsts.ROUTE_TYPE_BEZIRER && !(Mathf.Abs(src.rate) > 0.000001f))
            {
                info.type = XRouteConsts.ROUTE_TYPE_LINE;
            }
            else
            {
                info.type = src.type;
            }
            if (info.type == XRouteConsts.ROUTE_TYPE_LINE)
            {
                angle = Mathf.Atan2(info.endPos.y - info.startPos.y, info.endPos.x - info.startPos.x) * Mathf.Rad2Deg;
                info.lineAngle = angle;
            }
            else if (info.type == XRouteConsts.ROUTE_TYPE_STANDING)
            {
                info.lineAngle = angle;
            }
            else
            {
                angle = 0;
                info.lineAngle = 0;
            }
            info.rotLerp = src.lerp;
            info.playAni = src.playAni;
            info.animationSpeed = src.speed;
            info.velocity = (info.startPos - info.endPos).magnitude / info.moveTime;
            startPos = src.position;
        }
        Debug.Assert(m_Config.pathInfos.Count == m_RouteNodeList.Count, "CalcRouteNodeInfo Error");
    }

    private void UpdateLocalPosition(Vector2 newPos)
    {
        if (XRouteUtils.MirrorFlip)
        {
            newPos.x = -newPos.x;
            newPos.y = -newPos.y;
        }
        localPosition.x = newPos.x;
        localPosition.y = newPos.y;
        localPosition.z = depth;
    }

    private void UpdateLocalPositionAndRotation(Vector2 newPos, float angle)
    {
        if (XRouteUtils.MirrorFlip)
        {
            newPos.x = -newPos.x;
            newPos.y = -newPos.y;
        }
        localPosition.x = newPos.x;
        localPosition.y = newPos.y;
        localPosition.z = depth;
        if (m_InitAngle)
        {
            m_ZAngle = Mathf.LerpAngle(m_ZAngle, angle, XRouteUtils.RotateLerp);
        }
        else
        {
            m_InitAngle = true;
            m_ZAngle = angle;
        }
        localEulerAngles = new Vector3(m_CurZRotate, m_CurYRotate, m_ZAngle);
    }
}
