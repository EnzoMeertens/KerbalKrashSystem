using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// http://answers.unity3d.com/questions/259127/does-anyone-have-any-code-to-subdivide-a-mesh-and.html
/// </summary>
public static class MeshHelper
{
    static List<Vector3> vertices;
    static List<Vector3> normals;
    static List<Color> colors;
    static List<Vector2> uv;
    static List<Vector2> uv2;

    static List<int> indices;

    //List of all added vertices.
    static Dictionary<uint, int> newVectices;

    /// <summary>
    /// Initializes help variables.
    /// </summary>
    /// <param name="mesh"></param>
    private static void Initialize(Mesh mesh)
    {
        vertices = new List<Vector3>(mesh.vertices);
        normals = new List<Vector3>(mesh.normals);
        colors = new List<Color>(mesh.colors);
        uv = new List<Vector2>(mesh.uv);
        uv2 = new List<Vector2>(mesh.uv2);
        indices = new List<int>();
    }

    /// <summary>
    /// Unloads all used resources from memory, ready to be collected by the GC.
    /// </summary>
    private static void Destroy()
    {
        vertices = null;
        normals = null;
        colors = null;
        uv = null;
        uv2 = null;
        indices = null;
    }

    private static int GetNewVertex(int i1, int i2, Vector3 hitpoint, float distance)
    {
        int newIndex = vertices.Count;

        //Why does this work?
        /* Well ;) That's simply because for each edge (two indices) we need to add a new vertex. 
         * However an adjacent triangle (which shares the same edge) need to find the 
         * correct new generated vertex for this particular edge. Since the edge would be 
         * reverted for an adjacent triangle triangle i have to test both cases A--B and B--A. 
         * As lookup table i used a dictionary to lookup a certain edge. Since the edge can be 
         * inverted I test for both veriants.
         * Do you have a simpler way to match a new index to an edge (two indices pair)?
         */
        uint t1 = ((uint)i1 << 16) | (uint)i2;
        uint t2 = ((uint)i2 << 16) | (uint)i1;

        //Already added(?)
        if (newVectices.ContainsKey(t2))
            return newVectices[t2];

        //Already added(?)
        if (newVectices.ContainsKey(t1))
            return newVectices[t1];

        //Add new vertex to "new vertices" array.
        newVectices.Add(t1, newIndex);

        //Add new vertex half way between existing vertices.
        vertices.Add((vertices[i1] + vertices[i2]) * 0.5f);

        if (normals.Count > 0)
            normals.Add((normals[i1] + normals[i2]).normalized);

        if (colors.Count > 0)
            colors.Add((colors[i1] + colors[i2]) * 0.5f);

        if (uv.Count > 0)
            uv.Add((uv[i1] + uv[i2]) * 0.5f);

        if (uv2.Count > 0)
            uv2.Add((uv2[i1] + uv2[i2]) * 0.5f);

        return newIndex;
    }

    /// <summary>
    /// Divides each triangle into 4. A quad (2 tris) will be split into 2x2 quads (8 tris)
    /// </summary>
    /// <param name="mesh">The mesh to subdivide.</param>
    public static void Subdivide(Mesh mesh, Vector3 hitpoint, float distance)
    {
        newVectices = new Dictionary<uint, int>();

        Initialize(mesh);

        int[] triangles = mesh.triangles; //TODO: Only use the triangles closest to the dent...?

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i1 = triangles[i + 0];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            int a = GetNewVertex(i1, i2, hitpoint, distance);
            int b = GetNewVertex(i2, i3, hitpoint, distance);
            int c = GetNewVertex(i3, i1, hitpoint, distance);

            indices.Add(i1); indices.Add(a); indices.Add(c);
            indices.Add(i2); indices.Add(b); indices.Add(a);
            indices.Add(i3); indices.Add(c); indices.Add(b);
            indices.Add(a); indices.Add(b); indices.Add(c); //Center triangle.
        }

        mesh.vertices = vertices.ToArray();

        if (normals.Count > 0)
            mesh.normals = normals.ToArray();

        if (colors.Count > 0)
            mesh.colors = colors.ToArray();

        if (uv.Count > 0)
            mesh.uv = uv.ToArray();

        if (uv2.Count > 0)
            mesh.uv2 = uv2.ToArray();

        mesh.triangles = indices.ToArray();

        Destroy();
    }
}