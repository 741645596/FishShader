using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllFishDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    public float MaxDistance = 200;
    public Transform FishRoot;
    void Start()
    {
        var slider = transform.Find("Slider").GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnValueChanged);
        FishRoot.position = new Vector3(0, 0, 0);
    }

    void OnValueChanged(float percent)
    {
        float x = -MaxDistance * percent;
        if (FishRoot != null)
        {
            FishRoot.position = new Vector3(x, 0, 0);
        }
    }
}
