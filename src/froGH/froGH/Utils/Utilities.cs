using Rhino;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace froGH.Utils
{
    public static class Utilities
    {
        //LUTs for sin and cos operations
        static double[] cos = new double[]{
        1, 0.980785, 0.92388, 0.83147, 0.707107, 0.55557, 0.382683, 0.19509,
        0, -0.19509, -0.382683, -0.55557, -0.707107, -0.83147, -0.92388, -0.980785,
        -1, -0.980785, -0.92388, -0.83147, -0.707107, -0.55557, -0.382683, -0.19509,
         0,  0.19509, 0.382683, 0.55557, 0.707107, 0.83147, 0.92388, 0.980785, 1};

        static double[] sin = new double[] {
        0, 0.19509, 0.382683, 0.55557, 0.707107, 0.83147, 0.92388, 0.980785,
        1, 0.980785, 0.92388, 0.83147, 0.707107, 0.55557, 0.382683, 0.19509,
        0, -0.19509, -0.382683, -0.55557, -0.707107, -0.83147, -0.92388, -0.980785,
        -1, -0.980785, -0.92388, -0.83147, -0.707107, -0.55557, -0.382683, -0.19509, 0};
        public static void GetVertexNeighbours(Mesh M, int vertIndex, out int[] vertices, out int[] edges, out int[] faces)
        {
            vertices = null;
            edges = null;
            faces = null;
            //topology structure groups coincident vertices into a single "TopologyVertex"
            // topology vertex index
            int tvIndex = M.TopologyVertices.TopologyVertexIndex(vertIndex);

            // find connected edges in radial order
            // SortEdges puts edges in radial order
            M.TopologyVertices.SortEdges(tvIndex);
            edges = M.TopologyVertices.ConnectedEdges(tvIndex);
            if (edges == null || edges.Length == 0) return;

            // find neighbour vertices in the edges order
            vertices = new int[edges.Length];
            for (int j = 0; j < edges.Length; j++)
            {
                IndexPair edgeTips = M.TopologyEdges.GetTopologyVertices(edges[j]);
                vertices[j] = edgeTips.I == tvIndex ? edgeTips.J : edgeTips.I;
            }


            //
            //
            // this part should be rewritten to consider naked edges cases (right now it's not working as it should)
            //

            faces = null;


            // look for faces only if there is more than 1 edge
            if (edges.Length > 1)
            {
                // find neighbour faces in edges order
                // try this: pick first face index as face shared from first and second edge
                // for the following edges pick the index (of the two) that is different from the previous iteration
                faces = new int[edges.Length];
                List<int> facesList = new List<int>();


                // first edge/face index
                int[] cFaces;
                int faceCount = 0;
                // find connected faces indexes to first edge
                cFaces = M.TopologyEdges.GetConnectedFaces(edges[0]);
                if (cFaces.Length != 0)
                {
                    for (int j = 0; j < cFaces.Length; j++)
                    {
                        // find edges for j-th connected face
                        int[] fEdges = M.TopologyEdges.GetEdgesForFace(cFaces[j]);
                        // if face contains also index of second edge then we found our first face
                        if (fEdges.Contains(edges[1]))
                        {
                            facesList.Add(cFaces[j]);
                            faceCount = 1;
                            //faces[0] = cFaces[j];
                            break;
                        }
                    }
                }
                // all other edges
                for (int j = 1; j < edges.Length; j++)
                {
                    cFaces = M.TopologyEdges.GetConnectedFaces(edges[j]);
                    // if it's a clothed edge
                    if (cFaces.Length > 1)
                    {
                        facesList.Add(cFaces[0] != facesList[faceCount - 1] ? cFaces[0] : cFaces[1]);
                        faceCount++;
                    }
                    // else, if naked...
                    else if (cFaces.Length > 0 && cFaces[0] != facesList[faceCount - 1])
                    {
                        facesList.Add(cFaces[0]);
                        faceCount++;
                    }
                }

                faces = facesList.ToArray();
            }
        }

        public static Mesh MakeSphereMesh(Point3d P, double R, int step)
        {
            // build vertices
            Mesh S = new Mesh();
            S.Vertices.Add(new Point3d(P.X, P.Y, P.Z - R));
            for (int i = 8 + step; i < cos.Length - 8 - step; i += step)
                for (int j = 0; j < cos.Length - step; j += step)
                    S.Vertices.Add(new Point3d(P.X + cos[j] * R * cos[i], P.Y + sin[j] * R * cos[i], P.Z - sin[i] * R));
            S.Vertices.Add(new Point3d(P.X, P.Y, P.Z + R));

            // build quad faces
            int U = 32 / step;
            for (int i = 0; i < U / 2 - 2; i++)
                for (int j = 0; j < U; j++)
                    S.Faces.AddFace(i * U + j + 1, (i * U + (j + 1) % U) + 1, ((i + 1) % U) * U + (j + 1) % U + 1, ((i + 1) % U) * U + j + 1);

            // build tri faces
            int last = S.Vertices.Count - 1;
            int start = last - U;

            for (int i = 0; i < U; i++)
            {
                S.Faces.AddFace(0, (i + 1) % U + 1, i + 1);
                S.Faces.AddFace(last, start + i, start + (i + 1) % U);
            }

            //S.Normals.ComputeNormals();
            return S;
        }

        public static Mesh MakeUnitSphereMesh(int step)
        {
            // build vertices
            Mesh S = new Mesh();
            S.Vertices.Add(new Point3d(0, 0, -1));
            for (int i = 8 + step; i < cos.Length - 8 - step; i += step)
                for (int j = 0; j < cos.Length - step; j += step)
                    S.Vertices.Add(new Point3d(cos[j] * cos[i], sin[j] * cos[i], -sin[i]));
            S.Vertices.Add(new Point3d(0, 0, 1));

            // build quad faces
            int U = 32 / step;
            for (int i = 0; i < U / 2 - 2; i++)
                for (int j = 0; j < U; j++)
                    S.Faces.AddFace(i * U + j + 1, (i * U + (j + 1) % U) + 1, ((i + 1) % U) * U + (j + 1) % U + 1, ((i + 1) % U) * U + j + 1);

            // build tri faces
            int last = S.Vertices.Count - 1;
            int start = last - U;

            for (int i = 0; i < U; i++)
            {
                S.Faces.AddFace(0, (i + 1) % U + 1, i + 1);
                S.Faces.AddFace(last, start + i, start + (i + 1) % U);
            }

            //S.Normals.ComputeNormals();
            return S;
        }
    }
}
