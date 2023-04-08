using System;
using System.Collections.Generic;
using froGH.Properties;
using froGH.Utils;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class PathGenerator : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PathGenerator class.
        /// </summary>
        public PathGenerator()
          : base("Path Generator", "f_PathG",
              "Generates a path based on local directories such as Desktop, Documents, GH file path",
              "froGH", "File I-O")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Path Type", "P", "Path type:\n0 desktop\n1 documents\n2 GH file path\n3 C:\nConnect a value list for automatic list generation", GH_ParamAccess.item);
            pManager.AddTextParameter("Subdir", "D", "Custom subdirectory - optional", GH_ParamAccess.item);
            
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "Generated Path", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int type = 0;
            string subDir = null;

            DA.GetData(0, ref type);
            DA.GetData(1, ref subDir);

            String S = "";
            String root = "";
            String end = "\\";
            String eol = Environment.NewLine;

            // __________________ autoList part __________________

            // variable for the list
            Grasshopper.Kernel.Special.GH_ValueList vList;
            // tries to cast input as list
            try
            {

                // if the list is not the first parameter then change Input[0] to the corresponding value
                vList = (Grasshopper.Kernel.Special.GH_ValueList) Params.Input[0].Sources[0];

                if (!vList.NickName.Equals("Path Type"))
                {
                    vList.ClearData();
                    vList.ListItems.Clear();
                    vList.NickName = "Path Type";
                    var item1 = new Grasshopper.Kernel.Special.GH_ValueListItem("Desktop", "0");
                    var item2 = new Grasshopper.Kernel.Special.GH_ValueListItem("Documents", "1");
                    var item3 = new Grasshopper.Kernel.Special.GH_ValueListItem("GH file path", "2");
                    var item4 = new Grasshopper.Kernel.Special.GH_ValueListItem("C:", "3");
                    vList.ListItems.Add(item1);
                    vList.ListItems.Add(item2);
                    vList.ListItems.Add(item3);
                    vList.ListItems.Add(item4);

                    vList.ListItems[0].Value.CastTo(out type);
                }
            }
            catch
            {
                // handles anything that is not a value list
            }


            switch (type)
            {
                case 0:
                    root = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + end;
                    S = "Desktop";
                    break;
                case 1:
                    root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + end;
                    S = "Documents";
                    break;
                case 2:
                    //GH_Document ghDoc = OnPingDocument();

                    GH_Document ghDoc = null;// = Grasshopper.Instances.ActiveCanvas.Document;
                    try
                    {
                        ghDoc = OnPingDocument();
                    }
                    catch
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Document not found");
                    }

                    Utilities.IsDocumentInCluster(ref ghDoc);

                    //if (ghDoc == null || !ghDoc.IsFilePathDefined) return;

                    if (ghDoc.IsFilePathDefined)
                    {
                        int nameLen = ghDoc.DisplayName.TrimEnd('*').Length + 3; // +3 accounts for the '.gh' extension
                        int fileLen = ghDoc.FilePath.TrimEnd('*').Length;
                        root = ghDoc.FilePath.Substring(0, fileLen - nameLen);
                        S = "GH File Path";
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,"No GH Path defined. Saving to Desktop until then");
                        root = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        S = "Desktop";
                    }
                    break;
                case 3:
                    root = "C:" + end;
                    S = root;
                    break;
            }

            Message = S;

            DA.SetData(0, root + (subDir != null ? (subDir + end) : ""));


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
                return Resources.PathGen_4_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e3ad7f70-0de4-4bc1-a88b-1feb13701262"); }
        }
    }
}