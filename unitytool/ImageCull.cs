using System.Collections.Generic;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Effects/ImageCull", 16)]
    [RequireComponent(typeof(Image))]
    public class ImageCull : BaseMeshEffect
    {
        protected ImageCull()
        { }

        public override void ModifyMesh(VertexHelper vh)
        {
//            Image image = GetComponent<Image>();
//            if (image)
//            {
//                image.canvasRenderer.cull = true;
//            }
            vh.Clear();
        }
    }
}
