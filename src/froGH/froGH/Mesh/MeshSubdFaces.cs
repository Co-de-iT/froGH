using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class MeshSubdFaces : GH_Component
    {
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
            pManager.AddIntegerParameter("U subdivisions", "U", "The number of subdivisions in the U direction", GH_ParamAccess.item, 5);
            pManager.AddIntegerParameter("V subdivisions", "V", "The number of subdivisions in the V direction", GH_ParamAccess.item, 5);
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

            Point3d A, B, C, D;
            U++;
            V++;
            Mesh Mf, newM = new Mesh();

            for (int i = 0; i < M.Faces.Count; i++)
            {
                Point3d[][] grid = new Point3d[U][];
                for (int j = 0; j < U; j++) grid[j] = new Point3d[V];

                A = M.Vertices[M.Faces[i].A];
                B = M.Vertices[M.Faces[i].B];
                C = M.Vertices[M.Faces[i].C];
                D = M.Vertices[M.Faces[i].D];

                for (int k = 0; k < V; k++)
                {
                    double tk = k / (float)(V - 1);
                    grid[0][k] = A + (B - A) * tk;
                    grid[U - 1][k] = D + (C - D) * tk;

                }


                for (int k = 0; k < V; k++)
                    for (int j = 1; j < U - 1; j++)
                    {
                        double tj = j / (float)(U - 1);
                        grid[j][k] = grid[0][k] + (grid[U - 1][k] - grid[0][k]) * tj;
                    }

                for (int j = 0; j < U - 1; j++)
                    for (int k = 0; k < V - 1; k++)
                    {
                        Mf = new Mesh();
                        Mf.Vertices.Add(grid[j][k]);
                        Mf.Vertices.Add(grid[j][k + 1]);
                        Mf.Vertices.Add(grid[j + 1][k + 1]);
                        Mf.Vertices.Add(grid[j + 1][k]);
                        Mf.Faces.AddFace(0, 1, 2, 3);
                        newM.Append(Mf);
                    }
            }

            newM.RebuildNormals();
            newM.Weld(0.0);

            DA.SetData(0, newM);
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
                return Resources.divide_mesh_quad_face_bn_GH;
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