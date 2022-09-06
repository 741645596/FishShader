using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public class NoRaycastTarget
{
    static NoRaycastTarget()
    {
        ObjectFactory.componentWasAdded += ComponentWasAdded;
    }

    private static void ComponentWasAdded(Component component)
    {
        Text text = component as Text;
        if (text != null)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/GameData/AppRes/Common/CommonFont/FZLTZCHJW.TTF");
            text.raycastTarget = false;
            text.font = font;
            text.fontSize = 24;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
        }
        string name = component.gameObject.name;
        if (name.StartsWith("Text"))
        {
            if (text != null)
            {
                text.raycastTarget = false;
            }
        }
        else if (name.StartsWith("Image"))
        {
            Image image = component as Image;
            if (image != null)
            {
                image.raycastTarget = false;
            }
        }
        else if (name.StartsWith("RawImage"))
        {
            RawImage rawImage = component as RawImage;
            if (rawImage != null)
            {
                rawImage.raycastTarget = false;
            }
        }
    }
}