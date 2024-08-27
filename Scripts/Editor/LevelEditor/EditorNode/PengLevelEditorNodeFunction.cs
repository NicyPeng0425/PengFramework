using PengScript;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace PengLevelEditorNodes
{
    public class GenerateActor : PengLevelEditorNode
    {
        public PengLevelInt actorID;
        public GenerateActor(Vector2 pos, PengLevelEditor master, int id, string flowOut, string varOut, string varIn, string specialInfo)
        {
            InitialDraw(pos, master);
            nodeID = id;
            outID = ParseStringToDictionaryIntNodeIDConnectionID(flowOut);
            varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOut);
            varInID = ParseStringToDictionaryIntNodeIDConnectionID(varIn);
            meaning = "生成Actor。";

            inPoints = new PengLevelNodeConnection[1];
            inPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowIn, 0, this, null);
            outPoints = new PengLevelNodeConnection[1];
            outPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut, 0, this, null);
            inVars = new PengLevelNodeVariables[1];
            actorID = new PengLevelInt(this, "角色ID", 0, PengLevelNodeConnection.PengLevelNodeConnectionType.VarIn);
            inVars[0] = actorID;
            actorID.point = null;
            outVars = new PengLevelNodeVariables[1];
            outVars[0] = new PengLevelPengActor(this, "角色", 0, PengLevelNodeConnection.PengLevelNodeConnectionType.VarOut);

            type = PengLevelRuntimeFunction.LevelFunctionType.GenerateActor;
            nodeType = LevelNodeType.Function;
            name = GetDescription(type);
            ReadSpecialParaDescription(specialInfo);
            paraNum = 1;
        }

        public override string SpecialParaDescription()
        {
            return actorID.value.ToString();
        }

        public override void ReadSpecialParaDescription(string info)
        {
            if (info != "")
            {
                actorID.value = int.Parse(info);
            }
        }

        public override void DrawInVarValue(int inVarID, Rect field)
        {
            actorID.value = EditorGUI.IntField(field, actorID.value);
        }
    }

    public class SetMainActor : PengLevelEditorNode
    {
        public PengLevelPengActor actor;
        public SetMainActor(Vector2 pos, PengLevelEditor master, int id, string flowOut, string varOut, string varIn, string specialInfo)
        {
            InitialDraw(pos, master);
            nodeID = id;
            outID = ParseStringToDictionaryIntNodeIDConnectionID(flowOut);
            varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOut);
            varInID = ParseStringToDictionaryIntNodeIDConnectionID(varIn);
            meaning = "设置主控Actor。";

            inPoints = new PengLevelNodeConnection[1];
            inPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowIn, 0, this, null);
            outPoints = new PengLevelNodeConnection[1];
            outPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut, 0, this, null);
            inVars = new PengLevelNodeVariables[1];
            actor = new PengLevelPengActor(this, "角色", 0, PengLevelNodeConnection.PengLevelNodeConnectionType.VarIn);
            inVars[0] = actor;
            outVars = new PengLevelNodeVariables[0];

            type = PengLevelRuntimeFunction.LevelFunctionType.SetMainActor;
            nodeType = LevelNodeType.Function;
            name = GetDescription(type);
            ReadSpecialParaDescription(specialInfo);
            paraNum = 1;
        }
    }

    public class StartControl : PengLevelEditorNode
    {
        public StartControl(Vector2 pos, PengLevelEditor master, int id, string flowOut, string varOut, string varIn, string specialInfo)
        {
            InitialDraw(pos, master);
            nodeID = id;
            outID = ParseStringToDictionaryIntNodeIDConnectionID(flowOut);
            varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOut);
            varInID = ParseStringToDictionaryIntNodeIDConnectionID(varIn);
            meaning = "让所有角色开始接受控制。";

            inPoints = new PengLevelNodeConnection[1];
            inPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowIn, 0, this, null);
            outPoints = new PengLevelNodeConnection[1];
            outPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut, 0, this, null);
            inVars = new PengLevelNodeVariables[0];
            outVars = new PengLevelNodeVariables[0];

            type = PengLevelRuntimeFunction.LevelFunctionType.StartControl;
            nodeType = LevelNodeType.Function;
            name = GetDescription(type);
            ReadSpecialParaDescription(specialInfo);
            paraNum = 1;
        }
    }

    public class EndControl : PengLevelEditorNode
    {
        public EndControl(Vector2 pos, PengLevelEditor master, int id, string flowOut, string varOut, string varIn, string specialInfo)
        {
            InitialDraw(pos, master);
            nodeID = id;
            outID = ParseStringToDictionaryIntNodeIDConnectionID(flowOut);
            varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOut);
            varInID = ParseStringToDictionaryIntNodeIDConnectionID(varIn);
            meaning = "让所有角色停止接受控制。";

            inPoints = new PengLevelNodeConnection[1];
            inPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowIn, 0, this, null);
            outPoints = new PengLevelNodeConnection[1];
            outPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut, 0, this, null);
            inVars = new PengLevelNodeVariables[0];
            outVars = new PengLevelNodeVariables[0];

            type = PengLevelRuntimeFunction.LevelFunctionType.EndControl;
            nodeType = LevelNodeType.Function;
            name = GetDescription(type);
            ReadSpecialParaDescription(specialInfo);
            paraNum = 1;
        }
    }

    public class GenerateEnemy : PengLevelEditorNode
    {
        public bool isBoss = false;
        public List<Transform> trans = new List<Transform>();
        public List<int> id = new List<int>();
        public GenerateEnemy(Vector2 pos, PengLevelEditor master, int id, string flowOut, string varOut, string varIn, string specialInfo)
        {
            InitialDraw(pos, master);
            nodeID = id;
            outID = ParseStringToDictionaryIntNodeIDConnectionID(flowOut);
            varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOut);
            varInID = ParseStringToDictionaryIntNodeIDConnectionID(varIn);
            meaning = "生成敌人。";

            inPoints = new PengLevelNodeConnection[1];
            inPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowIn, 0, this, null);
            outPoints = new PengLevelNodeConnection[1];
            outPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut, 0, this, null);
            inVars = new PengLevelNodeVariables[0];
            outVars = new PengLevelNodeVariables[0];

            type = PengLevelRuntimeFunction.LevelFunctionType.GenerateEnemy;
            nodeType = LevelNodeType.Function;
            name = GetDescription(type);
            ReadSpecialParaDescription(specialInfo);
            paraNum = 1;
        }

        public override string SpecialParaDescription()
        {
            string result = isBoss? "1" + ";" :"0" + ";";
            if (trans.Count > 0)
            {
                for (int i = 0; i < trans.Count; i++)
                {
                    result += BaseScript.ParseVector3ToString(trans[i].position) + "|" + BaseScript.ParseVector3ToString(trans[i].eulerAngles) + "|" + id[i].ToString();
                    if (i != trans.Count - 1)
                    {
                        result += ";";
                    }
                }
            }
            return result;
        }

        public override void ReadSpecialParaDescription(string info)
        {
            trans.Clear();
            if (info != "")
            {
                string[] strings = info.Split(";");
                isBoss = int.Parse(strings[0]) > 0;
                if (strings.Length > 1 && editor != null)
                {
                    for (int i = 1; i < strings.Length; i++)
                    {
                        if (strings[i] != "")
                        {
                            string[] str = strings[i].Split("|");
                            Transform trans = new GameObject().transform;
                            trans.tag = "Temporary";
                            trans.name = nodeID + "号功能_召唤敌人_位置点_" + i.ToString();
                            trans.SetParent(editor.currentSelectingGO.transform);
                            trans.localPosition = BaseScript.ParseStringToVector3(str[0]);
                            trans.localEulerAngles = BaseScript.ParseStringToVector3(str[1]);
                            if (str.Length > 2)
                            {
                                id.Add(int.Parse(str[2]));
                            }
                            else
                            {
                                id.Add(100001);
                            }
                            this.trans.Add(trans);
                        }
                    }
                }
            }
        }

        public override void DrawMoreInfo(Rect moreInfoRect)
        {
            DrawNodeMeaning(moreInfoRect);
            Rect toggleLabel = new Rect(moreInfoRect.x + 200, moreInfoRect.y + 20, 50, 20);
            Rect button1 = new Rect(moreInfoRect.x + 280, moreInfoRect.y + 20, moreInfoRect.width - 320, 20);

            Rect toggle = new Rect(moreInfoRect.x + 255, moreInfoRect.y + 20, 20, 20);
            GUI.Box(toggleLabel, "是Boss");
            isBoss = EditorGUI.Toggle(toggle, isBoss);
            if (GUI.Button(button1, "增加生成点"))
            {
                Transform trans = new GameObject().transform;
                trans.tag = "Temporary";
                trans.name = nodeID + "号功能_召唤敌人_位置点";
                trans.SetParent(editor.currentSelectingGO.transform);
                trans.localPosition = Vector3.zero;
                trans.localEulerAngles = Vector3.zero;
                this.trans.Add(trans);
                id.Add(100001);
            }

            Rect paraPanel = new Rect(moreInfoRect.x + 200, moreInfoRect.y + 45, moreInfoRect.width - 240, moreInfoRect.height - 65);
            int line = Mathf.FloorToInt(paraPanel.height / 25f);
            int col = 1;
            if (trans.Count > 0)
            {
                col = Mathf.FloorToInt(trans.Count / line) + 1;
            }

            if (trans.Count > 0)
            {
                int index = 0;
                int delIndex = -1;
                int selectID = -1;
                for (int i = 0; i < col; i++)
                {
                    for (int j = 0; j < line; j++)
                    {
                        if (index < trans.Count)
                        {
                            Rect field = new Rect(paraPanel.x + 250 * i, paraPanel.y + 25 * j, 60, 20);
                            Rect fieldValue = new Rect(field.x + field.width + 10, field.y, 60, 20);
                            Rect delete = new Rect(fieldValue.x + fieldValue.width + 10, field.y, 45, 20);
                            Rect select = new Rect(delete.x + delete.width + 10, field.y, 45, 20);
                            GUI.Box(field, "敌人ID：");
                            id[index] = EditorGUI.IntField(fieldValue, id[index]);
                            if (GUI.Button(delete, "选中"))
                            {
                                selectID = index;
                            }
                            if (GUI.Button(select, "删除"))
                            {
                                delIndex = index;
                            }
                            index++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (delIndex >= 0)
                {
                    id.RemoveAt(delIndex);
                    GameObject.DestroyImmediate(trans[delIndex].gameObject);
                    trans.RemoveAt(delIndex);
                }
                if (selectID >= 0)
                {
                    Selection.activeGameObject = trans[selectID].gameObject;
                }
            }
        }
    }

    public class ActiveActor : PengLevelEditorNode
    {
        public PengLevelPengActor actor;
        public ActiveActor(Vector2 pos, PengLevelEditor master, int id, string flowOut, string varOut, string varIn, string specialInfo)
        {
            InitialDraw(pos, master);
            nodeID = id;
            outID = ParseStringToDictionaryIntNodeIDConnectionID(flowOut);
            varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOut);
            varInID = ParseStringToDictionaryIntNodeIDConnectionID(varIn);
            meaning = "激活关卡所包含的敌人的AI（但不包括使其开始接受操作）。";

            inPoints = new PengLevelNodeConnection[1];
            inPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowIn, 0, this, null);
            outPoints = new PengLevelNodeConnection[1];
            outPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut, 0, this, null);
            inVars = new PengLevelNodeVariables[0];
            outVars = new PengLevelNodeVariables[0];

            type = PengLevelRuntimeFunction.LevelFunctionType.ActiveActor;
            nodeType = LevelNodeType.Function;
            name = GetDescription(type);
            paraNum = 1;
        }
    }

    public class SetAirWall : PengLevelEditorNode
    {
        public SetAirWall(Vector2 pos, PengLevelEditor master, int id, string flowOut, string varOut, string varIn, string specialInfo)
        {
            InitialDraw(pos, master);
            nodeID = id;
            outID = ParseStringToDictionaryIntNodeIDConnectionID(flowOut);
            varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOut);
            varInID = ParseStringToDictionaryIntNodeIDConnectionID(varIn);
            meaning = "激活关卡AirWalls属性中的所有空气墙物体。空气墙建议直接使用框架自带预制体，拖入场景后作为Level的子物体，再修改其缩放即可。空气墙应当默认为激活的。";

            inPoints = new PengLevelNodeConnection[1];
            inPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowIn, 0, this, null);
            outPoints = new PengLevelNodeConnection[1];
            outPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut, 0, this, null);
            inVars = new PengLevelNodeVariables[0];
            outVars = new PengLevelNodeVariables[0];

            type = PengLevelRuntimeFunction.LevelFunctionType.SetAirWall;
            nodeType = LevelNodeType.Function;
            name = GetDescription(type);
            paraNum = 1;
        }
    }

    public class CloseAirWall : PengLevelEditorNode
    {
        public CloseAirWall(Vector2 pos, PengLevelEditor master, int id, string flowOut, string varOut, string varIn, string specialInfo)
        {
            InitialDraw(pos, master);
            nodeID = id;
            outID = ParseStringToDictionaryIntNodeIDConnectionID(flowOut);
            varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOut);
            varInID = ParseStringToDictionaryIntNodeIDConnectionID(varIn);
            meaning = "关闭关卡AirWalls属性中的所有空气墙物体。空气墙建议直接使用框架自带预制体，拖入场景后作为Level的子物体，再修改其缩放即可。空气墙应当默认为激活的。";

            inPoints = new PengLevelNodeConnection[1];
            inPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowIn, 0, this, null);
            outPoints = new PengLevelNodeConnection[1];
            outPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut, 0, this, null);
            inVars = new PengLevelNodeVariables[0];
            outVars = new PengLevelNodeVariables[0];

            type = PengLevelRuntimeFunction.LevelFunctionType.CloseAirWall;
            nodeType = LevelNodeType.Function;
            name = GetDescription(type);
            paraNum = 1;
        }
    }
}