using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("UI/Effects/Gradient")]
public class Gradient: BaseMeshEffect
{
    [SerializeField]
    private Color32 topColor = Color.white;

    [SerializeField]
    private Color32 bottomColor = Color.black;


    List<UIVertex> vertexs = new List<UIVertex>();

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        int count = vh.currentVertCount;
        if (count == 0) return;

        vertexs.Clear();
        //获取顶点
        for (int idx =0; idx < count;idx++)
        {
            var vertex = new UIVertex();
            vh.PopulateUIVertex(ref vertex, idx);
            vertexs.Add(vertex);
        }

        //计算文本的最高点以及最低点
        float bottomY = vertexs[0].position.y;
        float topY = vertexs[0].position.y;

        for (int idx = 1; idx < count; idx++)
        {
            float y = vertexs[idx].position.y;

            if (y > topY)
                topY = y;
            else if (y < bottomY)
                bottomY = y;
        }

        //修改顶点颜色
        float height = topY - bottomY;
        for (int idx = 0; idx < count; idx++)
        {
            UIVertex vertex = vertexs[idx];
            var clr = Color32.Lerp(bottomColor, topColor, (vertex.position.y - bottomY) / height);
            vertex.color = clr;
            vh.SetUIVertex(vertex, idx);
        }
    }
}