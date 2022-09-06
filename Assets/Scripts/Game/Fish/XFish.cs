using System;
using UnityEngine;
using System.Collections.Generic;

public class XFish: MonoBehaviour
{
    public static bool EnableShadow = true;
    public int fishid;
    public int uid;
    XRoute m_Route = new XRoute();
    Action<float> m_UpdateRouteHandler = null;
    FishStatus m_FishStatus;
    
    bool m_Alive;
    bool m_UseRealtime;

    Action m_RouteEndListener;
    float m_LastUpdateTime;
    Animator[] m_Animators;
    CapsuleCollider[] m_Colliders;
    bool m_ColliderEnable;
    SkinnedMeshRenderer[] m_SkinnedMeshRenderers;
    float m_CurrentAnimationSpeed;
    float m_FrozenTime;
    float m_LastAngle = -999;

    XFishTurnRed m_TurnRed;
    XTakeWalks m_TakeWalks;
    XFishPlate m_FishPlate;
    XIceBlock m_IceBlock;

    XFishInfo m_FishInfo;
    protected XRouteAction m_RouteAction;
    XIdleAction m_IdleAction;
    XFishChangeSkin m_ChangeSkin;
    XBindEffectToAnimation m_BindEffectToAnimation;
    XFishAnimation m_FishAnimation;

    List<Material> m_AllMaterials = new List<Material>();

    static int _ShadowProjDir = Shader.PropertyToID("_ShadowProjDir");
    static int _LightStr = Shader.PropertyToID("_LightStr");
    static int _IsHurt = Shader.PropertyToID("_IsHurt");

    public void Awake()
    {
        m_UseRealtime = false;
        m_RouteEndListener = null;
        m_TakeWalks = new XTakeWalks();
        m_TakeWalks.SetGameObject(gameObject);
        m_TurnRed = new XFishTurnRed();
    }

    private void OnDestroy()
    {
        if (m_Colliders != null)
        {
            for (int i = 0; i < m_Colliders.Length; i++)
            {
                XFishUtils.RemoveColliderFish(m_Colliders[i].GetInstanceID());
            }
            m_Colliders = null;
        }
        m_RouteEndListener = null;
        foreach (Material mMat in m_AllMaterials)
        {
            UnityEngine.Object.DestroyImmediate(mMat);
        }
        m_AllMaterials.Clear();
        m_AllMaterials = null;
    }


    public void SetUID(int id)
    {
        uid = id;
    }

    public int GetUID()
    {
        return uid;
    }

    #region 路径相关
    public XRoute GetRoute()
    {
        return m_Route;
    }

    public void ResetRoute(int routeid)
    {
        m_Route.Reset(routeid);
        m_Route.SetTransfrom(transform);
        m_Route.SetChangeNodeCallback(OnRouteNodeChange);
    }

    public void ResetRoute(int routeid, Vector2 startPos)
    {
        m_Route.Reset(routeid, startPos);
        m_Route.SetTransfrom(transform);
        m_Route.SetChangeNodeCallback(OnRouteNodeChange);
    }

    public void SetDepth(float depth)
    {
        m_Route.SetDepth(depth);
    }

    public void GotoFrame(float bornTime)
    {
        m_Route.GotoFrame(bornTime);
    }

    public bool IsRouteAlive()
    {
        return m_Route.IsRouteAlive();
    }

    public void SyncRouteAnimation()
    {
        SyncAnimation(m_Route.GetCurMovingTime());
    }

    public bool IsRouteLeftToRight()
    {
        return true;
    }

    public void SetRouteEndListener(Action cb)
    {
        m_RouteEndListener = cb;
    }

    void OnRouteNodeChange(XRouteBase route)
    {
        float speed = route.animationSpeed;
        SetColliderEnable(true);
        bool isLeftToRight = route.IsLeftToRight();
        if (m_FishInfo != null)
        {
            switch (m_FishInfo.FlipType)
            {
                case FishFlipType.FlipX:
                    {
                        if (isLeftToRight)
                        {
                            SetFlipX(false);
                        }
                        else
                        {
                            SetFlipX(true);
                        }
                        break;
                    }

                case FishFlipType.CenterFlip:
                    {
                        if (route.IsAbsoluteLeftToRight())
                        {
                            if (XRouteUtils.MirrorFlip)
                            {
                                SetFlipXY(true, true);
                            }
                            else
                            {
                                SetFlipXY(false, false);
                            }
                        }
                        else
                        {
                            if (XRouteUtils.MirrorFlip)
                            {
                                SetFlipXY(false, true);
                            }
                            else
                            {
                                SetFlipXY(true, false);
                            }
                        }
                        break;
                    }
            }
            if (m_FishInfo.SpeedMax != m_FishInfo.SpeedMin)
            {
                m_FishInfo.Velocity = route.velocity;
                speed *= CalcVelocitySpeed(route.velocity);
            }
        }
        SetAnimationSpeed(speed);
        m_RouteAction?.OnRouteAni(route.playAni, m_Route.GetCurMovingTime());
        m_FishAnimation?.OnRouteNodeChange(isLeftToRight, route.velocity);
    }

    public void SetUpdateRouteHandler(Action<float> cb)
    {
        m_UpdateRouteHandler = cb;
    }

    // 使用本地时间更新鱼的位置
    public void SetUseRealtime(bool bo)
    {
        m_UseRealtime = bo;
    }

    public XRouteAction GetRouteAction()
    {
        return m_RouteAction;
    }

    float CalcVelocitySpeed(float velocity)
    {
        const float MAX_SPEED = 3.0f;
        velocity = Mathf.Abs(velocity);
        velocity = Mathf.Min(MAX_SPEED, velocity);
        float ratio = velocity / MAX_SPEED;
        float velocity2Speed = Mathf.Lerp(m_FishInfo.SpeedMin, m_FishInfo.SpeedMax, ratio);
        //LogUtils.I($"{velocity} {velocity2Speed}");
        return velocity2Speed;
    }

    public void SetVelocity(float velocity)
    {
        float speed = CalcVelocitySpeed(velocity);
        SetAnimationSpeed(speed);
    }

    #endregion

    public void SetFishPlate(XFishPlate plate)
    {
        m_FishPlate = plate;
    }

    public void Reset()
    {
        m_FishStatus = FishStatus.kFISH_STATUS_MOVE;
        m_Alive = true;
        m_FrozenTime = 0;
        m_UpdateRouteHandler = null;
        m_TurnRed.StopAction();
        m_LastAngle = -999;
        SetColliderEnable(true);
        m_RouteAction?.Reset();
        m_IdleAction?.Reset();
        m_FishAnimation?.Reset();
        EndFrozen();
    }

    public void SetFlipX(bool flipX)
    {
        var localScale = transform.localScale;
        float x = Math.Abs(localScale.x);
        localScale.x = flipX ? -x : x;
        transform.localScale = localScale;
    }

    public void SetFlipY(bool flipY)
    {
        var localScale = transform.localScale;
        float y = Math.Abs(localScale.y);
        localScale.y = flipY ? -y : y;
        transform.localScale = localScale;
    }

    public void SetFlipXY(bool flipX, bool flipY)
    {
        var localScale = transform.localScale;
        float x = Math.Abs(localScale.x);
        localScale.x = flipX ? -x : x;
        float y = Math.Abs(localScale.y);
        localScale.y = flipY ? -y : y;
        transform.localScale = localScale;
    }

    #region 更新

    public void StartUpdate()
    {
        m_LastUpdateTime = Time.realtimeSinceStartup;
    }

    public void UpdateFish(float dt)
    {
        m_TurnRed.Update(dt);
        switch (m_FishStatus)
        {
            // 散步
            case FishStatus.kFISH_STATUS_TAKE_WALK:
                {
                    UpdateTalkWalk(dt);
                    break;
                }
            // 正常游走
            case FishStatus.kFISH_STATUS_MOVE:
                {
                    UpdateMove(dt);
                    break;
                }
            // 死亡
            case FishStatus.kFISH_STATUS_DEAD:
                {
                    break;
                }

            case FishStatus.kFish_STATUS_TEST:
                {
                    UpdateTest();
                    break;
                }
        }
    }

    void UpdateTest()
    {
        m_FishInfo?.UpdateShadow();
        m_BindEffectToAnimation?.UpdateNode(Time.deltaTime);
    }

    void UpdateTalkWalk(float dt)
    {
        m_TakeWalks.Update(dt);
        if (!m_TakeWalks.IsRunning())
        {
            m_FishStatus = FishStatus.kFISH_STATUS_NONE;
            m_RouteEndListener?.Invoke();
            m_Alive = false;
        }
    }

    void UpdateMove(float dt)
    {
        float stepTime = dt;
        if (m_UseRealtime)
        {
            stepTime = Time.realtimeSinceStartup - m_LastUpdateTime;
            m_LastUpdateTime = Time.realtimeSinceStartup;
        }
        if (m_FrozenTime > 0)
        {
            if (stepTime > m_FrozenTime)
            {
                m_FrozenTime = 0;
                stepTime -= m_FrozenTime;
            }
            else
            {
                m_FrozenTime -= stepTime;
            }
            if (m_FrozenTime <= 0)
            {
                EndFrozen();
            }
            else
            {
                m_IceBlock?.Update(m_FrozenTime, dt);
            }
        }
        if (m_FrozenTime <= 0)
        {
            m_BindEffectToAnimation?.UpdateNode(stepTime);
            if (m_UpdateRouteHandler != null)
            {
                m_UpdateRouteHandler.Invoke(stepTime);
                m_Route.UpdateRouteTime(stepTime);
                if (!m_Route.IsRouteAlive())
                {
                    m_Alive = false;
                    m_FishStatus = FishStatus.kFISH_STATUS_NONE;
                    m_RouteEndListener?.Invoke();
                }
            }
            else
            {
                m_IdleAction?.UpdateIdleAction(stepTime);
                m_RouteAction?.UpdateRouteAction(stepTime);
                m_ChangeSkin?.UpdateSkin();
                m_Route.UpdateRoute(stepTime);
                if (!m_Route.IsRouteAlive())
                {
                    m_Alive = false;
                    m_FishStatus = FishStatus.kFISH_STATUS_NONE;
                    m_RouteEndListener?.Invoke();
                }
            }
        }
    }

    #endregion

    public void InitModel()
    {
        // 动画状态机
        m_Animators = GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < m_Animators.Length; i++)
        {
            m_Animators[i].logWarnings = false;
        }
        // 碰撞器
        m_Colliders = GetComponentsInChildren<CapsuleCollider>(true);
        for (int i = 0; i < m_Colliders.Length; i++)
        {
            XFishUtils.AddColliderFish(m_Colliders[i].GetInstanceID(), this);
        }

        // 网格
        m_SkinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        // 材质
        List<Material> shadowMaterials = new List<Material>();
        m_FishAnimation = GetComponent<XFishAnimation>();
        if (m_FishAnimation != null)
        {
            var spriteRenders = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < spriteRenders.Length; i++)
            {
                if (spriteRenders[i].name == "ActionSprite")
                {
                    m_TurnRed.AddSpriteRenderer(spriteRenders[i]);
                }
            }
            m_FishAnimation.InitSortingOrder(fishid);
        }
        else
        {
            if (!EnableShadow)
            {
                RemoveShadowMaterial();
            }

            for (int i = 0; i < m_SkinnedMeshRenderers.Length; i++)
            {
                SkinnedMeshRenderer render = m_SkinnedMeshRenderers[i];
                int length = render.materials.Length;
                if (length == 0) continue;
                bool hasIce = false;
                for (int j = 0; j < length; j++)
                {
                    var mat = render.materials[j];
                    string name = mat.shader.name;
                    if (mat.HasProperty(_ShadowProjDir)) // 阴影
                    {
                        if (EnableShadow)
                        {
                            shadowMaterials.Add(mat);
                            m_AllMaterials.Add(mat);
                        }
                    }
                    else if (mat.HasProperty(_LightStr)) // 冰块
                    {
                        if (m_IceBlock == null)
                        {
                            m_IceBlock = new XIceBlock();
                        }
                        m_IceBlock.AddMaterial(mat);
                        hasIce = true;
                        m_AllMaterials.Add(mat);
                    }
                    else // 鱼本身
                    {
                        m_TurnRed.AddMaterial(mat);
                        m_AllMaterials.Add(mat);
                    }
                }
                if (hasIce)
                {
                    m_IceBlock.AddGameObject(render.gameObject);
                }
            }
        }

        m_FishInfo = GetComponentInChildren<XFishInfo>();
        m_ChangeSkin = GetComponent<XFishChangeSkin>();
        if (m_FishInfo != null)
        {
            if (EnableShadow)
            {
                m_FishInfo.InitShadow(shadowMaterials);
            }
            m_Route.SetLocalEulerAngles(m_FishInfo.RouteAngle, m_FishInfo.YRotate, m_FishInfo.ZRotate);
            m_FishInfo.InitLockPoint();
        }
        m_RouteAction = GetComponent<XRouteAction>();
        if (m_RouteAction != null)
        {
            m_RouteAction.SetXFish(this);
            m_RouteAction.Init();
        }
        m_IdleAction = GetComponent<XIdleAction>();
        if (m_IdleAction != null)
        {
            m_IdleAction.SetXFish(this);
        }
        m_BindEffectToAnimation = GetComponent<XBindEffectToAnimation>();
        if (m_BindEffectToAnimation != null)
        {
            m_BindEffectToAnimation.SetAnimator(GetMainAnimator());
        }
    }

    void RemoveShadowMaterial()
    {
        for (int i = 0; i < m_SkinnedMeshRenderers.Length; i++)
        {
            SkinnedMeshRenderer render = m_SkinnedMeshRenderers[i];
            int length = render.materials.Length;
            if (length == 0) continue;
            int shadowCount = 0;
            for (int j = 0; j < length; j++)
            {
                var mat = render.materials[j];
                string name = mat.shader.name;
                if (mat.HasProperty(_ShadowProjDir))
                {
                    shadowCount++;
                }
            }
            // 移除阴影的材质
            if (shadowCount > 0)
            {
                if (length == 1)
                {
                    GameObject.Destroy(render.gameObject);
                }
                else
                {
                    Material[] newArray = new Material[length - shadowCount];
                    int idx = 0;
                    for (int j = 0; j < length; j++)
                    {
                        Material mat = render.materials[j];
                        if (!mat.HasProperty(_ShadowProjDir))
                        {
                            newArray[idx] = mat;
                            idx++;
                        }
                        else
                        {
                            GameObject.Destroy(mat);
                        }
                    }
                    render.materials = newArray;
                }
            }
        }
    }


    #region 状态设置
    public bool IsAlive()
    {
        return m_Alive;
    }

    // 是否开启碰撞检测
    public void SetColliderEnable(bool enabled)
    {
        if (m_ColliderEnable == enabled)
        {
            return;
        }
        for (int i = 0; i < m_Colliders.Length; i++)
        {
            m_Colliders[i].enabled = enabled;
        }
        m_ColliderEnable = enabled;
    }

    public bool GetColliderEnable()
    {
        return m_ColliderEnable;
    }


    public void ComingDead()
    {
        if (m_FishStatus == FishStatus.kFISH_STATUS_DEAD) return;
        EndFrozen();
        SetColliderEnable(false);
        m_FishStatus = FishStatus.kFISH_STATUS_DEAD;
        m_RouteAction?.Clear();
        m_TurnRed.StopAction();
    }

    public void TakeWalks(int takeWalkType, float duration)
    {
        ComingDead();
        m_FishStatus = FishStatus.kFISH_STATUS_TAKE_WALK;
        if (takeWalkType == 1)
        {
            m_TakeWalks.Play(duration, 0, true);
        }
        else
        {
            if (IsInScreen())
            {
                m_TakeWalks.Play(duration, 5.0f, false);
            }
            else
            {
                m_TakeWalks.Play(0.1f, 0, true);
            }
        }
    }
    #endregion

    #region Animator逻辑

    public void SyncAnimation(float t)
    {
        if (m_FishInfo != null && m_FishInfo.UseIdleR)
        {
            if (!m_Route.IsLeftToRight())
            {
                PlayAnimation("idle_R", t);
                return;
            }
        }
        
        for (int i = 0; i < m_Animators.Length; i++)
        {
            var animator = m_Animators[i];
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.length > 0)
            {
                float percent = (t % info.length) / info.length;
                //LogUtils.V($"{percent}  {info.length}");
                animator.Play(info.fullPathHash, -1, percent);
            }
        }
    }

    // 设置所有的速率
    public void SetAnimationSpeed(float speed)
    {
        if (m_CurrentAnimationSpeed == speed) return;
        m_CurrentAnimationSpeed = speed;
        if (m_FishAnimation != null)
        {
            m_FishAnimation.SetAnimationSpeed(speed);
        }
        else
        {
            for (int i = 0; i < m_Animators.Length; i++)
            {
                m_Animators[i].speed = speed;
            }
        }
    }

    // 直接切换动画
    public void PlayAnimation(string clipName, float time = 0)
    {
        if (m_FishAnimation != null)
        {
            m_FishAnimation.PlayAnimation(clipName, time);
        }
        else
        {
            for (int i = 0; i < m_Animators.Length; i++)
            {
                if (m_Animators[i].gameObject.activeSelf)
                {
                    float clipLength = GetClipLength(m_Animators[i], clipName);
                    float percent = 0;
                    if (clipLength > 0)
                    {
                        percent = Mathf.Clamp01(time / clipLength);
                    }
                    m_Animators[i].Play(clipName, -1, percent);
                }
            }
        }
    }

    ///获取动画状态机animator的动画clip的播放持续时长
    public static float GetClipLength(Animator animator, string clipName)
    {
        if (null == animator || null == animator.runtimeAnimatorController) return 0;
        var clips = animator.runtimeAnimatorController.animationClips;
        if (null == clips || clips.Length <= 0) return 0;
        AnimationClip clip;
        for (int i = 0, len = clips.Length; i < len; ++i)
        {
            clip = clips[i];
            if (null != clip && clip.name == clipName)
                return clip.length;
        }
        return 0f;
    }

    // 当前动画播放到的百分比
    public float GetAnimationTimeProportion(string aniName)
    {
        if (m_FishAnimation != null)
        {
            return 0;
        }
        else
        {
            float percent = -1;
            for (int i = 0; i < m_Animators.Length; i++)
            {
                var info = m_Animators[i].GetCurrentAnimatorStateInfo(0);
                if (info.IsName(aniName))
                {
                    percent = m_Animators[i].GetCurrentAnimatorStateInfo(0).normalizedTime;
                    break;
                }
            }
            return percent;
        }
    }

    public void SetTrigger(string name)
    {
        if (m_FishAnimation != null)
        {
            m_FishAnimation.PlayAnimation(name);
        }
        else
        {
            for (int i = 0; i < m_Animators.Length; i++)
            {
                m_Animators[i].SetTrigger(name);
            }
        }
    }

    public void SetInt(string name, int value)
    {
        if (m_FishAnimation != null)
        {
            m_FishAnimation.PlayAnimation(name);
        }
        else
        {
            for (int i = 0; i < m_Animators.Length; i++)
            {
                m_Animators[i].SetInteger(name, value);
            }
        }
    }

    public void SetFloat(string name, float value)
    {
        if (m_FishAnimation != null)
        {
            m_FishAnimation.PlayAnimation(name);
        }
        else
        {
            for (int i = 0; i < m_Animators.Length; i++)
            {
                m_Animators[i].SetFloat(name, value);
            }
        }
    }

    public void SetBool(string name, bool value)
    {
        if (m_FishAnimation != null)
        {
            m_FishAnimation.PlayAnimation(name);
        }
        else
        {
            for (int i = 0; i < m_Animators.Length; i++)
            {
                m_Animators[i].SetBool(name, value);
            }
        }
    }

    Animator GetMainAnimator()
    {
        if (m_Animators != null && m_Animators.Length > 0)
        {
            return m_Animators[0];
        }
        return null;
    }

    #endregion

    #region 冰冻

    public bool HasIceBlock()
    {
        return m_IceBlock != null;
    }

    public void InitIceBlock(GameObject ice)
    {
        if (ice == null) return;
        m_IceBlock = new XIceBlock();
        var render = ice.GetComponentInChildren<MeshRenderer>();
        var mat = render.material;
        m_IceBlock.AddGameObject(ice);
        m_IceBlock.AddMaterial(mat);
    }

    public void StartFrozen(float frozenTime)
    {
        m_IceBlock?.Start();
        m_FrozenTime = frozenTime;
        m_FishPlate?.SetNeedRotate(false);
        SetAnimationSpeed(0);
    }

    public bool IsFrozen()
    {
        return m_FrozenTime > 0;
    }

    public void EndFrozen()
    {
        m_FrozenTime = 0;
        m_FishPlate?.SetNeedRotate(true);
        SetAnimationSpeed(1);
        m_IceBlock?.Stop();
    }
    #endregion

    #region 受击变红
    public void TurnRed()
    {
        m_TurnRed.Play();
    }

    public void TurnWhite()
    {
        m_TurnRed.StopAction();
    }
    #endregion

    #region 锁定点

    // 检测锁定点在屏幕
    public bool IsInScreen()
    {
        if (m_FishInfo != null)
        {
            return m_FishInfo.IsInScreen();
        }
        else
        {
            return CameraUtils.IsWorldPointInScreen(transform.position);
        }
    }

    public Vector3 GetLockPoint()
    {
        if (m_FishInfo != null)
        {
            return m_FishInfo.GetLockPoint();
        }
        else
        {
            return transform.position;
        }
    }
    #endregion

    public void TestDisplay()
    {
        m_FishStatus = FishStatus.kFish_STATUS_TEST;
    }

    public void HideAllEffect()
    {
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].gameObject.SetActive(false);
        }
    }

    public void PlayFadeOut(float duration)
    {
        HideAllEffect();
        var com = gameObject.GetComponent<TweenModelAlpha>();
        if (com == null)
        {
            com = gameObject.AddComponent<TweenModelAlpha>();
        }
        com.duration = duration;
        com.from = 1;
        com.to = 0;
        com.Property = "_FishAlpha";
        com.ResetToBeginningAndPlayForward();
    }
}
