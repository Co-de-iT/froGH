﻿using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Display;
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
            pManager.AddTextParameter("File Name", "F", "Filename and extension\n" +
                "use # to set the number of digits for progressive numbering\n" +
                "ex screencap####.png > screencap_0000.png\n" +
                "if # are omitted progressive numbering is off (files with same name+ext will be overwritten)", GH_ParamAccess.item);
            pManager.AddTextParameter("Viewport Name", "V", "Name of the Rhino Viewport to capture\nleave empty for active viewport", GH_ParamAccess.item);
            pManager.AddTextParameter("Image size", "WxH", "Image size in pixels, WxH (ex. 1920x1080)\nleave empty for viewport resolution", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Scale", "S", "Image size multiplier\nex. 1920x1080 * 1.2 = 2304x1296", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Grid", "g", "Show Grid", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Axes", "A", "Show Axes", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Widget", "W", "Show XYZ Widget", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Transparent Background", "T", "Transparent Background\n(only on supported file formats)", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Capture Z-buffer", "Z", "Capture Z-buffer - captures Z-buffer in a separate file\n" +
                "NOTE: Z-buffer is captured ONLY when the Image Size parameter (WxH) is empty (uses the viewport resolution)", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Capture", "C", "Activate Capture", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Trigger", "t", "Attach any value - if Capture is True, the value change will trigger a view capture", GH_ParamAccess.item);

            for (int i = 2; i <= 11; i++)
                pManager[i].Optional = true;
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
            //GH_Document ghDoc = null;// = Grasshopper.Instances.ActiveCanvas.Document;
            //try
            //{
            //    ghDoc = OnPingDocument();
            //}
            //catch
            //{
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Document not found");
            //}

            //if (ghDoc == null || !ghDoc.IsFilePathDefined) return;

            //// attempts to move this component at the top of the stack (to be executed last)
            //// see https://www.grasshopper3d.com/xn/detail/2985220:Comment:975472
            //ghDoc.ArrangeObject(this, GH_Arrange.MoveToFront);

            string dir = "", Name = "";
            if (!DA.GetData("File Path", ref dir)) return;
            if (!DA.GetData("File Name", ref Name)) return;
            if (string.IsNullOrWhiteSpace(dir)) return;
            if (string.IsNullOrWhiteSpace(Name)) return;

            // Make sure the directory path ends with a \
            if (!dir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                dir += System.IO.Path.DirectorySeparatorChar;

            // Do not create directories, only use existing ones.
            if (!System.IO.Directory.Exists(dir))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Directory does not exist - use only existing directories");
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
            string extension = nSplit[1];
            // verify if # number formatting is present
            string dFormat, numFormat, cleanName = nSplit[0].Replace("#", "");
            int hashes = nSplit[0].Length - cleanName.Length;
            if (hashes == 0)
            {
                dFormat = "";//"D3";
                numFormat = "";
            }
            else
            {
                dFormat = "D" + hashes.ToString();
                numFormat = "_{0}";
            }

            //Add an incremental number to the name
            string templateName = cleanName + numFormat + "." + extension; // nSplit[1];
            string templateZName = cleanName + numFormat + "_Z." + extension;

            // Assume index=0 for the first filename.
            string fileName = hashes == 0 ? templateName : FormatFilename(templateName, dFormat, 0);
            string fileNameFullPath = dir + fileName;
            string fileNameNumbered;
            int index = 0;

            // Try to increment the index until we find a name which doesn't exist yet.
            if (System.IO.File.Exists(fileNameFullPath) && hashes != 0)
                for (int i = 1; i < int.MaxValue; i++)
                {
                    fileNameNumbered = FormatFilename(templateName, dFormat, i);
                    string localName = dir + fileNameNumbered;
                    if (localName == fileNameFullPath)
                        return;

                    if (!System.IO.File.Exists(localName))
                    {
                        fileName = fileNameNumbered;
                        index = i;
                        break;
                    }
                }

            fileNameFullPath = dir + fileName;

            // Get viewport
            string VP = "";
            Rhino.Display.RhinoView view;
            DA.GetData("Viewport Name", ref VP);
            view = string.IsNullOrWhiteSpace(VP) ? Rhino.RhinoDoc.ActiveDoc.Views.ActiveView :
                Rhino.RhinoDoc.ActiveDoc.Views.Find(VP, false);

            int[] res = new int[1];
            string sizeText = "";
            DA.GetData("Image size", ref sizeText);
            double scale = 1.0;
            DA.GetData("Scale", ref scale);
            bool isViewportResolution = false;

            if (!string.IsNullOrWhiteSpace(sizeText))
            {
                res = sizeText.Split(new[] { 'x' }).Select(x => (int)(Convert.ToInt32(x) * scale)).ToArray();
            }
            else
            {
                res = new int[2];
                res[0] = (int)(view.Size.Width * scale);
                res[1] = (int)(view.Size.Height * scale);
                isViewportResolution = true;
            }


            bool grid = false, axes = false, widget = false, transpBg = false, captureZBuffer = false;
            DA.GetData("Grid", ref grid);
            DA.GetData("Axes", ref axes);
            DA.GetData("Widget", ref widget);
            DA.GetData("Transparent Background", ref transpBg);
            DA.GetData("Capture Z-buffer", ref captureZBuffer);

            if (captureZBuffer && !isViewportResolution)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Z-buffer won't be captured.\nTo capture Z-buffer, leave WxH parameter empty");

            // if extension is not png do not use transparent background
            if (!(string.Equals(extension, "png") || string.Equals(extension, "PNG"))) transpBg = false;

            Message = view.ActiveViewport.Name;

            bool capture = false;
            DA.GetData("Capture", ref capture);
            if (!capture)
            {
                Message = "";
                return;
            }
            ViewCapture vc = new ViewCapture
            {
                TransparentBackground = transpBg,
                Width = res[0],
                Height = res[1],
                DrawAxes = widget,
                DrawGrid = grid,
                DrawGridAxes = axes
            };
            Bitmap image;
            image = vc.CaptureToBitmap(view);
            //image = view.CaptureToBitmap(new Size(res[0], res[1]), grid, widget, axes);
            image.Save(fileNameFullPath);
            image.Dispose();
            if (captureZBuffer && isViewportResolution)
            {
                fileName = FormatFilename(templateZName, dFormat, index);
                fileNameFullPath = dir + fileName;
                ZBufferCapture zbcap = new ZBufferCapture(view.ActiveViewport);
                //zbcap.ShowCurves(true);
                //zbcap.ShowAnnotations(true);
                //zbcap.ShowIsocurves(true);
                //zbcap.ShowMeshWires(true);
                //zbcap.ShowPoints(true);
                image = zbcap.GrayscaleDib();
                image.Save(fileNameFullPath);
                image.Dispose();
                zbcap.Dispose();
            }

        }

        private string FormatFilename(string templateName, string dFormat, int i) => string.Format(templateName, i.ToString(dFormat));

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.ViewCapture2File_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("031DC183-383A-4E52-A190-1DAC0A8006FB"); }
        }
    }
}