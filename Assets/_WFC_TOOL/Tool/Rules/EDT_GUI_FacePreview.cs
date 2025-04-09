using UnityEngine;

namespace PCG_Tool
{

    public class EDT_GUI_FacePreview
    {
        public Vector3 position;
        public Quaternion rotation;
        public short ownerId;
        public FaceDirection dir;

        public EDT_GUI_FacePreview(Vector3 position, Vector3 normal, short ownerId, FaceDirection dir)
        {
            this.position = position;
            this.rotation = Quaternion.LookRotation(normal);
            this.ownerId = ownerId;
            this.dir = dir;
        }

        public static Vector3 GetScaleForNormal(Vector3 normal, Vector3 tileSize)
        {
            normal = normal.normalized;

            if (normal == Vector3.up || normal == Vector3.down)
                return new Vector3(tileSize.x, tileSize.z, 1); // top/bottom -> XZ
            if (normal == Vector3.left || normal == Vector3.right)
                return new Vector3(tileSize.z, tileSize.y, 1); // left/right -> ZY
            if (normal == Vector3.forward || normal == Vector3.back)
                return new Vector3(tileSize.x, tileSize.y, 1); // front/back -> XY

            return Vector3.one;
        }

        public bool IsHitByRay(Ray ray, Vector3 tileSize, out float distance)
        {
            distance = float.MaxValue;

            Plane plane = new Plane(rotation * Vector3.forward, position);
            if (plane.Raycast(ray, out float d))
            {
                Vector3 hit = ray.GetPoint(d);
                Vector3 local = Quaternion.Inverse(rotation) * (hit - position);

                Vector3 sizeLocal = Quaternion.Inverse(rotation) * tileSize;
                float halfX = Mathf.Abs(sizeLocal.x) * 0.5f;
                float halfY = Mathf.Abs(sizeLocal.y) * 0.5f;

                if(Mathf.Abs(local.x) <= halfX && Mathf.Abs(local.y) <= halfY)
                {
                    distance = d;
                    return true;
                }
            }
            return false;
        }
    }
}