using System;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Tween/Tween Animation")]
public class TweenAnimation : MonoBehaviour
{
    public Sprite[] sprites;
    public float interval;
    private float mTime;
    private Image mImage;
    private int mIndex;
    

    Image cachedImage
    {
        get
        {
            if (mImage == null)
            {
                mImage = GetComponent<Image>();
            }
            return mImage;
        }
    }

    void Update()
    {
        mTime += Time.deltaTime;
        if (mTime > interval)
        {
            mTime = 0;
            mIndex++;
            if (mIndex >= sprites.Length)
            {
                mIndex = 0;
            }
            cachedImage.sprite = sprites[mIndex];
        }
    }
}
