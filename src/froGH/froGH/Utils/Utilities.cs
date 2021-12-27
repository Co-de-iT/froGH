using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace froGH.Utils
{
    public static class Utilities
    {
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
    }
}
