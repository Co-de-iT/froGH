using froGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Drawing;

namespace froGH
{
    public class BakeAttributesEnhanced : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BakeAttributesEnhanced class.
        /// </summary>
        public BakeAttributesEnhanced()
          : base("Bake Attributes Enhanced", "f_Bake++",
              "Bakes objects with attributes (such as color, material, layer, object name, isocurve density)\nOption for baking into groups",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry (object) for baking", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Object name", GH_ParamAccess.item);
            pManager.AddTextParameter("Layer Name", "L", "Destination Layer Name", GH_ParamAccess.item);
            pManager.AddColourParameter("Color", "C", "Object Color", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "M", "Rendering Material", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Group number", "gN", "Group Number", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Bake in Groups", "bG", "Bake object in groups according to the group number for each object\nIf a group exists, object will be added to that group", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Bake", "B", "Performs bake operation", GH_ParamAccess.item, false);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;

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
             Original Version Written by Giulio Piacentino - 2010 11 21 - for Grasshopper 0.8.002
             Enhanced by Co-de-iT (Alessio) - now bakes on chosen layer(s) and in groups
             */
            GeometryBase obj = null;
            if (!DA.GetData(0, ref obj)) return;

            string name = "";
            DA.GetData(1, ref name);
            string layer = "";
            DA.GetData(2, ref layer);

            Color color = new Color();
            DA.GetData(3, ref color);
            Object material = new Object();
            DA.GetData(4, ref material);
            int group_n = 0;
            DA.GetData(5, ref group_n);

            bool group = false;
            DA.GetData(6, ref group);
            bool bake_iT = false;
            DA.GetData(7, ref bake_iT);

            if (!bake_iT) return;

            //Make new attribute to set name
            Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();

            //Set object name
            if (!string.IsNullOrEmpty(name))
            {
                att.Name = name;
            }

            //Set color
            if (!color.IsEmpty)
            {
                att.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject; //Make the color type "by object"
                att.ObjectColor = color;

                att.PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromObject; //Make the plot color type "by object"
                att.PlotColor = color;
            }

            // Set group

            if (group)
            {
                Rhino.RhinoDoc.ActiveDoc.Groups.Add(Convert.ToString(group_n));
                att.AddToGroup(group_n);

            }

            //Set layer
            if (!string.IsNullOrEmpty(layer) && Rhino.DocObjects.Layer.IsValidName(layer))
            {
                //Get the current layer index
                Rhino.DocObjects.Tables.LayerTable layerTable = Rhino.RhinoDoc.ActiveDoc.Layers;
                //int layerIndex = layerTable.Find(layer, true);
                int layerIndex = layerTable.FindByFullPath(layer, -1);

                if (layerIndex < 0) //This layer does not exist, we add it
                {
                    Rhino.DocObjects.Layer onlayer = new Rhino.DocObjects.Layer(); //Make a new layer
                    onlayer.Name = layer;
                    onlayer.Color = Color.Gainsboro; // sets new layer color - future dev: choose new layers color

                    layerIndex = layerTable.Add(onlayer); //Add the layer to the layer table
                    if (layerIndex > -1) //We managed to add the layer!
                    {
                        att.LayerIndex = layerIndex;
                        //Print("Added new layer to the document at position " + layerIndex + " named " + layer + ". ");
                    }
                    //else
                    //Print("Layer did not add. Try cleaning up your layers."); //This never happened to me.
                }
                else
                    att.LayerIndex = layerIndex; //We simply add to the existing layer
            }


            //Set plotweight
            //if (pWidth > 0)
            //{
            //    att.PlotWeightSource = Rhino.DocObjects.ObjectPlotWeightSource.PlotWeightFromObject;
            //    att.PlotWeight = pWidth;
            //}


            //Set material

            bool materialByName = !string.IsNullOrEmpty(material as string);
            Rhino.Display.DisplayMaterial inMaterial;
            if (material is GH_Material)
            {
                GH_Material gMat = material as GH_Material;
                inMaterial = gMat.Value as Rhino.Display.DisplayMaterial;
            }
            else
                inMaterial = material as Rhino.Display.DisplayMaterial;
            if (material is Color)
            {
                inMaterial = new Rhino.Display.DisplayMaterial((Color)material);
            }
            if (material != null && inMaterial == null && !materialByName)
            {
                if (!(material is string))
                {
                    try //We also resort to try with IConvertible
                    {
                        inMaterial = (Rhino.Display.DisplayMaterial)Convert.ChangeType(material, typeof(Rhino.Display.DisplayMaterial));
                    }
                    catch (InvalidCastException)
                    {
                    }
                }
            }
            if (inMaterial != null || materialByName)
            {
                string matName;

                if (!materialByName)
                {
                    matName = string.Format("D:{0}-E:{1}-S:{2},{3}-T:{4}",
                      Format(inMaterial.Diffuse),
                      Format(inMaterial.Emission),
                      Format(inMaterial.Specular),
                      inMaterial.Shine.ToString(),
                      inMaterial.Transparency.ToString()
                      );
                }
                else
                {
                    matName = (string)material;
                }

                int materialIndex = Rhino.RhinoDoc.ActiveDoc.Materials.Find(matName, true);
                if (materialIndex < 0 && !materialByName) //Material does not exist and we have its specs
                {
                    materialIndex = Rhino.RhinoDoc.ActiveDoc.Materials.Add(); //Let's add it
                    if (materialIndex > -1)
                    {
                        //Print("Added new material at position " + materialIndex + " named \"" + matName + "\". ");
                        Rhino.DocObjects.Material m = Rhino.RhinoDoc.ActiveDoc.Materials[materialIndex];
                        m.Name = matName;
                        m.DiffuseColor = inMaterial.Diffuse;
                        m.EmissionColor = inMaterial.Emission;
                        //m.ReflectionColor = inMaterial.Specular;
                        m.SpecularColor = inMaterial.Specular;
                        m.Shine = inMaterial.Shine;
                        m.Transparency = inMaterial.Transparency;
                        //m.TransparentColor = no equivalent

                        m.CommitChanges();

                        att.MaterialSource = Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject;
                        att.MaterialIndex = materialIndex;
                    }
                    else
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Material did not add. Try cleaning up your materials."); //This never happened to me.
                }
                else if (materialIndex < 0 && materialByName) //Material does not exist and we do not have its specs. We do nothing
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Warning: material name not found. I cannot set the source to this material name. Add a material with name: " + matName);
                }
                else
                {
                    //If this material exists, we do not replace it!
                    att.MaterialSource = Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject;
                    att.MaterialIndex = materialIndex;
                }
            }

            //Set wire density
            //if (wires == -1 || wires > 0)
            //{
            //    att.WireDensity = wires;
            //}


            //Bake to the right type of object
            switch (obj.ObjectType)
            {
                case Rhino.DocObjects.ObjectType.Brep:
                    Rhino.RhinoDoc.ActiveDoc.Objects.AddBrep(obj as Brep, att);
                    break;
                case Rhino.DocObjects.ObjectType.Curve:
                    Rhino.RhinoDoc.ActiveDoc.Objects.AddCurve(obj as Curve, att);
                    break;
                case Rhino.DocObjects.ObjectType.Point:
                    Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint((obj as Rhino.Geometry.Point).Location, att);
                    break;
                case Rhino.DocObjects.ObjectType.Surface:
                    Rhino.RhinoDoc.ActiveDoc.Objects.AddSurface(obj as Surface, att);
                    break;
                case Rhino.DocObjects.ObjectType.Mesh:
                    Rhino.RhinoDoc.ActiveDoc.Objects.AddMesh(obj as Mesh, att);
                    break;
                case Rhino.DocObjects.ObjectType.Extrusion:
                    typeof(Rhino.DocObjects.Tables.ObjectTable).InvokeMember("AddExtrusion", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod, null, Rhino.RhinoDoc.ActiveDoc.Objects, new object[] { obj, att });
                    break;
                case Rhino.DocObjects.ObjectType.PointSet:
                    Rhino.RhinoDoc.ActiveDoc.Objects.AddPointCloud(obj as Rhino.Geometry.PointCloud, att); //This is a speculative entry
                    break;
                default:
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The script does not know how to handle this type of geometry: " + obj.GetType().FullName);
                    break;
            }

        }

        static string Format(Color c)
        {
            return (new System.Text.StringBuilder("A")).Append(c.A.ToString()).Append("R").Append(c.R.ToString()).Append("G")
              .Append(c.G.ToString()).Append("B").Append(c.B.ToString()).ToString();
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.quinary; }
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
                return Resources.Bake_att____4_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("fdf420a6-42d7-4963-9c42-ea8ff7aa2656"); }
        }
    }
}