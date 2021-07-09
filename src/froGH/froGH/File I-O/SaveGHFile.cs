using System;
using System.Collections.Generic;
using System.IO;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class SaveGHFile : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the SaveGHFile class.
        /// </summary>
        public SaveGHFile()
          : base("Save GH File", "f_GHSave",
              "Saves a copy of the current GH file\nIf no custom name and/or directory are supplied,\nthe ones of the current document are used",
              "froGH", "File I-O")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Path", "P", "File Path", GH_ParamAccess.item);
            pManager.AddTextParameter("File Name", "F", "File Name", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Save trigger", "s", "Activate saving - attach a button and click to save", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Saved File Path", "Fp", "Directory and file name that will be saved", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string D = "";
            if (!DA.GetData(0, ref D)) return;
            string F = "";
            if (!DA.GetData(1, ref F)) return;
            bool S = false;
            DA.GetData(2, ref S);

            GH_Document ghDoc = null;// = Grasshopper.Instances.ActiveCanvas.Document;
            try
            {
                ghDoc = OnPingDocument();
            }
            catch
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Document not found");
            }

            if (ghDoc == null || !ghDoc.IsFilePathDefined) return;

            GH_DocumentIO docIO = new GH_DocumentIO(GH_Document.DuplicateDocument(Grasshopper.Instances.ActiveCanvas.Document));
            string path, file;

            int nameLen = ghDoc.DisplayName.TrimEnd('*').Length + 3; // +3 accounts for the '.gh' extension missing here
            int fileLen = ghDoc.FilePath.TrimEnd('*').Length;
            if (D == "" || D == null) path = ghDoc.FilePath.Substring(0, fileLen - nameLen);
            else path = D;
            if (F == "" || F == null) file = ghDoc.DisplayName.TrimEnd('*') + ".gh";
            else file = F + ".gh";

            docIO.Document.FilePath = path + file;

            if (S)
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                docIO.Save();
            }
            
            DA.SetData(0,docIO.Document.FilePath);
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
                return Resources.saveAsGH_2_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d0506527-ef94-4005-af94-91483edc33cf"); }
        }
    }
}