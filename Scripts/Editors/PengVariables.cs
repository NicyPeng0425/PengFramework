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
        public PengNode node;
        public string name;
        public PengVarType type;
        public Rect varRect;
        public PengNodeConnection point;

        public virtual void DrawVar(bool left, int index)
        {
            GUIStyle style = new GUIStyle("DD Background");
            style.fontSize = 10;
            if (left)
            {
                style.alignment = TextAnchor.MiddleLeft;
                varRect = new Rect(node.rectSmall.x + 5f, node.rectSmall.y + 5 + 23 * index, 110, 18);
            }
            else
            {
                style.alignment = TextAnchor.MiddleLeft;
                varRect = new Rect(node.rectSmall.x + 0.5f * node.rectSmall.width + 5f, node.rectSmall.y + 5 + 23 * index, 110, 18);
            }
            GUI.Box(varRect, " " + name + "(" + type.ToString() + ")", style);
            point.Draw(varRect);
        }
    }

    public class PengFloat: PengVar
    {
        public float value;

        public PengFloat(PengNode node, string name, float value, ConnectionPointType pointType) 
        {
            this.node = node;
            this.name = name;
            type = PengVarType.Float;
            this.value = value;
            point = new PengNodeConnection(pointType, node, this);
        }
    }

    public class PengInt : PengVar
    {
        public int value;

        public PengInt(PengNode node, string name, int value, ConnectionPointType pointType)
        {
            this.node = node;
            this.name = name;
            type = PengVarType.Int;
            this.value = value;
            point = new PengNodeConnection(pointType, node, this);
        }
    }

    public class PengString : PengVar
    {
        public string value;

        public PengString(PengNode node, string name, string value, ConnectionPointType pointType)
        {
            this.node = node;
            this.name = name;
            type = PengVarType.String;
            this.value = value;
            point = new PengNodeConnection(pointType, node, this);
        }

    }

    public class PengBool : PengVar
    {
        public bool value;

        public PengBool(PengNode node, string name, bool value, ConnectionPointType pointType)
        {
            this.node = node;
            this.name = name;
            type = PengVarType.Bool;
            this.value = value;
            point = new PengNodeConnection(pointType, node, this);
        }
    }

}
