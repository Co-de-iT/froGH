using froGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Drawing;

namespace froGH
{
    public class BitmapFromColorTree : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BitmapFromColorTree class.
        /// </summary>
        public BitmapFromColorTree()
          : base("Create Bitmap From Color Tree", "f_BMPfCT",
              "Create a Bitmap file from a color table, given as a DataTree\n" +
                "- X size (in pixels) is the n. of Tree Branches * Px\n" +
                "- Y size (in pixels) is the Branch list length * Py\n" +
                "All branches must have the same list length",
              "froGH", "View/Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //pManager.AddTextParameter("File Path", "P", "File absolute path", GH_ParamAccess.item);
            //pManager.AddTextParameter("File Name", "F", "Filename and extension - for example screencap.png\nvalid extensions: .bmp, .jpg, .png, .tif", GH_ParamAccess.item);
            pManager.AddColourParameter("Colors", "C", "", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Pixels X", "Px", "How many pixels per color value in the X direction", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Pixels Y", "Py", "How many pixels per color value in the Y direction", GH_ParamAccess.item, 1);
            //pManager.AddBooleanParameter("Save", "S", "Save image", GH_ParamAccess.item, false);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            //pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmap Image", "B", "The bitmap image", GH_ParamAccess.item);
            //pManager.AddBooleanParameter("Saved File status", "S", "True if File was saved successfully", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //string path = "", name = "";
            //if (!DA.GetData("File Path", ref path)) return;
            //if (!DA.GetData("File Name", ref name)) return;

            GH_Structure<GH_Colour> colors = new GH_Structure<GH_Colour>();
            if (!DA.GetDataTree(0, out colors)) return;

            int Px = 1, Py = 1;
            DA.GetData("Pixels X", ref Px);
            DA.GetData("Pixels Y", ref Py);

            if (Px <= 0 || Py <= 0)
            {
                Px = 1;
                Py = 1;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Pixel sizes set to 1 (values cannot be zero or negative)");
            }

            //bool save = false;
            //DA.GetData("Save", ref save);

            int resX = Px, resY = Py;
            int sizeX = colors.Branches.Count * resX;
            int sizeY = colors.Branches[0].Count * resY;

            Bitmap image = new Bitmap(sizeX, sizeY);

            for (int i = 0; i < colors.Branches.Count; i++)
                for (int j = 0; j < colors.Branches[i].Count; j++)
                    for (int k = 0; k < resX; k++)
                        for (int p = 0; p < resY; p++)
                            image.SetPixel(i * resX + k, sizeY - 1 - j * resY - p, colors.Branches[i][j].Value);

            DA.SetData(0, image);

            //if (!save) return;

            //// Make sure the directory path ends with a \
            //if (!path.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            //    path += System.IO.Path.DirectorySeparatorChar;

            //// Create directory if it doesn't exist
            //if (!System.IO.Directory.Exists(path))
            //    System.IO.Directory.CreateDirectory(path);

            //bool result = false;

            //try
            //{
            //    image.Save(path + name);
            //    result = true;
            //}
            //catch (Exception e)
            //{
            //    throw (e);
            //}

            //image.Dispose();

            //DA.SetData(1, result);
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
                return Resources.BitmapFromColorTree_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("D1629334-01DB-4317-A3A2-7D12A94D023B"); }
        }
    }
}