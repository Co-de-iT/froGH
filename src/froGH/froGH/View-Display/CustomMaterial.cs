using System;
using froGH.Properties;
using Grasshopper.Kernel;
using System.Drawing;
using Grasshopper.Kernel.Types;

namespace froGH
{
    public class CustomMaterial : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CustomMaterial class.
        /// </summary>
        public CustomMaterial()
          : base("Custom Material", "f_MatC",
              "Create Custom Render Materials",
              "froGH", "View/Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("Diffuse Color", "D", "Diffuse Color", GH_ParamAccess.item, Color.Black);
            pManager.AddColourParameter("Ambient Color", "A", "Ambient Color", GH_ParamAccess.item, Color.Black);
            pManager.AddColourParameter("Emission Color", "E", "", GH_ParamAccess.item, Color.Black);
            pManager.AddColourParameter("Reflection Color", "R", "", GH_ParamAccess.item, Color.White);
            pManager.AddNumberParameter("Reflectivity", "r", "Reflectivity\n0 - matte, 1 - mirror", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Reflection Glossiness", "rG", "Reflection Glossiness\n0 - polished, 1 - fully diffused", GH_ParamAccess.item, 0.25);
            pManager.AddNumberParameter("Refraction Glossiness", "rfG", "Refraction Glossiness\n0 - polished, 1 - fully diffused", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Fresnel Reflections", "F", "Enable/Disable Fresnel reflections", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Index of Refraction", "IoR", "Index of Refraction for Fresnel Reflections", GH_ParamAccess.item, 1.2);
            pManager.AddNumberParameter("Transparency", "t", "Transparency\n0 - opaque, 1- transparent", GH_ParamAccess.item, 0.0);

            for (int i = 0; i <= 9; i++) pManager[i].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Render Material", "M", "The Custom Render Material", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Color Color_Diffuse = Color.Black,
                Color_Ambient = Color.Black,
                Color_Emission = Color.Black,
                Color_Reflection = Color.Black;

            bool FresnelReflections = false;

            DA.GetData(0, ref Color_Diffuse);
            DA.GetData(1, ref Color_Ambient);
            DA.GetData(2, ref Color_Emission);
            DA.GetData(3, ref Color_Reflection);

            double Reflectivity = 0,
                ReflectionGlossiness = 0,
                RefractionGlossiness = 0,
                IoR = 0,
                Transparency = 0;

            DA.GetData(4, ref Reflectivity);
            DA.GetData(5, ref ReflectionGlossiness);
            DA.GetData(6, ref RefractionGlossiness);
            DA.GetData(7, ref FresnelReflections);
            DA.GetData(8, ref IoR);
            DA.GetData(9, ref Transparency);

            Reflectivity = Math.Min(Math.Max(Reflectivity, 0), 1);
            ReflectionGlossiness = Math.Min(Math.Max(ReflectionGlossiness, 0), 1);
            Transparency = Math.Min(Math.Max(Transparency, 0), 1);

            Rhino.DocObjects.Material material = new Rhino.DocObjects.Material();

            material.FresnelReflections = FresnelReflections;
            if (FresnelReflections)
                material.FresnelIndexOfRefraction = IoR;
            else material.IndexOfRefraction = IoR;
            material.DiffuseColor = Color_Diffuse;
            material.AmbientColor = Color_Ambient;
            material.EmissionColor = Color_Emission;
            material.ReflectionColor = Color_Reflection;
            material.Reflectivity = Reflectivity;
            material.RefractionGlossiness = RefractionGlossiness;
            material.ReflectionGlossiness = ReflectionGlossiness;
            material.Transparency = Transparency;

            var renderMaterial = Rhino.Render.RenderMaterial.CreateBasicMaterial(material, Rhino.RhinoDoc.ActiveDoc);
            DA.SetData(0, new GH_Material(renderMaterial));
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
                return Resources.CustomMaterialPreview_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2ed3aabf-9738-405e-a1d6-a61f51885b1a"); }
        }
    }
}