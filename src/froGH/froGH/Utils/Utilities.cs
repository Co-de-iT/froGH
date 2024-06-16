using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace froGH.Utils
{
    public static class Utilities
    {
        //LUTs for sin and cos operations
        static readonly double[] cos = new double[]{
        1, 0.980785, 0.92388, 0.83147, 0.707107, 0.55557, 0.382683, 0.19509,
        0, -0.19509, -0.382683, -0.55557, -0.707107, -0.83147, -0.92388, -0.980785,
        -1, -0.980785, -0.92388, -0.83147, -0.707107, -0.55557, -0.382683, -0.19509,
         0,  0.19509, 0.382683, 0.55557, 0.707107, 0.83147, 0.92388, 0.980785, 1};

        static readonly double[] sin = new double[] {
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

            faces = null;

            // look for faces only if there is more than 1 edge
            if (edges.Length == 1) return;

            // find neighbour faces in edges order
            // try this: pick first face index as face shared from first and second edge
            // for the following edges pick the index (of the two) that is different from the previous iteration
            faces = new int[edges.Length];
            List<int> facesList = new List<int>();


            // first edge/face index
            int[] connectedFaces;
            int faceCount = 0;
            // find connected faces indexes to first edge
            connectedFaces = M.TopologyEdges.GetConnectedFaces(edges[0]);
            if (connectedFaces.Length != 0)
            {
                for (int j = 0; j < connectedFaces.Length; j++)
                {
                    // find edges for j-th connected face
                    int[] fEdges = M.TopologyEdges.GetEdgesForFace(connectedFaces[j]);
                    // if face contains also index of second edge then we found our first face
                    if (fEdges.Contains(edges[1]))
                    {
                        facesList.Add(connectedFaces[j]);
                        faceCount = 1;
                        break;
                    }
                }
            }
            // all other edges
            for (int j = 1; j < edges.Length; j++)
            {
                connectedFaces = M.TopologyEdges.GetConnectedFaces(edges[j]);
                // if it's a clothed edge
                if (connectedFaces.Length > 1)
                {
                    facesList.Add(connectedFaces[0] != facesList[faceCount - 1] ? connectedFaces[0] : connectedFaces[1]);
                    faceCount++;
                }
                // else, if naked...
                else if (connectedFaces.Length > 0 && connectedFaces[0] != facesList[faceCount - 1])
                {
                    facesList.Add(connectedFaces[0]);
                    faceCount++;
                }
            }

            faces = facesList.ToArray();

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

        /// <summary>
        /// Checks if document is in a cluster - in that case looks for the root document and assigns it as the new <paramref name="ghDoc"/>
        /// </summary>
        /// <param name="ghDoc"></param>
        /// <returns>True if <paramref name="ghDoc"/> is in a cluster</returns>
        public static bool IsDocumentInCluster(ref GH_Document ghDoc)
        {
            // check if component is inside a cluster, taking care of nested clusters if necessary
            // see: https://discourse.mcneel.com/t/how-to-differ-a-clustered-gh-scriptcomponents-inparam-from-a-clusterinput/61459/4
            bool isInCluster = false;
            //ghDoc = ghDocIn;
            var owner = ghDoc.Owner;
            var cluster = owner as Grasshopper.Kernel.Special.GH_Cluster;
            while (cluster != null)
            {
                ghDoc = ghDoc.Owner.OwnerDocument();
                owner = ghDoc.Owner;
                cluster = owner as Grasshopper.Kernel.Special.GH_Cluster;
                isInCluster = true;
            }

            return isInCluster;
        }

        public static T[][] ToJaggedArray<T>(DataTree<T> values)
        {
            T[][] valuesArray = new T[values.BranchCount][];

            T[] valArray;
            for (int i = 0; i < values.BranchCount; i++)
            {
                valArray = new T[values.Branches[i].Count];

                for (int j = 0; j < values.Branches[i].Count; j++)
                {
                    valArray[j] = values.Branches[i][j];
                }
                valuesArray[i] = valArray;
            }
            return valuesArray;
        }

        public static DataTree<T> ToDataTree<T>(List<T>[] values)
        {
            DataTree<T> valuesTree = new DataTree<T>();
            for (int i = 0; i < values.Length; i++)
                valuesTree.AddRange(values[i], new GH_Path(i));

            return valuesTree;
        }

        public static DataTree<T> ToDataTree<T>(T[][] values)
        {
            DataTree<T> valuesTree = new DataTree<T>();
            for (int i = 0; i < values.Length; i++)
                    valuesTree.AddRange(values[i], new GH_Path(i));

            return valuesTree;
        }

        public static DataTree<T> ToDataTree<T>(List<T>[][] values)
        {
            DataTree<T> valuesTree = new DataTree<T>();
            for (int i = 0; i < values.Length; i++)
                for (int j = 0; j < values[i].Length; j++)
                    valuesTree.AddRange(values[i][j], new GH_Path(i, j));

            return valuesTree;
        }

        public static DataTree<T> ToDataTree<T>(T[][][] values)
        {
            DataTree<T> valuesTree = new DataTree<T>();
            for (int i = 0; i < values.Length; i++)
                for (int j = 0; j < values[i].Length; j++)
                    valuesTree.AddRange(values[i][j], new GH_Path(i, j));

            return valuesTree;
        }
    }
}
