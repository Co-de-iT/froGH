using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class MeshDenoising : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshDenoising class.
        /// </summary>
        public MeshDenoising()
          : base("Mesh De-Noising", "f_MDN",
              "Reduces noise in a mesh (i.e. from scanned pointclouds or messy ones)\nalgorithm by Paul Bourke - 1997 - http://paulbourke.net/geometry/polygonmesh/",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Mesh to de-noise", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Iterations", "i", "The number of iterations to perform", GH_ParamAccess.item, 1);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The De-Noised Mesh", GH_ParamAccess.item);
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
            int i = 1;
            DA.GetData(1, ref i);

            Point3d[] tVerts;

            for (int j = 0; j < i; j++)
            {
                tVerts = new Point3d[M.Vertices.Count];

                Parallel.For(0, M.Vertices.Count, k =>
                {
                    int[] neigh = M.Vertices.GetConnectedVertices(k);

                    Point3d newP = new Point3d();
                    Point3d thisP = M.Vertices[k];

                    for (int p = 0; p < neigh.Length; p++)
                    {
                        newP += M.Vertices[neigh[p]] - thisP;
                    }

                    newP /= (float)neigh.Length;

                    tVerts[k] = thisP + newP;

                });

                for (int k = 0; k < M.Vertices.Count; k++)
                    M.Vertices.SetVertex(k, tVerts[k]);

            }
            M.RebuildNormals();

            DA.SetData(0, M);
        }

        /// <summary>
        /// Exposure override for position in the SUbcategory (options primary to septenary)
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
                return Resources.Mesh_de_noising_2_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0b111690-af6b-4336-959f-1abf597c27f0"); }
        }
    }
}