using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextGradient : BaseMeshEffect
{
    [SerializeField]
    public Color32 topColor = Color.white;
    [SerializeField]
    public Color32 bottomColor = Color.black;

    private const int DefautlVertexNumPerFont = 6;

    List<UIVertex> vertexBuffers = new List<UIVertex>();
    public Mesh mesh = null;

    private void ModifyVertexColor(List<UIVertex> vertexList, int index, Color color)
    {
        UIVertex temp = vertexList[index];
        temp.color = color;
        vertexList[index] = temp;
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
        {
            return;
        }
        
        vh.GetUIVertexStream(vertexBuffers);
        //LogUtils.V(vertexBuffers.Count);

        if (mesh == null)
        {
            mesh = new Mesh();
        }
        vh.FillMesh(mesh);


        int count = vertexBuffers.Count;
        if (count > 0)
        {
            /** 给顶点着色(顶点的顺序图)            
            *   5-0 ---- 1
            *    | \    |
            *    |  \   |
            *    |   \  |
            *    |    \ |
            *    4-----3-2
            **/
            for (int i = 0; i < count; i += DefautlVertexNumPerFont)
            {
                ModifyVertexColor(vertexBuffers, i, topColor);
                ModifyVertexColor(vertexBuffers, i + 1, topColor);
                ModifyVertexColor(vertexBuffers, i + 2, bottomColor);
                ModifyVertexColor(vertexBuffers, i + 3, bottomColor);
                ModifyVertexColor(vertexBuffers, i + 4, bottomColor);
                ModifyVertexColor(vertexBuffers, i + 5, topColor);
            }
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertexBuffers);
    }
}