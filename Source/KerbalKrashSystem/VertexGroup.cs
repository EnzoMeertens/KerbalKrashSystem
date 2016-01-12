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
        Dictionary<int, List<int>> vertexDictionary = new Dictionary<int, List<int>>();
        private int center;
        internal Vector3 Center(MeshFilter[] filterList)
        {
            MeshFilter centerF = filterList[centerFilter];
            return centerF.transform.TransformPoint(centerF.mesh.vertices[center]);
            
        }

        int centerFilter;

        public VertexGroup(int center, int centerFilter)
        {
            this.center = center;
            this.centerFilter = centerFilter;
        }

        internal void AddVertex(int meshFilter, int i)
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
        
        internal void Deform(MeshFilter[] filterList, Vector3 transform, float tolerance, Vector4 contactPoint, float DentDistance)
        {
            Vector3 worldCenter = Center(filterList);
            float distance = Vector3.Distance(worldCenter, contactPoint); //Get the distance from the vertex to the position of the krash.
            if (distance <= DentDistance)
            {

                foreach (int meshFilter in vertexDictionary.Keys.ToList())
                {
                    Vector3[] vertices = filterList[meshFilter].mesh.vertices;
                    foreach (int i in vertexDictionary[meshFilter])
                    {
                        Vector3 worldVertex = filterList[meshFilter].transform.TransformPoint(vertices[i]); //Transform the point of contact into the world reference frame.
                        
                        worldVertex += transform;

                        //Transform the vertex from the world's frame of reference to the local frame of reference and overwrite the existing vertex.
                        vertices[i] = filterList[meshFilter].transform.InverseTransformPoint(worldVertex);
                    }
                    filterList[meshFilter].mesh.vertices = vertices;
                }
            }
        }

        internal void Deform()
        {
            throw new NotImplementedException();
        }
    }
}
