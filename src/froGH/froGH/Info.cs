using froGH.Properties;
using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.Reflection;

namespace froGH
{
    public class Info : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "froGH";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Resources.froGH_GH;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "It should have been for GH but... spelling\n froGH is a sparse collection of (in)utilities for Grasshopper";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("d2580c41-5c48-4997-87c5-b6333b5e21d7");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Co-de-iT";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "info@co-de-it.com";
            }
        }
        public override string AssemblyVersion
        {
            // change it in AssemblyInfo.cs - this should sync it automatically
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var assemblyName = new AssemblyName(assembly.FullName);
                return assemblyName.Version.ToString();
            }
            //get
            //{
            //    //Return a string representing the version.
            //    return "2.2.14"; 
            //}
        }

        public override string Version => AssemblyVersion;
    }

    /// <summary>
    /// Add Category icon
    /// </summary>
    public class froGHCategoryIcon : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Grasshopper.Instances.ComponentServer.AddCategoryIcon("froGH", Resources.froGH_category);
            Grasshopper.Instances.ComponentServer.AddCategorySymbolName("froGH", 'f');
            return GH_LoadingInstruction.Proceed;
        }
    }
}
