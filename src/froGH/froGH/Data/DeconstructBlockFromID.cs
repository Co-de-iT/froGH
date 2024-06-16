using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace froGH
{
    public class DeconstructBlockFromID : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructBlockFromID class.
        /// </summary>
        public DeconstructBlockFromID()
          : base("Deconstruct Block From ID", "f_BlDecon",
              "Deconstructs a Rhino Block from its name",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Block ID", "bID", "ID(name) of the Rhino Block", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Block content", "C", "The content of the Block", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> bID = new List<string>();

            if (!DA.GetDataList(0, bID)) return;

            DataTree<GeometryBase> gbs = new DataTree<GeometryBase>();

            for (int i = 0; i < bID.Count; i++)
            {

                InstanceDefinition def = RhinoDoc.ActiveDoc.InstanceDefinitions.Find(bID[i]);
                if (def == null)
                {
                    gbs.Add(null, new GH_Path(i));
                    continue;
                }
                RhinoObject[] objects = def.GetObjects();
                GeometryBase gb;
                for (int j = 0; j < objects.Length; j++)
                {
                    gb = objects[j].DuplicateGeometry();
                    gbs.Add(gb, new GH_Path(i));
                }
            }

            DA.SetDataTree(0, gbs);
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
                return Resources.DeconstructBlockFromID_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7bc55855-a1e8-4426-8c61-c82801ad02df"); }
        }
    }
}