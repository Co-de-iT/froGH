using froGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using System;
using System.Collections.Generic;

namespace froGH
{
    public class SelectRhinoObjects : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SelectRhinoObjects class.
        /// </summary>
        public SelectRhinoObjects()
          : base("SelectRhinoObjects", "f_SelRhObj",
              "Select referenced Objects in Rhino by their GUID\n" +
                "All kinds of Rhino entities can be selected",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Objects", "O", "Referenced Objects from Rhino to be selected\n" +
                "Attach a Geometry or GUID parameter component if objects are other than geometry\n" +
                "(ex: text, dimensions, dots, etc.)", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Selection trigger", "S", "Add a button to trigger selection", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Selected", "S", "A boolean result for each object, true if selection was successful", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<IGH_Goo> data = new List<IGH_Goo>();
            if (!DA.GetDataList(0, data)) return;

            bool select = false;
            DA.GetData(1, ref select);

            if (!select) return;

            RhinoDoc.ActiveDoc.Objects.UnselectAll();

            Guid id;
            IGH_GeometricGoo gGoo;
            List<bool> selected = new List<bool>();
            foreach (IGH_Goo goo in data)
            {
                id = Guid.Empty;
                if (!goo.IsValid)
                {
                    selected.Add(false);
                    continue;
                }

                goo.CastTo<Guid>(out id);
                if (id == Guid.Empty)
                {
                    gGoo = GH_Convert.ToGeometricGoo(goo);
                    id = gGoo.ReferenceID;
                }
                selected.Add(RhinoDoc.ActiveDoc.Objects.Select(id, true, true, true));
            }

            DA.SetDataList(0, selected);
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
                return Resources.SelectRhinoObjects_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8510797F-E7A9-4130-9119-78FBF3EE5214"); }
        }
    }
}