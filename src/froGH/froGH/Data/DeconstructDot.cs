using froGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using System;

namespace froGH
{
    public class DeconstructDot : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructDot class.
        /// </summary>
        public DeconstructDot()
          : base("Deconstruct Dot", "f_DotDecon",
              "Deconstructs a Rhino Dot Object\n(best used in combination with Deconstruct Block)",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Text Dot", "D", "The Text Dot object from Rhino", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Location", "L", "Location of Text Dot object", GH_ParamAccess.item);
            pManager.AddTextParameter("Main Text", "T", "Main Text", GH_ParamAccess.item);
            pManager.AddTextParameter("Secondary Text", "T2", "Secondary Text", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Text Size", "S", "Text Size", GH_ParamAccess.item);
            pManager.AddTextParameter("Text Font", "F", "Text Font Face", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            IGH_Goo dotInput = null;
            TextDot dot = null;
            GH_Guid dotGuid;

            if (!DA.GetData(0, ref dotInput)) return;

            //dotInput.CastTo(out dot);

            if (!dotInput.CastTo(out dot))
            {
                dotGuid = dotInput as GH_Guid;
                Guid dg = dotGuid.Value;
                var dotDoc = RhinoDoc.ActiveDoc.Objects.FindId(dg);
                dot = dotDoc.Geometry as TextDot;
            }

            DA.SetData(0, dot.Point);
            DA.SetData(1, dot.Text);
            DA.SetData(2, dot.SecondaryText);
            DA.SetData(3, dot.FontHeight);
            DA.SetData(4, dot.FontFace);

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
                return Resources.Deconstruct_Dot_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3d65c517-d171-4f55-b708-d6c0176e8cdd"); }
        }
    }
}