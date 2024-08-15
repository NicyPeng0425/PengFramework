using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace PengVariables
{
    public enum PengVarType
    {
        [Description("无")]
        None,
        [Description("浮点")]
        Float,
        [Description("整型")]
        Int,
        [Description("字符串")]
        String,
        [Description("布尔")]
        Bool,
        [Description("PengActor")]
        PengActor,
        [Description("列表")]
        PengList,
        [Description("向量3")]
        Vector3,
        [Description("向量2")]
        Vector2,
        [Description("彭泛型")]
        T,
    }

    public class PengVar
    {
        public PengScript.BaseScript script;
        public string name;
        public PengVarType type;
        public Rect varRect;
        public int index;
    }

    public class PengFloat: PengVar
    {
        public float value;

        public PengFloat(string name, int index, PengScript.ConnectionPointType pointType) 
        {
            this.name = name;
            this.index = index;
            type = PengVarType.Float;
        }

    }

    public class PengInt : PengVar
    {
        public int value;

        public PengInt(string name, int index, PengScript.ConnectionPointType pointType)
        {
            this.name = name;
            this.index = index;
            type = PengVarType.Int;
        }
    }

    public class PengString : PengVar
    {
        public string value;

        public PengString(string name, int index, PengScript.ConnectionPointType pointType)
        {
            this.name = name;
            this.index = index;
            type = PengVarType.String;
        }
    }

    public class PengBool : PengVar
    {
        public bool value;

        public PengBool(string name, int index, PengScript.ConnectionPointType pointType)
        {
            this.name = name;
            this.index = index;
            type = PengVarType.Bool;
        }
    }

    public class PengPengActor : PengVar
    {
        public PengActor value = null;
        public PengPengActor(string name, int index, PengScript.ConnectionPointType pointType)
        {
            this.name = name;
            this.index = index;
            type = PengVarType.PengActor;
        }
    }

    public class PengList<PengActor> : PengVar
    {
        public List<PengActor> value = new List<PengActor>();

        public PengList(){ }

        public PengList(string name, int index, PengScript.ConnectionPointType pointType)
        {
            this.name = name;
            this.index = index;
            this.type = PengVarType.PengList;
        }
    }

    public class PengVector3 : PengVar
    {
        public Vector3 value = Vector3.zero;
        public PengVector3(string name, int index, PengScript.ConnectionPointType pointType)
        {
            this.name = name;
            this.index = index;
            type = PengVarType.Vector3;
        }
    }

    public class PengVector2 : PengVar
    {
        public Vector2 value = Vector2.zero;
        public PengVector2(string name, int index, PengScript.ConnectionPointType pointType)
        {
            this.name = name;
            this.index = index;
            type = PengVarType.Vector2;
        }
    }

    public class PengT : PengVar
    {
        public PengVar value;

        public PengT(string name, int index, PengScript.ConnectionPointType pointType)
        {
            this.name = name;
            this.index = index;
            type = PengVarType.T;
        }

    }
}
