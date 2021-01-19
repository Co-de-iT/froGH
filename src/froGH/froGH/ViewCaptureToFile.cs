using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

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
            pManager.AddTextParameter("File Path", "P", "File absolute path", GH_ParamAccess.item);
            pManager.AddTextParameter("File Name", "F", "Filename and extension", GH_ParamAccess.item);
            pManager.AddTextParameter("Viewport Name", "V", "Name of the Rhino Viewport to capture", GH_ParamAccess.item);
            pManager.AddTextParameter("Image size", "S", "Image size in pixels, WxH (ex. 1920x1080)\nleave empty for viewport resolution", GH_ParamAccess.item, "");
            pManager.AddBooleanParameter("Grid", "g", "Show Grid", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Axes", "A", "Show Axes", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Widget", "W", "Show XYZ Widget", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Capture", "c", "Activate Capture", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Trigger", "T", "Attach any value - if Capture is True, the value change will trigger a view capture", GH_ParamAccess.item);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
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

            //Message = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.Name;

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

            // Make sure the directory ends with a \
            if (dir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                dir += System.IO.Path.DirectorySeparatorChar;

            // Do not create directories, only use existing ones.
            if (!System.IO.Directory.Exists(dir))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Directory does not exist - use only existing directories");
                return;
            }

            //Add an incremental number to the name
            string[] nSplit = Name.Split('.');
            Name = nSplit[0] + "_{0}." + nSplit[1];

            // Assume index=0 for the first filename.
            string fileName = dir + string.Format(Name, 0.ToString("D3"));

            // Try to increment the index until we find a name which doesn't exist yet.
            if (System.IO.File.Exists(fileName))
                for (int i = 1; i < int.MaxValue; i++)
                {
                    string localName = dir + string.Format(Name, i.ToString("D3"));
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

            if (!string.IsNullOrWhiteSpace(size))
            {
                res = size.Split(new[] { 'x' }).Select(x => Convert.ToInt32(x)).ToArray();
                sz = new Size(res[0], res[1]);
            }



            Rhino.Display.RhinoView view = Rhino.RhinoDoc.ActiveDoc.Views.Find(VP, false);
            Bitmap image;

            bool grid = false, axes = false, widget = false;
            DA.GetData(4, ref grid);
            DA.GetData(5, ref axes);
            DA.GetData(6, ref widget);

            if (res.Length == 1)
                image = view.CaptureToBitmap(grid, widget, axes);
            else
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