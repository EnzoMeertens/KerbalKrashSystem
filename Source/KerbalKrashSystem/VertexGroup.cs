using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalKrashSystem
{
    class VertexGroup
    {
        Dictionary<MeshFilter, List<int>> vertexDictionary = new Dictionary<MeshFilter, List<int>>();
        private int center;
        internal Vector3 Center
        {
            get
            {
                return centerFilter.transform.TransformPoint(centerFilter.mesh.vertices[center]);
            }
        }

        MeshFilter centerFilter;

        public VertexGroup(int center, MeshFilter centerFilter)
        {
            this.center = center;
            this.centerFilter = centerFilter;
        }

        internal void AddVertex(MeshFilter meshFilter, int i)
        {
            List<int> indexList;
            if (vertexDictionary.ContainsKey(meshFilter))
            {
                indexList = vertexDictionary[meshFilter];
            }
            else
            {
                indexList = new List<int>();
                vertexDictionary[meshFilter] = indexList;
            }
            indexList.Add(i);
        }

        internal void Deform(Vector3 rangeMin, Vector3 rangeMax, float tolerance, Vector4 contactPoint, float DentDistance)
        {
            Vector3 worldCenter = Center;
            float distance = Vector3.Distance(worldCenter, contactPoint); //Get the distance from the vertex to the position of the krash.
            if (distance <= DentDistance)
            {
                Vector3 transform;

                transform.x = UnityEngine.Random.Range(rangeMin.x, rangeMax.x) / tolerance;
                transform.y = UnityEngine.Random.Range(rangeMin.y, rangeMax.y) / tolerance;
                transform.z = UnityEngine.Random.Range(rangeMin.z, rangeMax.z) / tolerance;

                foreach (MeshFilter meshFilter in vertexDictionary.Keys.ToList())
                {
                    Vector3[] vertices = meshFilter.mesh.vertices;
                    foreach (int i in vertexDictionary[meshFilter])
                    {
                        Vector3 worldVertex = meshFilter.transform.TransformPoint(vertices[i]); //Transform the point of contact into the world reference frame.
                        
                        worldVertex += transform;

                        //Transform the vertex from the world's frame of reference to the local frame of reference and overwrite the existing vertex.
                        vertices[i] = meshFilter.transform.InverseTransformPoint(worldVertex);
                    }
                    meshFilter.mesh.vertices = vertices;
                }
            }
        }

        internal void Deform()
        {
            throw new NotImplementedException();
        }
    }
}
