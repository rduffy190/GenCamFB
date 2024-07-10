using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCamFB
{
    public class StBuilder
    {
        public StBuilder(List<string> Init) {
            foreach(string inits in Init)
            {
                _stBuild = _stBuild.Append(inits + ";\r\n");
            }
            _stBuild =_stBuild.Append("IF bExecute AND NOT _execute THEN\r\n" +
                "\t_write(Execute:= FALSE);\t\r\n\t_create(Execute:= FALSE);\r\n\r\n" +
                "\t_state:= BUILD_BUFFER;\r\n\t\r\nEND_IF\r\n\r\nCASE _state OF\r\n" +
                "\tIDLE:  //wait\r\n\t\tbDone:= bError:= FALSE;\t\r\n\t\t\r\n\t\r\n" +
                "\tBUILD_BUFFER:  //build flat buffer\r\n\t\t_builder(forceDefaults := TRUE);                   //Start flatbuffer builder and load with default values\r\n" +
                "\t\r\n\t\t_name_Offset:= _builder.createString(sProfileName);\r\n\t\t\r\n" +
                "\t\t//_camBuilderProfileData_Offset:= _builder.createString('');\r\n" +
                "\t\t_camBuilderProfileData_Offset:= _builder.createString('{$\"ProfileData$\":{$\"Header$\":{$\"ApplicationName$\":$\"CamDesigner$\"}}}');\r\n");
            _segCount = 0; 
        }
        public void addSegment(Segment segment)
        {
            _stBuild =_stBuild.Append("\t\t_motion_sync_fbtypes_CfgFlexProfileSegment.startCfgFlexProfileSegment(_builder);\n"); 
            foreach (string key in _keyTranslator.Keys)
            {
                _stBuild = _stBuild.Append("\t\t_motion_sync_fbtypes_CfgFlexProfileSegment." + _keyTranslator[key]+"(");
                if (segment.PointData[key].isFormula)
                   _stBuild = _stBuild.Append(segment.PointData[key].Formula);
                else
                   _stBuild = _stBuild.Append(segment.PointData[key].Value);
                _stBuild =_stBuild.Append(");\n"); 
            }
            _stBuild = _stBuild.Append("\t\t_motion_sync_fbtypes_CfgFlexProfileSegment.addLawType(CXA_MotionSync_fbs.motion_sync_fbtypes_SegmentLawType."
                            + segment.MotionLaw + ");\n");
            _stBuild = _stBuild.AppendFormat("\t\tsegments[{0}]:= _motion_sync_fbtypes_CfgFlexProfileSegment.endCfgFlexProfileSegment();\r\n", _segCount);
            _segCount++; 
        }
        public string ST()
        {
            _stBuild = _stBuild.AppendFormat("\t\t_segments_Offset:= _motion_sync_fbtypes_AxsCfgSingleFlexProfile.createSegmentsVector(_builder,  ADR(segments[0]), {0});\r\n\t\t\t\t\t\t\r\n" +
                "\t\t_motion_sync_fbtypes_AxsCfgSingleFlexProfile.startAxsCfgSingleFlexProfile(_builder);\t\r\n\t\t_motion_sync_fbtypes_AxsCfgSingleFlexProfile.addName(_name_Offset);\r\n" +
                "\t\t_motion_sync_fbtypes_AxsCfgSingleFlexProfile.addSegments(_segments_Offset);\r\n\t\t_motion_sync_fbtypes_AxsCfgSingleFlexProfile.addMasterAxsRefVel(1);\r\n" +
                "\t\t_motion_sync_fbtypes_AxsCfgSingleFlexProfile.addCamBuilderProfileData(_camBuilderProfileData_Offset);\r\n\t\t\r\n" +
                "\t\t_builder.finish(_motion_sync_fbtypes_AxsCfgSingleFlexProfile.endAxsCfgSingleFlexProfile() );\r\n" +
                "\t\t_nodeValue_profile.SetFlatbuffer(_builder);                 //Set flatbuffer out of the builder in the variable to be written\r\n\r\n\t\t\r\n" +
                "\t\t_state:= UPDATE_PROFILE;\r\n\t\r\n\tUPDATE_PROFILE:  //update profile (write)\r\n" +
                "\t\t_nodeName := concat('motion/axs/',sAxisName); \r\n" +
                "\t\t_nodeName := concat(_nodeName, '/cfg/functions/coupling/sync-motion/flexprofile/profiles/');\r\n" +
                "\t\t_nodeName := concat(_nodeName,sProfileName);  \r\n" +
                "\t\t_write(\r\n\t\t\tExecute:= TRUE, \r\n\t\t\tNodeName:= _nodeName, \r\n\t\t\tNodeValueIn:= _nodeValue_profile, \r\n" +
                "\t\t\tNodeValueOut:= _nodeValue_dummy);\t\r\n" +
                "\t\t\t\r\n\t\tIF _write.Done THEN \r\n" +
                "\t\t\t_state:= SET_NODE_VALIDATE;\r\n\t\t\t\r\n" +
                "\t\tELSIF _write.Error THEN\r\n" +
                "\t\t\t_state:= ERROR;\r\n\t\t\t\r\n" +
                "\t\tEND_IF\r\n\t\r\n\tSET_NODE_VALIDATE:\r\n" +
                "\t\t_nodeValue_validate.SetValueString(ADR(sProfileName));\r\n" +
                "\t\t_state:= CREATE_PROFILE;\r\n\t\r\n" +
                "\tCREATE_PROFILE:  //validate profile (create)\r\n" +
                "\t\t_nodeName := concat('motion/axs/',sAxisName); \r\n" +
                "\t\t_nodeName := concat(_nodeName, '/cfg/functions/coupling/sync-motion/flexprofile/validate');\r\n" +
                "\t\t_create(\r\n\t\t\tExecute:= TRUE, \r\n" +
                "\t\t\tNodeName:= _nodeName, \r\n" +
                "\t\t\tNodeValueIn:= _nodeValue_validate, \r\n" +
                "\t\t\tNodeValueOut:= _nodeValue_dummy);\t" +
                "\r\n\t\tIF _create.Done THEN \r\n" +
                "\t\t\t_state:= CHECK_VALID;\r\n\t\t\t\r" +
                "\n\t\tELSIF _create.Error THEN\r\n" +
                "\t\t\t_state:= ERROR;\r\n\t\t\t\r" +
                "\n\t\tEND_IF\t\r\n\t" +
                "\t\t\r\n\tCHECK_VALID: \r\n" +
                "\t\t_nodeName := concat('motion/axs/',sAxisName); \r\n" +
                "\t\t_nodeName := concat(_nodeName, '/state/functions/coupling/sync-motion/flexprofile/profiles/');\r\n" +
                "\t\t_nodeName := concat(_nodeName,sProfileName); \r\n\t\t_read(Execute := TRUE, \r\n" +
                "\t\t\t  NodeName := _nodeName, \r\n\t\t\t  NodeValue:= _nodeValue_Valid); \r\n" +
                "\t\tIF _read.Error THEN \r\n\t\t\t_state := ERROR; \r\n" +
                "\t\tELSIF _read.Done THEN \r\n" +
                "\t\t\t_flex_validate.getRootAsAxsStateSingleFlexProfile(_nodeValue_Valid.getData(),_nodeValue_Valid.GetSize()); \r\n" +
                "\t\t\tIF _flex_validate.getAccessState() = 'VALID' THEN \r\n" +
                "\t\t\t\t_state := DONE; \r\n" +
                "\t\t\tELSE \r\n" +
                "\t\t\t\t_read(Execute := FALSE); \r\n" +
                "\t\t\tEND_IF\r\n" +
                "\t\tEND_IF" +
                "\r\n\tDONE: // done\r" +
                "\n\t\tbDone:= TRUE;\r\n\t\t\r" +
                "\n\t\tIF NOT bExecute THEN _state:= IDLE; END_IF;\r\n\t\r" +
                "\n\tERROR: // error\r\n\t\tbError:= TRUE;\r\n\t\t\r" +
                "\n\t\tIF NOT bExecute THEN _state:= IDLE; END_IF;\r\n\t\r\n" +
                "\t\r\nEND_CASE\r\n\r\n\r\n" +
                "_execute:= bExecute;", _segCount);
            return _stBuild.ToString();
        }



        StringBuilder _stBuild = new StringBuilder();
        Dictionary<string, string> _keyTranslator = new Dictionary<string, string>() { { "gain", "addGain" }, { "range", "addRange" }, {"v0","addV0"},{"v1","addV1"},{"a0","addA0"},
                                                                                    {"a1","addA1"},{"j0","addJ0"}, {"j1","addJ1"}, {"limA0","addLimA0"}, {"limA1","addLimA1"},
                                                                                    {"limV","addLimV" },{"limJ0","addLimJ0"},{"limJ1","addLimJ1"},{"lambda","addLambda"},{"master","addMaster"}};
        int _segCount; 
    }
}
