using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Newtonsoft.Json;
using PlcOpenBuilder; 

namespace GenCamFB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Cam Json | *.json;*.JSON";
            ofd.ShowDialog();
            if (ofd.FileName.ToLower().Contains(".json"))
            {
                processJson(ofd.FileName);
                this.JSONFile.Text = ofd.FileName;
            }
        }

        private void processJson(string fileName)
        {
            StreamReader sr = new StreamReader(fileName);
            string json = sr.ReadToEnd();
            if (json != null)
            {
            #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                dynamic cam = JsonConvert.DeserializeObject(json);
                dynamic camProfile = JsonConvert.DeserializeObject(cam.camBuilderProfileData.ToString());
                dynamic profile = JsonConvert.DeserializeObject(camProfile.ProfileData.ToString());
                foreach (dynamic variable in profile.Variables)
                {
                    Variables CamVars; 
                    CamVars.Value = variable.Value.ToString();
                    CamVars.Name = variable.Name.ToString();
                    CamVars.Type = "Real"; 
                    _variables.Add(CamVars);
                }
                foreach (dynamic camSegment in cam.segments)
                {
                    Segment segment = new Segment();
                    foreach (string key in SegmentKeys.Keys)
                    {
                        SegmentData data; 
                        data.Value = camSegment[key];
                        data.isFormula = false;
                        data.Formula = null;
                        segment.PointData.Add(key, data); 
                    }
                    segment.MotionLaw = camSegment.lawType.ToString();
                    segment.SyncType = camSegment.syncType.ToString();
            
                    dynamic camFormula = JsonConvert.DeserializeObject(camSegment.camBuilderSegmentData.ToString());
                    foreach (string key in _formulaKeys)
                    {
                        if (camFormula[key] != null)
                        {
                           SegmentData data = segment.PointData[_keyTranslator[key]];
                            data.isFormula = true;
                            data.Formula = camFormula[key].ToString().Trim('=');
                            segment.PointData.Remove(_keyTranslator[key]);
                            segment.PointData.Add(_keyTranslator[key], data); 

                        }
                    }
                    _segments.Add(segment);
                }
            #pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "PLC Open | *.xml; *.XML";
            sfd.ShowDialog();
            if (sfd.FileName.ToLower().Contains(".xml"))
            {
                CreateFB(sfd.FileName);
                this.FBFile.Text = sfd.FileName;
            }
        }

        private void CreateFB(string fileName)
        {
            //Get what you named the XML file to name the FB 
            string name = fileName.Split('\\').Last();
            name = name.Split('.').First(); 
            PlcOpen fb = new PlcOpen("Bosch Rexroth", "CtrlX Core", "1.00", name);
            fb.AddPou(name, POUType.Function_Block);
            fb.AddInput(name, "bExecute", "BOOL");
            fb.AddInput(name, "sAxisName", "string"); 
            fb.AddInput(name, "sProfileName", "string");
            foreach (Variables variable in _variables)
            {
                fb.AddInput(name,variable.Name,variable.Type);
                fb.InitialValue(name,variable.Name,variable.Value);
            }
            fb.AddOutput(name, "bDone", "BOOL");
            fb.AddOutput(name, "bError", "BOOL"); 
            foreach (Variables variable in _locVar) 
            {
                fb.AddVar(name, variable.Name, variable.Type); 
            }
            fb.setStringLength(name, "_nodeName", "100"); 
            foreach(Variables variable in _localConst)
            {
                fb.AddConstVar(name, variable.Name, variable.Type);
                fb.InitialValue(name, variable.Name, variable.Value);
            }
            StBuilder stBuilder = new StBuilder();
            foreach (Segment segment in _segments)
            {
                stBuilder.addSegment(segment);
            }
            fb.CreateST(name, stBuilder.ST());
            fb.SaveDoc(fileName); 
        }

        private  List<Segment> _segments = new List<Segment>();
        private  List<Variables> _variables = new List<Variables>();
        private List<Variables> _locVar = new List<Variables>() { new Variables { Name = "_builder", Type = "flatbuffers.FlatBufferBuilder", Value = null },
                                                                    new Variables { Name = "_motion_sync_fbtypes_AxsCfgSingleFlexProfile", Type = "motion_sync_fbtypes_AxsCfgSingleFlexProfile", Value = null},
                                                                    new Variables { Name = "_motion_sync_fbtypes_CfgFlexProfileSegment", Type ="motion_sync_fbtypes_CfgFlexProfileSegment", Value = null },
                                                                    new Variables { Name = "_write", Type = "DL_WriteNodeValue", Value = null },
                                                                    new Variables { Name = "_create", Type = "DL_CreateNodeValue" , Value = null},
                                                                    new Variables { Name = "_nodeValue_profile", Type = "CXA_Datalayer.DL_NodeValue", Value = null},
                                                                    new Variables { Name = "_nodeValue_validate", Type = "CXA_Datalayer.DL_NodeValue", Value = null},
                                                                    new Variables { Name = "_nodeValue_dummy", Type = "CXA_Datalayer.DL_NodeValue", Value = null },
                                                                    new Variables { Name = "segments", Type = "ARRAY[0..15] OF UDINT", Value = null },
                                                                    new Variables { Name = "_camBuilderProfileData_Offset", Type = "UDINT", Value = null},
                                                                    new Variables { Name = "_name_Offset", Type = "UDINT", Value = null },
                                                                    new Variables { Name = "_segments_Offset", Type = "UDINT", Value = null},
                                                                    new Variables { Name = "_execute", Type = "BOOL", Value = null},
                                                                    new Variables { Name = "_state", Type = "UINT", Value = null},
                                                                    new Variables { Name = "_nodeName", Type = "string", Value = null} };
        private List<Variables> _localConst = new List<Variables>() { new Variables { Name = "IDLE" , Type = "UINT", Value = "0" },
                                                                     new Variables { Name = "BUILD_BUFFER", Type = "UINT", Value ="1"},
                                                                     new Variables {Name = "UPDATE_PROFILE", Type = "UINT", Value = "2"},
                                                                     new Variables {Name = "SET_NODE_VALIDATE", Type = "UINT", Value = "3"},
                                                                     new Variables {Name = "CREATE_PROFILE", Type = "UINT", Value = "4"},
                                                                     new Variables {Name = "DONE", Type = "UINT", Value = "5"},
                                                                     new Variables {Name = "ERROR", Type = "UINT", Value = "6"}}; 
        private static readonly List<string> _formulaKeys = new List<string> { "a0Formula", "a1Formula", "AmaxFormula", "j0Formula", "j1Formula", "JmaxFormula", "MasterRangeFormula", "gainFormula", "v0Formula", "v1Formula", "VmaxFormula" };
        private static readonly Dictionary<string, string> _keyTranslator = new Dictionary<string, string>() { { "a0Formula", "a0" }, {"a1Formula","a1"},/*{"AmaxFormula","limA0"},*/{"j0Formula", "j0" },
                                                                                                {"j1Formula", "j1" },/*{"JmaxFormula","limj0" },*/{"MasterRangeFormula","range"},{"gainFormula","gain"},
                                                                                                {"v0Formula","v0"},{"v1Formula","v1"},{"VmaxFormula","limV"} };
    }
}