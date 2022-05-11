using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("CustomVolume/CustomRender")]
public sealed class CustomVolume1 : VolumeComponent, IPostProcessComponent
{
    [Tooltip("�Ƿ���Ч��")]
    public BoolParameter enableEffect = new BoolParameter(true);
    public bool IsActive() => enableEffect == true;
    public bool IsTileCompatible() => false;
}
