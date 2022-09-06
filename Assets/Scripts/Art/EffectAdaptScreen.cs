using UnityEngine;
using System.Collections;

public class EffectAdaptScreen : MonoBehaviour
{
    public GameObject FX_U;
    public GameObject FX_D;
    public GameObject FX_L;
    public GameObject FX_R;

    private void Start()
    {
        var size = CameraUtils.GetWorldSize();
        float hw = size.x / 2;
        float hh = size.y / 2;

        if(FX_U != null) FX_U.transform.localPosition = new Vector3(0, hh, 0);
        if(FX_D != null) FX_D.transform.localPosition = new Vector3(0, -hh, 0);
        if(FX_L != null) FX_L.transform.localPosition = new Vector3(-hw, 0, 0);
        if(FX_R != null) FX_R.transform.localPosition = new Vector3(hw, 0, 0);
    }

    private void OnDestroy()
    {
    }

}
