using UnityEngine;

namespace KKS
{
    public static class Debug_Visual_Point
    {
        public static GameObject CreateSphere(Transform parent, Vector3 localPosition, Color color)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            temp.transform.parent = parent;
            temp.transform.localScale = Vector3.one * 0.25f;
            temp.transform.localPosition = localPosition;
            temp.collider.isTrigger = true;
            temp.collider.enabled = false;
            temp.renderer.material.color = color;
            return temp;
        }

        public static GameObject CreateSphere(Vector3 worldPosition, Color color)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            temp.transform.localScale = Vector3.one * 0.25f;
            temp.transform.position = worldPosition;
            temp.collider.isTrigger = true;
            temp.collider.enabled = false;
            temp.renderer.material.color = color;
            return temp;
        }
    }
}
