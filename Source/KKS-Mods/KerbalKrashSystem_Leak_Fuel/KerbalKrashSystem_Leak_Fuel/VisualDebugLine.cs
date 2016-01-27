using UnityEngine;

namespace KerbalKrashSystem_Leak
{
    class VisualDebugLine : MonoBehaviour
    {
        private float _length = 1.0f;
        private LineRenderer line = null;
        public float Duration = 10.0f;
        private GameObject obj;

        public VisualDebugLine(Part part, Color start, Color end)
        {
            // First of all, create a GameObject to which LineRenderer will be attached.
            obj = new GameObject("Line");

            // Then create renderer itself...
            line = obj.AddComponent<LineRenderer>();
            line.transform.parent = part.transform; // ...child to our part...
            line.useWorldSpace = false; // ...and moving along with it (rather 
            // than staying in fixed world coordinates)
            line.transform.localPosition = Vector3.zero;
            line.transform.localEulerAngles = Vector3.zero;
            line.sortingOrder = (-1);

            // Make it render a red to yellow triangle, 1 meter wide and 2 meters long
            line.material = new Material(Shader.Find("Particles/Additive"));
            line.SetColors(start, end);
            line.SetWidth(0.5f, 0);
            line.SetVertexCount(2);
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, Vector3.forward * 2);
        }

        public VisualDebugLine(GameObject part, Color start, Color end)
        {
            // First of all, create a GameObject to which LineRenderer will be attached.
            obj = new GameObject("Line");

            // Then create renderer itself...
            line = obj.AddComponent<LineRenderer>();
            line.transform.parent = part.transform; // ...child to our part...
            line.useWorldSpace = false; // ...and moving along with it (rather 
            // than staying in fixed world coordinates)
            line.transform.localPosition = Vector3.zero;
            line.transform.localEulerAngles = Vector3.zero;
            line.sortingOrder = (-1);

            // Make it render a red to yellow triangle, 1 meter wide and 2 meters long
            line.material = new Material(Shader.Find("Particles/Additive"));
            line.SetColors(start, end);
            line.SetWidth(0.5f, 0);
            line.SetVertexCount(2);
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, Vector3.forward * 2);
        }

        public VisualDebugLine(Vector3 part, Color start, Color end)
        {
            // First of all, create a GameObject to which LineRenderer will be attached.
            obj = new GameObject("Line");

            // Then create renderer itself...
            line = obj.AddComponent<LineRenderer>();
            //line.transform.parent = part.transform; // ...child to our part...
            line.useWorldSpace = false; // ...and moving along with it (rather 
            // than staying in fixed world coordinates)
            line.transform.position = part;
            line.transform.localEulerAngles = Vector3.zero;
            line.sortingOrder = (-1);

            // Make it render a red to yellow triangle, 1 meter wide and 2 meters long
            line.material = new Material(Shader.Find("Particles/Additive"));
            line.SetColors(start, end);
            line.SetWidth(0.5f, 0);
            line.SetVertexCount(2);
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, Vector3.zero);
        }

        public float Length
        {
            get { return _length; }
            set
            {
                _length = value;
                line.SetPosition(1, Vector3.forward * _length);
            }
        }

        public Quaternion LookAt(Vector3 point)
        {
            line.transform.rotation = Quaternion.LookRotation(point);
            return line.transform.rotation;
        }
    }
}
