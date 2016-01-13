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
        private Vector3 centerV;
        internal Vector3 Center(MeshFilter[] filterList)
        {
            MeshFilter centerF = filterList[centerFilter];
            return centerF.transform.TransformPoint(centerF.mesh.vertices[center]);
        }
        internal Vector3 Center()
        {
            return centerV;
        }

        int centerFilter;

        public VertexGroup(int meshFilter, int i, Vector3 centerV)
        {
            center = i;
            centerFilter = meshFilter;
            this.centerV = centerV;
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
        
        internal void Deform(MeshFilter[] filterList, Vector3[] transform, Vector4 contactPoint, float DentDistance)
        {
            Vector3 worldCenter = Center(filterList);
            float distance = Vector3.Distance(worldCenter, contactPoint); //Get the distance from the vertex to the position of the krash.
            if (distance <= DentDistance)
            {
                foreach (int meshFilter in vertexDictionary.Keys.ToList())
                {
                    Mesh mesh = filterList[meshFilter].mesh;
                    if(mesh == null)
                    {
                        mesh = filterList[meshFilter].sharedMesh;
                    }
                    Vector3[] vertices = mesh.vertices;
                    foreach (int i in vertexDictionary[meshFilter])
                    {
                        vertices[i] += transform[meshFilter];
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
