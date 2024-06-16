using System;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class MeshToCSSnippet : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshToCSSnippet class.
        /// </summary>
        public MeshToCSSnippet()
          : base("Mesh To C# Snippet", "f_M2CS",
              "Generates C# code for a given Mesh",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input Mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("C# snippet", "C#", "C# Text snippet", GH_ParamAccess.item);
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

            string eol = Environment.NewLine;
            string snippet = "Mesh mesh = new Mesh();" + eol + eol;

            snippet += "Point3d[] vertices = new Point3d[]{";

            for (int i = 0; i < M.Vertices.Count; i++)
            {
                snippet += string.Format("new Point3d({0}, {1}, {2})", M.Vertices[i].X, M.Vertices[i].Y, M.Vertices[i].Z);
                if (i < M.Vertices.Count - 1) snippet += ", ";
            }

            snippet += "};" + eol + eol;
            snippet += "mesh.Vertices.AddVertices(vertices);" + eol + eol;

            for (int i = 0; i < M.Faces.Count; i++)
            {
                if (M.Faces[i].IsTriangle)
                    snippet += string.Format("mesh.Faces.AddFace({0}, {1}, {2});{3}", M.Faces[i].A, M.Faces[i].B, M.Faces[i].C, eol);
                else
                    snippet += string.Format("mesh.Faces.AddFace({0}, {1}, {2}, {3});{4}", M.Faces[i].A, M.Faces[i].B, M.Faces[i].C, M.Faces[i].D, eol);
            }

            snippet += eol;

            snippet += "mesh.UnifyNormals();" + eol + "mesh.RebuildNormals();";

            DA.SetData(0, snippet);
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.quinary; }
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
                return Resources.Mesh2CSSnippet_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8856d695-4fe7-4c0c-aa7d-dde6494aac12"); }
        }
    }
}