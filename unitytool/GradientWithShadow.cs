using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("UI/Effects/GradientWithShadow")]
public class GradientWithShadow : BaseMeshEffect
{
    [SerializeField]
    private Color32 topColor = Color.white;

    [SerializeField]
    private Color32 bottomColor = Color.black;

    [SerializeField]
    private Color m_EffectColor = new Color(0f, 0f, 0f, 0.5f);

    [SerializeField]
    private Vector2 m_EffectDistance = new Vector2(1f, -1f);

    [SerializeField]
    private bool m_UseGraphicAlpha = true;

    private const float kMaxEffectDistance = 600f;


    List<UIVertex> vertexs = new List<UIVertex>();



#if UNITY_EDITOR
    protected override void OnValidate()
    {
        effectDistance = m_EffectDistance;
        base.OnValidate();
    }
#endif

    public Color effectColor
    {
        get { return m_EffectColor; }
        set
        {
            m_EffectColor = value;
            if (graphic != null)
                graphic.SetVerticesDirty();
        }
    }

    public Vector2 effectDistance
    {
        get { return m_EffectDistance; }
        set
        {
            if (value.x > kMaxEffectDistance)
                value.x = kMaxEffectDistance;
            if (value.x < -kMaxEffectDistance)
                value.x = -kMaxEffectDistance;

            if (value.y > kMaxEffectDistance)
                value.y = kMaxEffectDistance;
            if (value.y < -kMaxEffectDistance)
                value.y = -kMaxEffectDistance;

            if (m_EffectDistance == value)
                return;

            m_EffectDistance = value;

            if (graphic != null)
                graphic.SetVerticesDirty();
        }
    }

    public bool useGraphicAlpha
    {
        get { return m_UseGraphicAlpha; }
        set
        {
            m_UseGraphicAlpha = value;
            if (graphic != null)
                graphic.SetVerticesDirty();
        }
    }

    public void SetTopAndBottomColor(Color color) {
        this.topColor = color;
        this.bottomColor = color;
		if (graphic != null)
			graphic.SetVerticesDirty();
    }
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        int count = vh.currentVertCount;
        if (count == 0) return;

        //获取顶点
        for (int idx = 0; idx < count; idx++)
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

        vertexs.Clear();
        vh.GetUIVertexStream(vertexs);
        ApplyShadowZeroAlloc(vertexs, effectColor, 0, vertexs.Count, effectDistance.x, effectDistance.y);
        vh.Clear();
        vh.AddUIVertexTriangleStream(vertexs);
        vertexs.Clear();
    }

    protected void ApplyShadowZeroAlloc(List<UIVertex> verts, Color32 color, int start, int end, float x, float y)
    {
        UIVertex vt;

        var neededCapacity = verts.Count + end - start;
        if (verts.Capacity < neededCapacity)
            verts.Capacity = neededCapacity;

        for (int i = start; i < end; ++i)
        {
            vt = verts[i];
            verts.Add(vt);

            Vector3 v = vt.position;
            v.x += x;
            v.y += y;
            vt.position = v;
            var newColor = color;
            if (m_UseGraphicAlpha)
                newColor.a = (byte)((newColor.a * verts[i].color.a) / 255);
            vt.color = newColor;
            verts[i] = vt;
        }
    }
}