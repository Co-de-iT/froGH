using System;
using System.Collections.Generic;
using System.Linq;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class ExtractMeshFace : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ExtractMeshFace class.
        /// </summary>
        public ExtractMeshFace()
          : base("Extract Mesh Faces", "f_MFaces",
              "Extract Mesh Faces (by index) as Mesh",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Input Mesh to process", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Face Indexes", "i", "The face indexes", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh Face", "F", "The Faces at indexes i as Mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh M = new Mesh();
            if (!DA.GetData(0, ref M)) return;
            if (!M.IsValid || M == null) return;

            List<int> i = new List<int>();
            DA.GetDataList(1, i);

            List<int> ind = i.Select(id => id % M.Faces.Count).ToList();

            Mesh F = meshFromFace(M, ind);

            DA.SetData(0, F);
        }

        private Mesh meshFromFace(Mesh M, List<int> fInd)
        {
            Mesh mOut = new Mesh();
            for (int i = 0; i < fInd.Count; i++)
            {
                Mesh M1 = new Mesh();
                MeshFace f = M.Faces.GetFace(fInd[i]);
                if (f.IsValid(M.Vertices.Count))
                {
                    M1.Vertices.Add(M.Vertices[f.A]);
                    M1.Vertices.Add(M.Vertices[f.B]);
                    M1.Vertices.Add(M.Vertices[f.C]);
                    if (f.IsTriangle)
                    {
                        M1.Faces.AddFace(0, 1, 2);
                    }
                    else if (f.IsQuad)
                    {
                        M1.Vertices.Add(M.Vertices[f.D]);
                        M1.Faces.AddFace(0, 1, 2, 3);
                    }
                }
                mOut.Append(M1);
            }
            return mOut;
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
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
                return Resources.Mesh_Extract_Face_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("20982d84-9dab-408e-8710-461c5bed34ee"); }
        }
    }
}