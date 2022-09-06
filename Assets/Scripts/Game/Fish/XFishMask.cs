using System;
using UnityEngine;
using System.Collections.Generic;

class XFishMask: MonoBehaviour
{

    private void Start()
    {
        UpdateMesh();
        var render = GetComponent<MeshRenderer>();
        render.sortingLayerID = SortingLayer.NameToID("Mask");
    }

    void UpdateMeshUV()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var mesh = meshFilter.mesh;
        var uv = mesh.uv;
        LogUtils.V(uv.Length);
        for (int i = 0; i < uv.Length; i++)
        {
            LogUtils.V($"{uv[i].x} {uv[i].y}");
        }
    }

    public void UpdateMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        Vector2Int gridSize = new Vector2Int(3, 3);

        float startX = -75;
        float startY = -75;
        float width = 50;
        float height = 50;

        float centerX = 5.12f;
        float centerY = 5.12f;

        //计算顶点及UV
        List<Vector3> vertices = new List<Vector3>()
        {
            new Vector3(startX, 0, startY), new Vector3(startX + width * 1, 0, startY), new Vector3(startX + width * 2, 0, startY), new Vector3(startX + width * 3, 0, startY),
            new Vector3(startX, 0, startY + height * 1), new Vector3(-centerX, 0, -centerY), new Vector3(centerX, 0, -centerY), new Vector3(startX + width * 3, 0, startY + height * 1),
            new Vector3(startX, 0, startY + height * 2), new Vector3(-centerX, 0, centerY), new Vector3(centerX, 0, centerY), new Vector3(startX + width * 3, 0, startY + height * 2),
            new Vector3(startX, 0, startY + height * 3), new Vector3(startX + width * 1, 0, startY + height * 3), new Vector3(startX + width * 2, 0, startY + height * 3), new Vector3(startX + width * 3, 0, startY + height * 3),

        };
        List<Vector2> uvs = new List<Vector2>()
        {
            new Vector2(0, 0), new Vector2(0.1f, 0), new Vector2(0.9f, 0), new Vector2(1, 0),
            new Vector2(0, 0.1f), new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.1f), new Vector2(1, 0.1f),
            new Vector2(0, 0.9f), new Vector2(0.1f, 0.9f), new Vector2(0.9f, 0.9f), new Vector2(1, 0.9f),
            new Vector2(0, 1), new Vector2(0.1f, 1), new Vector2(0.9f, 1), new Vector2(1, 1),
        };


        //顶点序列
        int a = 0;
        int b = 0;
        int c = 0;
        int d = 0;
        int startIndex = 0;
        int[] indexs = new int[gridSize.x * gridSize.y * 2 * 3];//顶点序列
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                //四边形四个顶点
                a = y * (gridSize.x + 1) + x;//0
                b = (y + 1) * (gridSize.x + 1) + x;//1
                c = b + 1;//2
                d = a + 1;//3

                //计算在数组中的起点序号
                startIndex = y * gridSize.x * 2 * 3 + x * 2 * 3;

                //左上三角形
                indexs[startIndex] = a;//0
                indexs[startIndex + 1] = b;//1
                indexs[startIndex + 2] = c;//2

                //右下三角形
                indexs[startIndex + 3] = c;//2
                indexs[startIndex + 4] = d;//3
                indexs[startIndex + 5] = a;//0
            }
        }

        //
        mesh.SetVertices(vertices);//设置顶点
        mesh.SetUVs(0, uvs);//设置UV
        mesh.SetIndices(indexs, MeshTopology.Triangles, 0);//设置顶点序列
        //mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        //mesh.RecalculateTangents();

        meshFilter.mesh = mesh;
    }
}
