using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModifyFishMaterial : MonoBehaviour
{
    public string SrcProperty = "_SourceBlend";
    public float ScrValue = 5;
    public string DestProperty = "_DestBlend";
    public float DestValue = 10;
    public int renderQueue = 3000;

    private List<Material> mMaterials;
    private int mSrcID;
    private int mDestID;
    private int mZwriteID;

    private void Start()
    {
        mSrcID = Shader.PropertyToID(SrcProperty);
        mDestID = Shader.PropertyToID(DestProperty);
        mZwriteID = Shader.PropertyToID("_ZWriteMode");
        var meshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

        mMaterials = new List<Material>();
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            SkinnedMeshRenderer render = meshRenderers[i];
            int length = render.materials.Length;
            if (length == 0) continue;
            for (int j = 0; j < length; j++)
            {
                var mat = render.materials[j];
                mat.renderQueue = renderQueue;
                mMaterials.Add(mat);
            }
        }

        SetMaterialProperty();
    }

    private void SetMaterialProperty()
    {
        var list = mMaterials;
        for (int i = 0; i < list.Count; i++)
        {
            var mat = list[i];

            if (mat.HasProperty(mSrcID))
            {
                mat.SetFloat(mSrcID, ScrValue);
            }

            if (mat.HasProperty(mDestID))
            {
                mat.SetFloat(mDestID, DestValue);
            }

            //if (mat.HasProperty(mZwriteID))
            //{
            //    mat.SetFloat(mZwriteID, 0);
            //}
        }
    }

    private void OnDestroy()
    {
        var list = mMaterials;
        for (int i = 0; i < list.Count; i++)
        {
            GameObject.Destroy(list[i]);
        }
    }
}
