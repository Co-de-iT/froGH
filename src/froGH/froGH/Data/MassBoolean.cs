using froGH.Properties;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace froGH
{
    public class MassBoolean : GH_Component
    {

        //private int type;
        private bool result;
        private string bOperator;

        /// <summary>
        /// Initializes a new instance of the MassBoolean class.
        /// </summary>
        public MassBoolean()
          : base("Mass Boolean", "f_MassBool",
              "Mass boolean operator on a list of values",
              "froGH", "Data")
        {
            bOperator = GetValue("Boolean Operator", "AND");
            UpdateMessage();
            ExpireSolution(true);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Values", "V", "Boolean Values for mass operation", GH_ParamAccess.list);
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

            List<bool> values = new List<bool>();
            if (!DA.GetDataList(0, values)) return;
            if (values == null || values.Count == 0) return;

            result = values[0];

            if (bOperator == "AND")
                for (int i = 1; i < values.Count; i++)
                    result = result && values[i];
            else
                for (int i = 1; i < values.Count; i++)
                    result = result || values[i];


            DA.SetData(0, result);
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            ToolStripMenuItem toolStripMenuItem = Menu_AppendItem(menu, "AND", AND_Click, true, GetValue("Boolean Operator", "AND") == "AND");
            ToolStripMenuItem toolStripMenuItem2 = Menu_AppendItem(menu, "OR", OR_Click, true, GetValue("Boolean Operator", "AND") == "OR");
            Menu_AppendSeparator(menu);
        }

        private void AND_Click(object sender, EventArgs e)
        {
            RecordUndoEvent("AND");
            SetValue("Boolean Operator", "AND");
            bOperator = GetValue("Boolean Operator", "AND");
            UpdateMessage();
            ExpireSolution(true);
        }

        private void OR_Click(object sender, EventArgs e)
        {
            RecordUndoEvent("OR");
            SetValue("Boolean Operator", "OR");
            bOperator = GetValue("Boolean Operator", "AND");
            UpdateMessage();
            ExpireSolution(true);
        }

        private void UpdateMessage()
        {
            Message = bOperator;
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("BooleanOperator", bOperator);

            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            //bOperator = "AND";
            reader.TryGetString("BooleanOperator", ref bOperator);
            UpdateMessage();
            //Message = bOperator;
            return base.Read(reader);
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