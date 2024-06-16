using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using froGH.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;

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
            pManager.AddIntegerParameter("Depth Level", "D", "Depth level for search\n0 for current Directory only, -1 for full recursive search", GH_ParamAccess.item, 0);

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Subdirectories", "D", "Directory tree", GH_ParamAccess.tree);
            pManager.AddTextParameter("Files", "F", "Files tree", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string P = null;
            if (!DA.GetData(0, ref P)) return;

            if (!Directory.Exists(P))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Directory does not exist");

            int depthLevel = 0;
            DA.GetData(1, ref depthLevel);

            // if missing, add the last \ character
            if (P[P.Length - 1] != '\\') P += '\\';

            DataTree<string> folderTree = new DataTree<string>();
            DataTree<string> filesTree = new DataTree<string>();

            GH_Path path = new GH_Path(0);
            // if depthLevel is 0, add subdirs to the Dir output
            if(depthLevel == 0) folderTree.AddRange(Directory.GetDirectories(P).Select(s => s.Remove(0, P.Length)).ToList(), path);
            // else add the root dir
            else folderTree.Add(P, path);

            TreeDirAndFiles(ref folderTree, ref filesTree, path, P, depthLevel);

            DA.SetDataTree(0, folderTree);
            DA.SetDataTree(1, filesTree);
        }

        private void TreeDirAndFiles(ref DataTree<string> folderTree, ref DataTree<string> filesTree, GH_Path path, string currentDir, int level)
        {
            // Add files in current dir
            List<string> files = Directory.GetFiles(currentDir).Select(s => s.Remove(0, currentDir.Length)).ToList();
            filesTree.AddRange(files, path);

            // check for subdirs
            List<string> directories = Directory.GetDirectories(currentDir).ToList();

            // return condition
            if (directories.Count == 0 || (level != -1 && path.Length > level)) return;

            GH_Path tPath;
            for (int i = 0; i < directories.Count; i++)
            {
                tPath = path.AppendElement(i);
                string dir = directories[i];

                folderTree.Add(dir.Remove(0, currentDir.Length), tPath);
                TreeDirAndFiles(ref folderTree, ref filesTree, tPath, directories[i] + "\\", level);
            }
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
                return Resources.DirectoryReader_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6B316165-8448-41DF-A7D7-BC2A5EC77116"); }
        }
    }
}