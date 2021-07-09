using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class ReadFileFromURL : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ReadFileFromURL class.
        /// </summary>
        public ReadFileFromURL()
          : base("Read File From URL", "f_URLr",
              "Reads a file from a URL",
              "froGH", "File I-O")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("URL address", "URL", "URL of the file to read", GH_ParamAccess.item);
            pManager.AddBooleanParameter("By Line", "L", "if true, each line is read as a separate string - default false", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("String of content", "S", "The file content in string format", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string URL = "";
            if (!DA.GetData(0, ref URL)) return;
            bool byLine = false;
            DA.GetData(1, ref byLine);

            System.Net.WebClient webClient = new System.Net.WebClient();
            string readHtml = webClient.DownloadString(URL);

            List<string> fileContent = new List<string>();

            if(byLine)
                fileContent = readHtml.Split((char)'\n').ToList();
            else fileContent.Add(readHtml);

            DA.SetDataList(0, fileContent);

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
                return Resources.Load_from_Web_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e2e4dfb6-7bff-4765-be6f-acb531e82cc2"); }
        }
    }
}