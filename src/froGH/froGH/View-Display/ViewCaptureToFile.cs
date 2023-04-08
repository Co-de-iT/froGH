using froGH.Properties;
using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.Linq;

namespace froGH
{
    public class ViewCaptureToFile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ViewCaptureToFile class.
        /// </summary>
        public ViewCaptureToFile()
          : base("View Capture To File", "f_VC2F",
              "Saves a bitmap of the selected Rhino viewport - with options\nMake sure this object is on top of all other objects\n(i.e.executes last)",
              "froGH", "View/Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Path", "P", "File absolute path\nPath MUST already exist", GH_ParamAccess.item);
            pManager.AddTextParameter("File Name", "F", "Filename and extension - for example screencap.png\nuse # to set the number of digits\nex screencap####.png > screencap_0000.png\nif omitted the default is 3 digits", GH_ParamAccess.item);
            pManager.AddTextParameter("Viewport Name", "V", "Name of the Rhino Viewport to capture\nleave empty for current view", GH_ParamAccess.item);
            pManager.AddTextParameter("Image size", "WxH", "Image size in pixels, WxH (ex. 1920x1080)\nleave empty for viewport resolution", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Scale", "S", "Image size multiplier\nex. 1920x1080 * 1.2 = 2304x1296", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Grid", "g", "Show Grid", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Axes", "A", "Show Axes", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Widget", "W", "Show XYZ Widget", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Capture", "C", "Activate Capture", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Trigger", "t", "Attach any value - if Capture is True, the value change will trigger a view capture", GH_ParamAccess.item);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
            pManager[9].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            /*
             original code by David Rutten
             adapted and extended by Alessio Erioli
             */

            // get active document
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

            // attempts to move this component at the top of the stack (to be executed last)
            // see https://www.grasshopper3d.com/xn/detail/2985220:Comment:975472
            ghDoc.ArrangeObject(this, GH_Arrange.MoveToFront);

            bool capture = false;
            DA.GetData("Capture", ref capture);

            if (!capture)
            {
                Message = "";
                return;
            }
            string dir = "", Name = "";
            if (!DA.GetData(0, ref dir)) return;
            if (!DA.GetData(1, ref Name)) return;
            if (string.IsNullOrWhiteSpace(dir)) return;
            if (string.IsNullOrWhiteSpace(Name)) return;

            // Make sure the directory path ends with a \
            if (!dir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                dir += System.IO.Path.DirectorySeparatorChar;

            // Do not create directories, only use existing ones.
            if (!System.IO.Directory.Exists(dir))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Directory does not exist - use only existing directories");
                return;
            }

            // split filename and extension
            string[] nSplit = Name.Split('.');
            // verify if extension is present
            if (nSplit.Length < 2 || nSplit[1] == null || nSplit[1] == "")
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Filename extension missing; use .jpg, .png or .bmp");
                return;
            }
            // verify if # number formatting is present
            string dFormat, cleanName = nSplit[0].Replace("#", "");
            int hashes = nSplit[0].Length - cleanName.Length;
            if (hashes == 0) dFormat = "D3";
            else dFormat = "D" + hashes.ToString();

            //Add an incremental number to the name
            Name = cleanName + "_{0}." + nSplit[1];

            // Assume index=0 for the first filename.
            string fileName = dir + string.Format(Name, 0.ToString(dFormat));

            // Try to increment the index until we find a name which doesn't exist yet.
            if (System.IO.File.Exists(fileName))
                for (int i = 1; i < int.MaxValue; i++)
                {
                    string localName = dir + string.Format(Name, i.ToString(dFormat));
                    if (localName == fileName)
                        return;

                    if (!System.IO.File.Exists(localName))
                    {
                        fileName = localName;
                        break;
                    }
                }
            string VP = "";
            DA.GetData(2, ref VP);
            if (string.IsNullOrWhiteSpace(VP)) VP = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.Name;

            Message = VP;

            int[] res = new int[1];
            Size sz;
            string size = "";
            DA.GetData(3, ref size);
            double scale = 1.0;
            DA.GetData(4, ref scale);

            Rhino.Display.RhinoView view = Rhino.RhinoDoc.ActiveDoc.Views.Find(VP, false);

            if (!string.IsNullOrWhiteSpace(size))
            {
                res = size.Split(new[] { 'x' }).Select(x => (int)(Convert.ToInt32(x) * scale)).ToArray();
            }
            else
            {
                res = new int[2];
                res[0] = (int)(view.Size.Width * scale);
                res[1] = (int)(view.Size.Height * scale);
            }

            sz = new Size(res[0], res[1]);

            Bitmap image;

            bool grid = false, axes = false, widget = false;
            DA.GetData(5, ref grid);
            DA.GetData(6, ref axes);
            DA.GetData(7, ref widget);

            //if (res.Length == 1)
            //    image = view.CaptureToBitmap(grid, widget, axes);
            //else
            image = view.CaptureToBitmap(new Size(res[0], res[1]), grid, widget, axes);

            image.Save(fileName);
            image.Dispose();

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
                return Resources.View_Capture_to_File_2_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3b24465b-8de9-4390-82b8-ddd17d5b1ff3"); }
        }
    }
}