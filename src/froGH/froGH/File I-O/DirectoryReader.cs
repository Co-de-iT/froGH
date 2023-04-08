using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using froGH.Properties;
using Grasshopper.Kernel;

namespace froGH
{
    public class DirectoryReader : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DIrectoryReader class.
        /// </summary>
        public DirectoryReader()
          : base("Directory Reader", "f_DirRead",
              "Returns the list of Files and subdirectories in a Directory\nDouble click the component to update",
             "froGH", "File I-O")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Directory Path", "P", "Path to the directory to read", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("File list", "F", "List of files in the directory", GH_ParamAccess.list);
            pManager.AddTextParameter("Subdir list", "D", "List of subdirs in the directory", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string D = null;
            if (!DA.GetData(0, ref D)) return;

            if (!Directory.Exists(D))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Directory does not exist");

            // if missing, add the last \ character
            if (D[D.Length - 1] != '\\') D += '\\';

            List<string> F, S;

            F = Directory.GetFiles(D).Select(s => s.Remove(0, D.Length)).ToList();
            S = Directory.GetDirectories(D).Select(s => s.Remove(0, D.Length)).ToList();

            DA.SetDataList(0, F);
            DA.SetDataList(1, S);
        }

        public override void CreateAttributes()
        {
            m_attributes = new DirectoryReader_Attributes(this);
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
                return Resources.Read_Dir_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("fd1b969f-7ec2-4609-99e7-936dcc995997"); }
        }
    }
}