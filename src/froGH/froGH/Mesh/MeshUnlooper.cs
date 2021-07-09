using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class MeshUnlooper : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshUnlooper class.
        /// </summary>
        public MeshUnlooper()
          : base("Mesh Unlooper", "f_MUnl",
              "Opens closed Mesh loop strips",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Closed Mesh strip", "M", "Closed Mesh strip", GH_ParamAccess.item);
            pManager.AddNumberParameter("Displacement", "d", "Displacement for the added points to ensure the loop stays open", GH_ParamAccess.item, 0);

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Open Mesh strip", "M", "Open Mesh strip", GH_ParamAccess.item);
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

            double d = 0;
            DA.GetData(1, ref d);

            // a loop is a mesh with 2 naked edges closed polylines
            //
            // first, let's check if the naked loops are 2
            Polyline[] nE = M.GetNakedEdges();
            if (nE.Length != 2) return; // not a loop
                                        //
                                        // then, we'll check if they are closed
            bool closed = true;
            foreach (Polyline p in nE)
            {
                closed = closed && p.IsClosed;
            }
            if (!closed) return;

            //
            // extra check: let's see if all faces are connected in a ring
            // if that is the case, the number of connected faces to the first face
            // will be equal to the total number of faces
            int[] mF = M.Faces.GetConnectedFacesToEdges(0, true);
            if (mF.Length != M.Faces.Count) return;

            // M.GetNakedEdgePointStatus(); // this can be useful as well
            M.Weld(5);
            M.RebuildNormals();

            Mesh m1 = M.DuplicateMesh();

            MeshFace f0 = m1.Faces[0];
            Point3d p0 = m1.Vertices[f0.A];
            Point3d p1 = m1.Vertices[f0.D];
            p0 += (Vector3d)m1.Normals[f0.A] * d;
            p1 += (Vector3d)m1.Normals[f0.D] * d;

            m1.Vertices.Add(p0.X, p0.Y, p0.Z);
            m1.Vertices.Add(p1.X, p1.Y, p1.Z);
            m1.Faces.AddFace(m1.Vertices.Count - 2, f0.B, f0.C, m1.Vertices.Count - 1);
            m1.Faces.DeleteFaces(new int[] { 0 });

            m1.RebuildNormals();

            DA.SetData(0, m1);
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
                return Resources.Mesh_unlooper_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("af6ff499-71c3-407e-aee9-93fb4ecca036"); }
        }
    }
}