using System;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class ReduceMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ReduceMesh class.
        /// </summary>
        public ReduceMesh()
          : base("Reduce Mesh", "f_RedMesh",
              "Reduce Mesh polygon count",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Mesh to reduce", GH_ParamAccess.item);
            pManager.AddNumberParameter("Reduction ratio", "r", "the % of initial vertices and faces to kill (0..1)", GH_ParamAccess.item, 0.5);
            pManager.AddBooleanParameter("Distortion", "d", "Allow Distortion", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Accuracy", "a", "Accuracy (1..10)", GH_ParamAccess.item, 5);
            pManager.AddBooleanParameter("Normalize Length", "n", "Normalize Length", GH_ParamAccess.item, false);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The reduced Mesh", GH_ParamAccess.item);
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

            double r = 0;
            bool dist = false;
            int acc = 0;
            bool norm = false;

            DA.GetData(1, ref r);
            DA.GetData(2, ref dist);
            DA.GetData(3, ref acc);
            DA.GetData(4, ref norm);

            int poly = (int)(M.Faces.Count * (1 - r));
            // M.Reduce(desired polygon Count, allow distortion, accuracy (less accurate 1-10 more accurate), normalize Size)
            M.Reduce(poly, dist, acc, norm);

            DA.SetData(0, M);
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
                return Resources.ReduceMesh_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("bd32000c-4e1b-42b7-9075-21340dcdd99b"); }
        }
    }
}