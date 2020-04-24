using UnityEngine;

namespace KKS
{
    public static class Debug_Visual_Point
    {
        public static GameObject CreateSphere(Transform parent, Vector3 localPosition, Color color)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Collider collider = temp.GetComponent<Collider>();

            temp.transform.parent = parent;
            temp.transform.localScale = Vector3.one * 0.25f;
            temp.transform.localPosition = localPosition;

            collider.isTrigger = true;
            collider.enabled = false;

            temp.GetComponent<Renderer>().material.color = color;

            return temp;
        }

        public static GameObject CreateSphere(Vector3 worldPosition, Color color)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Collider collider = temp.GetComponent<Collider>();

            temp.transform.localScale = Vector3.one * 0.25f;
            temp.transform.position = worldPosition;

            collider.isTrigger = true;
            collider.enabled = false;

            temp.GetComponent<Renderer>().material.color = color;

            return temp;
        }
    }
}
