using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ClippableParticle : MonoBehaviour {
    public GameObject _maskObject;

    void Awake() {
        //Debug.Log("ClippableParticle.Awake() was called");
        initMaskObj();
    }

    private void OnEnable() {
        //Debug.Log("ClippableParticle.OnEnable() was called");
        initMaskObj();
    }

    void Start() {
        //Debug.Log("ClippableParticle.Start() was called");
        initMaskObj();
    }

    private List<Material> _materials = new List<Material>();

    public void initMaskObj() {
        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>(true);

        List<Renderer> _renderers = new List<Renderer>();
        _materials.Clear();
        for (int i = 0; i < particleSystems.Length; i++) {
            var ps = particleSystems[i];
            var rnd = ps.GetComponent<Renderer>();

            if (rnd == null)
                continue;

            _renderers.Add(rnd);
            if (rnd.material.HasProperty(Shader.PropertyToID("_ClipRect")))
                _materials.Add(rnd.material);
        }

        GameObject rectObj = GetComponentInParent<RectMask2D>()?.gameObject;
        _maskObject = rectObj ? rectObj : GetComponentInParent<Mask>()?.gameObject;

        if (_materials.Count > 0) {
            // ScrollView位置变化时重新计算裁剪区域
            listenParentScroll();

            SetClip();
        }
    }

    public void listenParentScroll() {
        var componentInParent = GetComponentInParent<ScrollRect>();
        if (componentInParent)
            componentInParent.onValueChanged.AddListener((e) => { SetClip(); });
    }

    private Vector3[] _maskWorldCorner = new Vector3[4];
    private Vector4 _clipRect;

    private void updateClipRect() {
        if (!_maskObject)
            return;

        _maskObject.GetComponent<RectTransform>().GetWorldCorners(_maskWorldCorner); // 计算world space中的点坐标
        if (_clipRect == null)
            _clipRect = new Vector4();

        _clipRect.Set( // 选取左下角和右上角
            _maskWorldCorner[0].x, _maskWorldCorner[0].y,
            _maskWorldCorner[2].x, _maskWorldCorner[2].y
        );
    }

    public void SetClip() {
        updateClipRect();
        if (_clipRect == null)
            return;
        for (int i = 0; i < _materials.Count; i++) {
            var material = _materials[i];

            material.EnableKeyword("UNITY_UI_CLIP_RECT");
            material.SetVector("_ClipRect", _clipRect); // 设置裁剪区域
        }

        //mt.EnableKeyword("UNITY_UI_CLIP_RECT");
        //mt.SetVector("_ClipRect", _clipRect); // 设置裁剪区域
        //// mt.SetFloat("_UseClipRect", 1.0f); // 开启裁剪
    }
}