using System.IO;
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
                processJson(ofd.FileName);
        }

        private void processJson(string fileName)
        {
            StreamReader sr = new StreamReader(fileName);
            string json = sr.ReadToEnd();
            if (json != null)
            {
            #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                dynamic cam = JsonConvert.DeserializeObject(json);
                foreach (dynamic camSegment in cam.segments)
                {
                    Segment segment = new Segment();
                    foreach (string key in _segmentKeys)
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

        }
        private  List<Segment> _segments = new List<Segment>();
        private static readonly List<string> _segmentKeys = new List<string> { "gain", "range", "v0", "v1", "a0", "a1", "j0", "j1", "limA0", "limA1", "limV", "limJ0", "limJ1", "lambda", "master" };
        private static readonly List<string> _formulaKeys = new List<string> { "a0Formula", "a1Formula", "AmaxFormula", "j0Formula", "j1Formula", "JmaxFormula", "MasterRangeFormula", "gainFormula", "v0Formula", "v1Formula", "VmaxFormula" };
        private static readonly Dictionary<string, string> _keyTranslator = new Dictionary<string, string>() { { "a0Formula", "a0" }, {"a1Formula","a1"},{"AmaxFormula","limA0"},{"j0Formula", "j0" },
                                                                                                {"j1Formula", "j1" },{"JmaxFormula","limj0" },{"MasterRangeFormula","range"},{"gainFormula","gain"},
                                                                                                {"v0Formula","v0"},{"v1Formula","v1"},{"VmaxFormula","v0"} };
    }
}