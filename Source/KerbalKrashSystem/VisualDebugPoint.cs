using UnityEngine;

namespace KKS
{
    class VisualDebugPoint : MonoBehaviour
    {
        private float _diameter = 1.0f;
        private MeshRenderer point = null;
        public float Duration = 10.0f;
        private GameObject obj;

        void Update()
        {
            Duration -= Time.deltaTime;

            if (Duration < 0)
            {
                point.enabled = false;
                point = null;
                obj.DestroyGameObject();
                Debug.Log("Destroyed Point");
            }
        }

        public VisualDebugPoint(Part part, Color color)
        {
            // First of all, create a GameObject to which LineRenderer will be attached.
            obj = new GameObject("Point");

            // Then create renderer itself...
            point = obj.AddComponent<MeshRenderer>();
            point.transform.parent = part.transform; // ...child to our part...
            //point.useWorldSpace = false; // ...and moving along with it (rather 
            // than staying in fixed world coordinates)
            point.transform.localPosition = Vector3.zero;
            point.transform.localEulerAngles = Vector3.zero;
            point.sortingOrder = (-1);

            // Make it render a red to yellow triangle, 1 meter wide and 2 meters long
            point.material = new Material(Shader.Find("Particles/Additive"));

            point.material.color = color;
            point.transform.localScale = new Vector3(10,10,10);
        }

        public VisualDebugPoint(GameObject part, Color color)
        {
            // First of all, create a GameObject to which LineRenderer will be attached.
            obj = new GameObject("Point");

            // Then create renderer itself...
            point = obj.AddComponent<MeshRenderer>();
            point.transform.parent = part.transform; // ...child to our part...
            //point.useWorldSpace = false; // ...and moving along with it (rather 
            // than staying in fixed world coordinates)
            point.transform.localPosition = Vector3.zero;
            point.transform.localEulerAngles = Vector3.zero;
            point.sortingOrder = (-1);

            // Make it render a red to yellow triangle, 1 meter wide and 2 meters long
            point.material = new Material(Shader.Find("Particles/Additive"));

            point.material.color = color;
            point.transform.localScale = new Vector3(10, 10, 10);
        }

        public VisualDebugPoint(Vector3 part, Color color)
        {
           // First of all, create a GameObject to which LineRenderer will be attached.
            obj = new GameObject("Point");

            // Then create renderer itself...
            point = obj.AddComponent<MeshRenderer>();
            point.transform.position = part; // ...child to our part...
            //point.useWorldSpace = false; // ...and moving along with it (rather 
            // than staying in fixed world coordinates)
            point.transform.localPosition = Vector3.zero;
            point.transform.localEulerAngles = Vector3.zero;
            point.sortingOrder = (-1);

            // Make it render a red to yellow triangle, 1 meter wide and 2 meters long
            point.material = new Material(Shader.Find("Particles/Additive"));

            point.material.color = color;
            point.transform.localScale = new Vector3(10, 10, 10);
        }

        public float Diameter
        {
            get { return _diameter; }
            set
            {
                _diameter = value;
                point.transform.localScale = Vector3.one * _diameter;
            }
        }
    }
}
