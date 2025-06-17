using froGH.Properties;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace froGH
{
    public class MeshSubdFaces : GH_Component
    {
        readonly object locker = new object();

        /// <summary>
        /// Initializes a new instance of the MeshSubdFaces class.
        /// </summary>
        public MeshSubdFaces()
          : base("Mesh Subdivide Quad Faces", "f_SubMQF",
              "Subdivides a Quad Mesh Face in a custom UV number of sub-faces",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Quad Mesh", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U subdivisions", "U", "The number of subdivisions in the U direction", GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("V subdivisions", "V", "The number of subdivisions in the V direction", GH_ParamAccess.item, 2);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Subdivided Mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh M = new Mesh();
            if (!DA.GetData(0, ref M)) return;
            if (!(M.IsValid) || M == null) return;
            int U = 5;
            DA.GetData(1, ref U);
            int V = 5;
            DA.GetData(2, ref V);

            U++;
            V++;
            Mesh newMesh;

            // DivideUVEqual Method breaks with Weaverbird subdivided meshes
            // moreover, the performance is almost identical to the other method
            //if (U == V && M.Faces.TriangleCount == 0)
            //    newMesh = DivideUVEqual(M, U);
            //else
            newMesh = DivideUV(M, U, V);

            DA.SetData(0, newMesh);
        }

        private Mesh DivideUV(Mesh M, int Upts, int Vpts)
        {
            Mesh newMesh = new Mesh();
            // array of new mesh faces
            Mesh[] meshFaces = new Mesh[M.Faces.Count];

            double Ustep = 1.0 / (Upts - 1);
            double Vstep = 1.0 / (Vpts - 1);

            int cores = Environment.ProcessorCount;
            int count = M.Faces.Count;

            Parallel.For(0, cores, threadId =>
            {
                Point3f A, B, C, D;
                float tj, tk;

                Point3f[][] grid = new Point3f[Upts][];
                for (int j = 0; j < Upts; j++) grid[j] = new Point3f[Vpts];

                int max = count * (threadId + 1) / cores;

                lock (locker)
                {
                    for (int i = count * threadId / cores; i < max; i++)
                    {
                        A = M.Vertices[M.Faces[i].A];
                        B = M.Vertices[M.Faces[i].B];
                        C = M.Vertices[M.Faces[i].C];
                        D = M.Vertices[M.Faces[i].D];

                        meshFaces[i] = new Mesh();

                        //compute grid points (mesh vertices)
                        for (int k = 0; k < Vpts; k++)
                        {
                            tk = (float)(k * Vstep);
                            for (int j = 0; j < Upts; j++)
                            {
                                tj = (float)(j * Ustep);
                                grid[j][k] = A * (1 - tj) * (1 - tk) + B * tj * (1 - tk) + C * tj * tk + D * (1 - tj) * tk;
                                meshFaces[i].Vertices.Add(grid[j][k]);
                            }
                        }

                        // compute Mesh faces
                        for (int j = 0; j < Upts - 1; j++)
                            for (int k = 0; k < Vpts - 1; k++)
                                meshFaces[i].Faces.AddFace(k * Upts + j, k * Upts + (j + 1), (k + 1) * Upts + (j + 1), (k + 1) * Upts + j);

                    }
                }
            });

            Task.WaitAll();


            for (int i = 0; i < meshFaces.Length; i++)
                newMesh.Append(meshFaces[i]);
            newMesh.RebuildNormals();

            return newMesh;
        }

        /// <summary>
        /// For the cases in which U == V and mesh is all quads - INCOMPLETE (AND SO FAR NOT USED)
        /// </summary>
        /// <param name="M">Mesh</param>
        /// <param name="Upts">number of points = n. of divisions +1</param>
        /// <returns></returns>
        private Mesh DivideUVEqual(Mesh M, int Upts)
        {
            Mesh newMesh = M.DuplicateMesh();
            newMesh.Weld(Math.PI);

            // . . . 1. compute new vertices

            int[][] newVertIndices = new int[newMesh.TopologyEdges.Count][];
            int count = newMesh.Vertices.Count;
            double step = 1.0 / (Upts - 1);
            Point3f A, B, V;
            float t;

            // 1.1 create new vertices on edges
            for (int i = 0; i < newMesh.TopologyEdges.Count; i++)
            {
                // indices of edge extremities vertices
                IndexPair edgePts = newMesh.TopologyEdges.GetTopologyVertices(i);
                A = newMesh.TopologyVertices[edgePts[0]];
                B = newMesh.TopologyVertices[edgePts[1]];
                newVertIndices[i] = new int[Upts];
                newVertIndices[i][0] = edgePts[0];
                newVertIndices[i][Upts - 1] = edgePts[1];

                for (int j = 1; j < Upts - 1; j++)
                {
                    // compute new vertex and add it to the mesh
                    t = (float)(j * step);
                    V = A * (1 - t) + B * t;
                    newMesh.Vertices.Add(V);
                    // record vertices indices for this edge
                    newVertIndices[i][j] = count;
                    count++;
                }

            }

            //// 1.1.1 create new vertices on edges (parallel)
            //Point3f[] newpts = new Point3f[newMesh.TopologyEdges.Count * (Upts - 2)];
            //Parallel.For(0, newMesh.TopologyEdges.Count, i =>
            //{
            //    Point3f Ap, Bp, Vp;
            //    float tp;
            //    // indices of edge extremities vertices
            //    IndexPair edgePts = newMesh.TopologyEdges.GetTopologyVertices(i);
            //    Ap = newMesh.TopologyVertices[edgePts[0]];
            //    Bp = newMesh.TopologyVertices[edgePts[1]];
            //    newVertIndices[i] = new int[Upts];
            //    newVertIndices[i][0] = edgePts[0];
            //    newVertIndices[i][Upts - 1] = edgePts[1];

            //    int countPar = i * (Upts - 2);
            //    for (int j = 1; j < Upts - 1; j++)
            //    {
            //        // compute new vertex and add it to the mesh
            //        tp = (float)(j * step);
            //        Vp = Ap * (1 - tp) + Bp * tp;
            //        newpts[countPar + j - 1] = Vp;
            //        // newMesh.Vertices.Add(Vp);
            //        // record vertices indices for this edge
            //        newVertIndices[i][j] = count+countPar+j-1;
            //    }
            //});
            //newMesh.Vertices.AddVertices(newpts);
            //count = newMesh.Vertices.Count;


            // 1.2 create intermediate vertices and new faces
            int[] currentEdges;
            int[,] grid;
            int oldFacesCount = newMesh.Faces.Count;
            List<MeshFace> newFaces = new List<MeshFace>();

            for (int i = 0; i < oldFacesCount; i++)
            {
                // get topology edges for current face
                currentEdges = newMesh.TopologyEdges.GetEdgesForFace(i);

                // populate point matrix frame from edges

                grid = new int[Upts, Upts];

                // get frame indices for currentEdges 0 and 2
                for (int j = 0; j < Upts; j++)
                {
                    grid[0, j] = newVertIndices[currentEdges[0]][j];
                    grid[Upts - 1, j] = newVertIndices[currentEdges[2]][j];
                }
                // get frame indices for currentEdges 1 and 3
                for (int j = 1; j < Upts - 1; j++)
                {
                    grid[j, 0] = newVertIndices[currentEdges[1]][j];
                    grid[j, Upts - 1] = newVertIndices[currentEdges[3]][j];
                }

                // nested loop to make intermediate points
                for (int k = 1; k < Upts - 1; k++)
                {
                    A = newMesh.TopologyVertices[grid[0, k]];
                    B = newMesh.TopologyVertices[grid[Upts - 1, k]];
                    for (int j = 1; j < Upts - 1; j++)
                    {
                        // compute new vertex and add it to the mesh
                        t = (float)(j * step);
                        V = A * (1 - t) + B * t;
                        newMesh.Vertices.Add(V);
                        // record vertices indices
                        grid[j, k] = count;
                        count++;
                    }
                }

                // build new faces indices
                for (int j = 0; j < Upts - 1; j++)
                    for (int k = 0; k < Upts - 1; k++)
                        newFaces.Add(new MeshFace(grid[j, k], grid[j, k + 1], grid[j + 1, k + 1], grid[j + 1, k]));
            }

            // add new faces and remove old faces 
            for (int i = 0; i < newFaces.Count; i++)
                newMesh.Faces.AddFace(newFaces[i]);

            for (int i = oldFacesCount - 1; i >= 0; i--)
                newMesh.Faces.RemoveAt(i);

            // . . . 2. output newMesh
            newMesh.UnifyNormals();
            newMesh.RebuildNormals();

            return newMesh;
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.SubdivideMeshQuadFace_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("81b6f196-3eb0-4001-b588-6f4d81f0f623"); }
        }
    }
}