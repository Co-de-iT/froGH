using System;
using System.Collections.Generic;
using froGH.Properties;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace froGH
{
    public class MassBoolean : GH_Component
    {

        private int type;
        private bool result;

        /// <summary>
        /// Initializes a new instance of the MassBoolean class.
        /// </summary>
        public MassBoolean()
          : base("Mass Boolean", "f_MassBool",
              "Mass boolean operator on a list of values",
              "froGH", "Data")
        {
            type = 0;
            updateMessage();
        }

        private void updateMessage()
        {
            if (type == 0)
            {
                Message = "AND";
                result = true;
            }
            else
            {
                Message = "OR";
                result = false;
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Values", "V", "Boolean Values for mass operation", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Boolean operator", "o", "type of operator:\n0 - AND\n1 - OR", GH_ParamAccess.item, 0);
            //pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Result", "B", "Boolean Result", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData(1, ref type))
            {
                Params.OnParametersChanged();
                var pType = Params.Input[1] as GH_PersistentParam<GH_Integer>;
                type = pType.PersistentData[0][0].Value;
            }

            List<bool> values = new List<bool>();
            if (!DA.GetDataList(0, values))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input V failed to collect data");
            }

            updateMessage();

            for (int i = 0; i < values.Count; i++)
            {
                if (type == 0)
                {
                    result = result && values[i];
                }
                else
                {
                    result = result || values[i];
                }
            }

            DA.SetData(0, result);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("Boolean operator", type);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            type = 0;
            reader.TryGetInt32("Boolean operator", ref type);
            updateMessage();
            return base.Read(reader);
        }

        /// <summary>
        /// Exposure override for position in the SUbcategory (options primary to septenary)
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
                return Resources.MassBool_2_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c4f0bc72-6a60-4483-b8b6-a8f2cb2fa15a"); }
        }
    }
}