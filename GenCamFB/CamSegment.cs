using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCamFB
{
    struct SegmentData
    {
        public bool isFormula;
        public decimal Value;
        public string Formula; 
    }
    struct Segment
    {
        public Segment() { }
        public Dictionary<string, SegmentData> PointData = new Dictionary<string, SegmentData>();
        public string MotionLaw;
        public string SyncType;
    }
}
