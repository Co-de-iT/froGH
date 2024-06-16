using froGH.Properties;
using froGH.Utils;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Threading.Tasks;

namespace froGH
{
    /// <summary>
    /// First attempt at a Task-Capable component - see: https://developer.rhino3d.com/guides/grasshopper/programming-task-capable-component/
    /// Works, but for this case it does not boost performance as I hoped - so I kept the regular version
    /// </summary>
    public class SpaceFillingPolyGen_Task : GH_TaskCapableComponent<SpaceFillingPolyGen_Task.SolveResults>
    {
        private PolyHedra polyHedra;

        /// <summary>
        /// Initializes a new instance of the SpaceFillingPolyGen class.
        /// </summary>
        public SpaceFillingPolyGen_Task()
          : base("Space-Filling Polyhedra Generator - Parallel", "f_SPHGen_P",
              "Creates a space filling Polyhedron\nTask-Capable Component",
              "froGH", "Mesh-Create")
        {
            polyHedra = new PolyHedra();
            Params.ParameterSourcesChanged += new GH_ComponentParamServer.ParameterSourcesChangedEventHandler(ParamSourceChanged);
        }

        // this autolist method is from: https://discourse.mcneel.com/t/automatic-update-of-valuelist-only-when-connected/152879/6?u=ale2x72
        // works much better as it does not clog the solver with exceptions if a list of numercal values is connected
        private void ParamSourceChanged(object sender, GH_ParamServerEventArgs e)
        {
            if ((e.ParameterSide == GH_ParameterSide.Input) && (e.ParameterIndex == 1))
            {
                foreach (IGH_Param source in e.Parameter.Sources)
                {
                    if (source is Grasshopper.Kernel.Special.GH_ValueList)
                    {
                        Grasshopper.Kernel.Special.GH_ValueList vList = source as Grasshopper.Kernel.Special.GH_ValueList;

                        if (!vList.NickName.Equals("Polyhedron type"))
                        {
                            vList.ClearData();
                            vList.ListItems.Clear();
                            vList.NickName = "Polyhedron type";
                            var item1 = new Grasshopper.Kernel.Special.GH_ValueListItem("Bisymmetric Hendecahedron", "0");
                            var item2 = new Grasshopper.Kernel.Special.GH_ValueListItem("Rhombic Dodecahedron", "1");
                            var item3 = new Grasshopper.Kernel.Special.GH_ValueListItem("Elongated Dodecahedron", "2");
                            var item4 = new Grasshopper.Kernel.Special.GH_ValueListItem("Sphenoid Hendecahedron", "3");
                            var item5 = new Grasshopper.Kernel.Special.GH_ValueListItem("Truncated Octahedron", "4");
                            var item6 = new Grasshopper.Kernel.Special.GH_ValueListItem("Gyrobifastigium", "5");

                            vList.ListItems.Add(item1);
                            vList.ListItems.Add(item2);
                            vList.ListItems.Add(item3);
                            vList.ListItems.Add(item4);
                            vList.ListItems.Add(item5);
                            vList.ListItems.Add(item6);

                            vList.ListMode = Grasshopper.Kernel.Special.GH_ValueListMode.DropDown; // change this for a different mode (DropDown is the default)
                            vList.ExpireSolution(true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Base Plane", "P", "Base Plane for Polyhedron", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddIntegerParameter("Polyhedron type", "t",
                "0 Bysimmetric Hendecahedron" +
                "\n1 Rhombic Dodecahedron" +
                "\n2 Elongated Dodecahedron" +
                "\n3 Sphenoid Hendecahedron" +
                "\n4 Truncated Octahedron" +
                "\n5 Gyrobifastigium", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Scale", "s", "Polyhedron scale (center on base point)", GH_ParamAccess.item, 1);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Polyhedron as Mesh", GH_ParamAccess.item);
            pManager.AddCurveParameter("Face Polylines", "P", "Face contours as polylines", GH_ParamAccess.list);
        }

        public class SolveResults
        {
            public Mesh mesh { get; set; }
            public Polyline[] faceContours { get; set; }
        }

        public static SolveResults ComputeSpaceFillingPolyhedra(PolyHedra polyHedra, Plane plane, int type, double scale)
        {
            SolveResults result = new SolveResults();
            result.mesh = polyHedra.GetMesh(type);
            result.faceContours = polyHedra.GetFaceContours(type);

            Transform scaleT = Transform.Scale(new Point3d(), scale);
            Transform orient = Transform.PlaneToPlane(Plane.WorldXY, plane);

            result.mesh.Transform(scaleT);
            result.mesh.Transform(orient);
            foreach (Polyline f in result.faceContours)
            {
                f.Transform(scaleT);
                f.Transform(orient);
            }

            return result;
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // input data (in parallel mode)
            if (InPreSolve)
            {
                Plane P = new Plane();
                int type = 0;
                double scale = 1.0;
                DA.GetData(0, ref P);
                DA.GetData(1, ref type);
                DA.GetData(2, ref scale);

                if (!P.IsValid || P == null) P = Plane.WorldXY;
                if (scale == 0.0) scale = 1.0;

                // Queue up the task
                Task<SolveResults> task = Task.Run(() => ComputeSpaceFillingPolyhedra(polyHedra, P, type, scale), CancelToken);
                TaskList.Add(task);
                return;
            }

            // usual process if component is not in parallel computing mode
            // Basically all the "old" SolveInstance goes here (except for output)
            if (!GetSolveResults(DA, out SolveResults result))
            {
                Plane P = new Plane();
                int type = 0;
                double scale = 1.0;
                DA.GetData(0, ref P);
                DA.GetData(1, ref type);
                DA.GetData(2, ref scale);

                if (!P.IsValid || P == null) P = Plane.WorldXY;
                if (scale == 0.0) scale = 1.0;

                result = ComputeSpaceFillingPolyhedra(polyHedra, P, type, scale);
            }

            // output data
            if (result != null)
            {
                DA.SetData(0, result.mesh);
                DA.SetDataList(1, result.faceContours);
            }
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
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
                return Resources.SpaceFillingPolyhedra_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("DD75BE28-0A9A-497B-A5BA-CB90F13E0F1E"); }
        }
    }
}