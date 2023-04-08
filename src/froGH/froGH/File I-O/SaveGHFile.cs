using System;
using System.Collections.Generic;
using System.IO;
using froGH.Properties;
using froGH.Utils;
using GH_IO.Serialization;
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
              "Saves a copy of the current GH document as a new file\nUse with a Button or a Froggle (not a Toggle)",
              "froGH", "File I-O")
        { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Path", "P", "File Path\nIf no directory is supplied,\nthe one of the current document will be used", GH_ParamAccess.item);
            pManager.AddTextParameter("File Name", "F", "File Name (without the .gh extension)\nIf no name is supplied,\nthe current document name will be used", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Save", "s", "Activate saving\nattach a Button or a Froggle and set to True to save" +
                "\nfor best results, use a Froggle, not a Toggle", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Saved File Path", "P", "Directory and file name that will be saved", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Saved File status", "S", "True if File was saved successfully", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string P = "";
            if (!DA.GetData(0, ref P)) return;
            string F = "";
            if (!DA.GetData(1, ref F)) return;
            bool saveFile = false;
            DA.GetData(2, ref saveFile);

            GH_Document ghDoc = null;
            try
            {
                ghDoc = OnPingDocument();
            }
            catch
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Save this GH Document first");
            }

            // check if we are inside a cluster
            Utilities.IsDocumentInCluster(ref ghDoc);

            if (ghDoc == null || !ghDoc.IsFilePathDefined) return;

            // define and check name and path
            string destinationPath, fileName;
            int nameLen = ghDoc.DisplayName.TrimEnd('*').Length + 3; // +3 accounts for the '.gh' extension missing here
            int fileLen = ghDoc.FilePath.TrimEnd('*').Length;

            if (P == "" || P == null) destinationPath = ghDoc.FilePath.Substring(0, fileLen - nameLen);
            else destinationPath = P;
            if (F == "" || F == null) fileName = ghDoc.DisplayName.TrimEnd('*') + ".gh";
            else fileName = F + ".gh";

            bool result = false;

            if (saveFile)
            {
                //// if a Toggle is attached (not considered anymore)
                //if (Params.Input[2].Sources[0] is Grasshopper.Kernel.Special.GH_BooleanToggle toggle)
                //    result = SaveWithToggle(ghDoc, toggle, destinationPath, fileName);
                //// in any other case...
                //else
                //{
                GH_DocumentIO docIO = new GH_DocumentIO(GH_Document.DuplicateDocument(Grasshopper.Instances.ActiveCanvas.Document));
                docIO.Document.FilePath = destinationPath + fileName;

                // save new file
                if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);
                result = docIO.Save();
                //}
            }

            DA.SetData(0, result ? $"{destinationPath + fileName}" : "File not saved");
            DA.SetData(1, result);
        }

        private bool SaveWithToggle(GH_Document ghDoc, Grasshopper.Kernel.Special.GH_BooleanToggle toggle, string destinationPath, string fileName)
        {
            bool result = false;
            GH_DocumentIO docIO = new GH_DocumentIO(ghDoc), docIOCopy;

            // deactivate toggle for the save operation
            toggle.Value = false;
            toggle.ExpireSolution(true);

            // save current document
            docIO.Save();
            docIOCopy = new GH_DocumentIO(GH_Document.DuplicateDocument(Grasshopper.Instances.ActiveCanvas.Document));
            docIOCopy.Document.FilePath = destinationPath + fileName;

            // save new file
            if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);
            result = docIOCopy.Save();

            // restore toggle status
            //toggle.Value = true;
            //toggle.ExpireSolution(true);

            // saving works, but it currently does not output after saving (resetting toggle to False makes component exit)
            // keeping toggle reset to True sets even the saved file toggle to true (huh?!?)

            return result;
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