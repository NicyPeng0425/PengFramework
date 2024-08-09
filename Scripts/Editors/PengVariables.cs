using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PengVariables
{
    public enum PengVarType
    {
        None,
        Float,
        Int,
        String,
        Bool,
        GameObject,
        PengActor
    }

    public class PengVar
    {
        public PengScript.BaseScript script;
        public PengNode node;
        public string name;
        public PengVarType type;
        public Rect varRect;
        public PengNodeConnection point;
        public int index;
        public ConnectionPointType connectionType;

        public virtual void DrawVar()
        {
            GUIStyle style = new GUIStyle("DD Background");
            style.fontSize = 10;
            if (connectionType == ConnectionPointType.In)
            {
                style.alignment = TextAnchor.MiddleLeft;
                varRect = new Rect(node.rectSmall.x + 5f, node.rectSmall.y + 5 + 23 * index, 110, 18);
            }
            else if(connectionType == ConnectionPointType.Out)
            {
                style.alignment = TextAnchor.MiddleLeft;
                varRect = new Rect(node.rectSmall.x + 0.5f * node.rectSmall.width + 5f, node.rectSmall.y + 5 + 23 * index, 110, 18);
            }
            GUI.Box(varRect, " " + name + "(" + type.ToString() + ")", style);
            point.Draw(varRect);
        }

        public static void SetValue(ref PengVar toBeSet, ref PengVar toBeGet)
        {
            System.Type type = toBeSet.GetType();
            switch (type.FullName)
            {
                case "PengFloat":
                    PengFloat pfs = toBeSet as PengFloat;
                    PengFloat pfg = toBeGet as PengFloat;
                    if (pfs != null) { pfs.value = pfg.value; }
                    break;
                case "PengInt":
                    PengInt pis = toBeSet as PengInt;
                    PengInt pig = toBeGet as PengInt;
                    if (pis != null) { pis.value = pig.value; }
                    break;
                case "PengString":
                    PengString pss = toBeSet as PengString;
                    PengString psg = toBeGet as PengString;
                    if (pss != null) { pss.value = psg.value; }
                    break;
                case "PengBool":
                    PengBool pbs = toBeSet as PengBool;
                    PengBool pbg = toBeGet as PengBool;
                    if (pbs != null) { pbs.value = pbg.value; }
                    break;
            }
        }

    }

    public class PengFloat: PengVar
    {
        public float value;

        public PengFloat(PengNode node, string name, int index, ConnectionPointType pointType) 
        {
            this.node = node;
            this.name = name;
            this.index = index;
            connectionType = pointType;
            type = PengVarType.Float;
            point = new PengNodeConnection(pointType, index, node, this);
        }

    }

    public class PengInt : PengVar
    {
        public int value;

        public PengInt(PengNode node, string name, int index, ConnectionPointType pointType)
        {
            this.node = node;
            this.name = name;
            this.index = index;
            connectionType = pointType;
            type = PengVarType.Int;
            point = new PengNodeConnection(pointType, index, node, this);
        }
    }

    public class PengString : PengVar
    {
        public string value;

        public PengString(PengNode node, string name, int index, ConnectionPointType pointType)
        {
            this.node = node;
            this.name = name;
            this.index = index;
            connectionType = pointType;
            type = PengVarType.String;
            point = new PengNodeConnection(pointType, index, node, this);
        }

    }

    public class PengBool : PengVar
    {
        public bool value;

        public PengBool(PengNode node, string name, int index, ConnectionPointType pointType)
        {
            this.node = node;
            this.name = name;
            this.index = index;
            connectionType = pointType;
            type = PengVarType.Bool;
            point = new PengNodeConnection(pointType, index, node, this);
        }
    }

}
