using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCamFB
{
    public static class SegmentKeys
    {
        public static readonly List<string> Keys = new List<string> { "gain", "range", "v0", "v1", "a0", "a1", "j0", "j1", "limA0", "limA1", "limV", "limJ0", "limJ1", "lambda", "master" };
    }
    public struct SegmentData
    {
        public bool isFormula;
        public decimal Value;
        public string Formula; 
    }
    public struct Segment
    {
        public Segment() { }
        public Dictionary<string, SegmentData> PointData = new Dictionary<string, SegmentData>();
        public string MotionLaw;
        public string SyncType;
    }
}
