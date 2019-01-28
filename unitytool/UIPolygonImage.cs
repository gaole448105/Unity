using System.Collections.Generic;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Effects/UIPolygonImage", 16)]
    [RequireComponent(typeof(Image))]
    public class UIPolygonImage : BaseMeshEffect
    {
        protected UIPolygonImage()
        { }

        public override void ModifyMesh(VertexHelper vh)
        {
            Image image = GetComponent<Image>();
            if (image.type != Image.Type.Simple)
            {
                return;
            }

            Sprite sprite = image.overrideSprite;
            if (sprite == null || sprite.triangles.Length == 6)
            {
                return;
            }

            // Kanglai: at first I copy codes from Image.GetDrawingDimensions
            // to calculate Image's dimensions. But now for easy to read, I just take usage of corners.

            if (vh.currentVertCount != 4)
            {
                return;
            }
            UIVertex vertice = new UIVertex();
            vh.PopulateUIVertex(ref vertice, 0);
            Vector2 lb = vertice.position;
            vh.PopulateUIVertex(ref vertice, 2);
            Vector2 rt = vertice.position;

            // Kanglai: recalculate vertices from Sprite!
            int len = sprite.vertices.Length;
            var vertices = new List<UIVertex>(len);
            Vector2 Center = sprite.bounds.center;
            Vector2 invExtend = new Vector2(1 / sprite.bounds.size.x, 1 / sprite.bounds.size.y);
            for (int i = 0; i < len; i++)
            {
                vertice = new UIVertex();
                // normalize
                float x = (sprite.vertices[i].x - Center.x) * invExtend.x + 0.5f;
                float y = (sprite.vertices[i].y - Center.y) * invExtend.y + 0.5f;
                // lerp to position
                vertice.position = new Vector2(Mathf.Lerp(lb.x, rt.x, x), Mathf.Lerp(lb.y, rt.y, y));
                vertice.color = image.color;
                vertice.uv0 = sprite.uv[i];
                vertices.Add(vertice);
            }

            len = sprite.triangles.Length;

            var triangles = new List<int>(len);
            for (int i = 0; i < len; i++)
            {
                triangles.Add(sprite.triangles[i]);
            }

            vh.Clear();
            vh.AddUIVertexStream(vertices, triangles);
        }
    }
}