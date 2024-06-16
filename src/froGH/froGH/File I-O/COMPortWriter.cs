using froGH.Properties;
using Grasshopper.Kernel;
using System;

namespace froGH
{
    public class COMPortWriter : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the COMPortWriter class.
        /// </summary>
        public COMPortWriter()
          : base("COM Port Writer", "f_COMpW",
              "Sends data to a COM port",
              "froGH", "File I-O")
        {
            Params.ParameterSourcesChanged += new GH_ComponentParamServer.ParameterSourcesChangedEventHandler(ParamSourceChanged);
        }

        // this autolist method is from: https://discourse.mcneel.com/t/automatic-update-of-valuelist-only-when-connected/152879/6?u=ale2x72
        // works much better as it does not clog the solver with exceptions if a list of numercal values is connected
        private void ParamSourceChanged(object sender, GH_ParamServerEventArgs e)
        {
            if ((e.ParameterSide == GH_ParameterSide.Input) && (e.ParameterIndex == 3))
            {
                foreach (IGH_Param source in e.Parameter.Sources)
                {
                    if (source is Grasshopper.Kernel.Special.GH_ValueList)
                    {
                        Grasshopper.Kernel.Special.GH_ValueList vList = source as Grasshopper.Kernel.Special.GH_ValueList;
                        string listNickName = "parity";
                        if (!vList.NickName.Equals(listNickName))
                        {
                            vList.ClearData();
                            vList.ListItems.Clear();
                            vList.NickName = listNickName;
                            var item1 = new Grasshopper.Kernel.Special.GH_ValueListItem("Even", "0");
                            var item2 = new Grasshopper.Kernel.Special.GH_ValueListItem("Mark", "1");
                            var item3 = new Grasshopper.Kernel.Special.GH_ValueListItem("None", "2");
                            var item4 = new Grasshopper.Kernel.Special.GH_ValueListItem("Odd", "3");
                            var item5 = new Grasshopper.Kernel.Special.GH_ValueListItem("Space", "4");
                            vList.ListItems.Add(item1);
                            vList.ListItems.Add(item2);
                            vList.ListItems.Add(item3);
                            vList.ListItems.Add(item4);
                            vList.ListItems.Add(item5);

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
            pManager.AddTextParameter("String", "S", "The string to send", GH_ParamAccess.item);
            pManager.AddTextParameter("Port", "P", "COM port for data output", GH_ParamAccess.item);
            pManager.AddIntegerParameter("BAUD Rate", "B", "BAUD Rate", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Parity", "P", "Parity\n0 Even, 1 Mark, 2 None, 3 Odd, 4 Space\ninsert a Value List for Autolist", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Data Bits", "b", "Data packages bits", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Send trigger", "s", "Attach a toggle or button - set to true to send data", GH_ParamAccess.item, false);
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output Message", "out", "Output Message", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string S = "";
            if (!DA.GetData(0, ref S)) return;
            string port = "";
            if (!DA.GetData(1, ref port)) return;
            int B = 0, parityType = 0, bitRate = 0;
            if (!DA.GetData(2, ref B)) return;
            if (!DA.GetData(3, ref parityType)) return;
            if (!DA.GetData(4, ref bitRate)) return;

            bool save = false;
            DA.GetData(5, ref save);

            string message = "";

            string eol = Environment.NewLine;

            // set parity
            System.IO.Ports.Parity parity = new System.IO.Ports.Parity();
            switch (parityType)
            {
                case 0:
                    parity = System.IO.Ports.Parity.Even;
                    break;
                case 1:
                    parity = System.IO.Ports.Parity.Mark;
                    break;
                case 2:
                    parity = System.IO.Ports.Parity.None;
                    break;
                case 3:
                    parity = System.IO.Ports.Parity.Odd;
                    break;
                case 4:
                    parity = System.IO.Ports.Parity.Space;
                    break;
                default:
                    parity = System.IO.Ports.Parity.None;
                    break;
            }

            // initialize port
            System.IO.Ports.SerialPort serialPort = new System.IO.Ports.SerialPort(port, B, parity, bitRate);

            if (save)
            {
                message = "";
                serialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler( (sender, e) =>
                { message = $"data sent to {serialPort.PortName}"; });
                serialPort.ErrorReceived += new System.IO.Ports.SerialErrorReceivedEventHandler((sender, e) =>
                { message = $"something's wrong on {serialPort.PortName}: {e}"; });
                serialPort.Open();
                serialPort.Write(S);
                serialPort.Close();
            }
            else
            {

                message = "press send button to send data to:" + eol + serialPort.PortName + " " + serialPort.BaudRate + " " + serialPort.Parity + " " + serialPort.DataBits;
            }

            DA.SetData(0, message);
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
                return Resources.WriteToCOMPort_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ac113b34-106a-4d8e-a3a0-768e6e0e8f39"); }
        }
    }
}