using froGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;

namespace froGH
{
    public class ObjectLayer : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ObjectLayer class.
        /// </summary>
        public ObjectLayer()
          : base("ObjectLayer", "f_OLay",
              "Gets the Layer of a Rhino referenced object",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Object", "O", "Object referenced from Rhino", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Layer", "L", "Object Layer", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // code for accessing GUID by Fraser Greenroyd
            // https://frasergreenroyd.com/how-to-access-geometry-guids-in-grasshopper-components/
            IGH_GeometricGoo geom = null;
            if(!DA.GetData(0, ref geom)) return;

            Guid id = geom.ReferenceID;
            
            Rhino.DocObjects.ObjectAttributes objAtt = Rhino.RhinoDoc.ActiveDoc.Objects.Find(id).Attributes;

            int layerIndex = objAtt.LayerIndex;

            // Layers[layerIndex].Name only returns the layer name without path
            // FullPath returns the full path in the format "TopLayer::SubLayer::SubSubLayer"
            string layer = Rhino.RhinoDoc.ActiveDoc.Layers[layerIndex].FullPath;
            

            DA.SetData(0, layer);
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
                return Resources.ObjectLayer_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7cca1ed4-a7c5-4342-9e19-e6de46b41b4c"); }
        }
    }
}