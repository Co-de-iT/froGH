using froGH.Properties;
using Grasshopper.Kernel;
using System;

namespace froGH
{
    public class GHFilePathInfo : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FilePathInfo class.
        /// </summary>
        public GHFilePathInfo()
          : base("GH File Path Info", "f_GHFilePath",
              "Provides info on current Grasshopper Document file name and path\nUseful for exporting files with the same name as the gh file and in the same directory",
              "froGH", "File I-O")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("File Name", "F", "Current Grasshopper File Name", GH_ParamAccess.item);
            pManager.AddTextParameter("File Path", "P", "Current Grasshopper File Path", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Document ghDoc = null;// = Grasshopper.Instances.ActiveCanvas.Document;
            try
            {
                ghDoc = OnPingDocument();
            }
            catch
            {
               AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Document not found");
            }

            // check if component is inside a cluster, taking care of nested clusters if necessary
            // see: https://discourse.mcneel.com/t/how-to-differ-a-clustered-gh-scriptcomponents-inparam-from-a-clusterinput/61459/4
            var owner = ghDoc.Owner;
            var cluster = owner as Grasshopper.Kernel.Special.GH_Cluster;
            while (cluster != null)
            {
                ghDoc = ghDoc.Owner.OwnerDocument();
                owner = ghDoc.Owner;
                cluster = owner as Grasshopper.Kernel.Special.GH_Cluster;
            }

            if (ghDoc == null || !ghDoc.IsFilePathDefined) return;

            int nameLen = ghDoc.DisplayName.TrimEnd('*').Length + 3; // +3 accounts for the '.gh' extension
            int fileLen = ghDoc.FilePath.TrimEnd('*').Length;

            string F = ghDoc.DisplayName.TrimEnd('*');
            string P = ghDoc.FilePath.Substring(0, fileLen - nameLen);

            DA.SetData(0, F);
            DA.SetData(1, P);
        }

        public override void CreateAttributes()
        {
            m_attributes = new GHFilePathInfo_Attributes(this);
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
                return Resources.GHFileDir_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b08c361e-ed82-461a-853d-37ce9159275c"); }
        }
    }
}