using UnityEngine;

namespace KerbalKrashSystem_Leak
{
    class VisualDebugLine : MonoBehaviour
    {
        private float _length = 1.0f;
        public LineRenderer line = null;
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
            line.startColor = start;
            line.endColor = end;
            line.startWidth = 0.1f;
            line.endWidth = 0.05f;
            line.positionCount = 2;
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
            line.startColor = start;
            line.endColor = end;
            line.startWidth = 0.1f;
            line.endWidth = 0.05f;
            line.positionCount = 2;
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
            line.startColor = start;
            line.endColor = end;
            line.startWidth = 0.1f;
            line.endWidth = 0.05f;
            line.positionCount = 2;
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
            line.transform.localRotation = Quaternion.LookRotation(point);
            return line.transform.localRotation;
        }

        public Quaternion LookAt(Vector3 point, Vector3 up)
        {
            line.transform.localRotation = Quaternion.LookRotation(point, up);
            return line.transform.localRotation;
        }
    }
}
