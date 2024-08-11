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
        PengActor,
        ListPengActor,
        Vector3,
        Vector2,
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
                varRect = new Rect(node.rectSmall.x + 5f, node.rect.y + node.rect.height + 5 + 23 * index, 110, 18);
            }
            else if(connectionType == ConnectionPointType.Out)
            {
                style.alignment = TextAnchor.MiddleLeft;
                varRect = new Rect(node.rectSmall.x + 0.5f * node.rectSmall.width + 5f, node.rect.y + node.rect.height + 5 + 23 * index, 110, 18);
            }
            GUI.Box(varRect, " " + name + "(" + type.ToString() + ")", style);
            if (point != null)
            {
                point.Draw(varRect);
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

    public class PengPengActor : PengVar
    {
        public PengActor value = null;
        public PengPengActor(PengNode node, string name, int index, ConnectionPointType pointType)
        {
            this.node = node;
            this.name = name;
            this.index = index;
            connectionType = pointType;
            type = PengVarType.PengActor;
            point = new PengNodeConnection(pointType, index, node, this);
        }
    }

    public class PengListPengActor : PengVar
    {
        public List<PengActor> value = new List<PengActor>();
        public PengListPengActor(PengNode node, string name, int index, ConnectionPointType pointType)
        {
            this.node = node;
            this.name = name;
            this.index = index;
            connectionType = pointType;
            type = PengVarType.ListPengActor;
            point = new PengNodeConnection(pointType, index, node, this);
        }
    }

    public class PengVector3 : PengVar
    {
        public Vector3 value = Vector3.zero;
        public PengVector3(PengNode node, string name, int index, ConnectionPointType pointType)
        {
            this.node = node;
            this.name = name;
            this.index = index;
            connectionType = pointType;
            type = PengVarType.Vector3;
            point = new PengNodeConnection(pointType, index, node, this);
        }
    }

    public class PengVector2 : PengVar
    {
        public Vector2 value = Vector2.zero;
        public PengVector2(PengNode node, string name, int index, ConnectionPointType pointType)
        {
            this.node = node;
            this.name = name;
            this.index = index;
            connectionType = pointType;
            type = PengVarType.Vector2;
            point = new PengNodeConnection(pointType, index, node, this);
        }
    }
}
