using PengVariables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PengLevelRuntimeLevelScriptVariables
{
    public enum PengLevelRuntimeLevelScriptVariableType
    {
        PengActor,
        ListPengActor,
        String,
        Int,
        Vector3,
    }

    public class PengLevelVar
    {
        public PengLevelRuntimeFunction.BaseScript script;
        public string name;
        public PengLevelRuntimeLevelScriptVariableType type;
        public int index;
    }

    public class PengPengActor : PengLevelVar
    {
        public PengActor value = null;
        public PengPengActor(string name, int index)
        {
            this.name = name;
            this.index = index;
            type = PengLevelRuntimeLevelScriptVariableType.PengActor;
        }
    }

    public class ListPengActor : PengLevelVar
    {
        public List<PengActor> value = new List<PengActor>();

        public ListPengActor(string name, int index)
        {
            this.name = name;
            this.index = index;
            this.type = PengLevelRuntimeLevelScriptVariableType.ListPengActor;
        }
    }

    public class PengString : PengLevelVar
    {
        public string value;

        public PengString(string name, int index)
        {
            this.name = name;
            this.index = index;
            type = PengLevelRuntimeLevelScriptVariableType.String;
        }
    }

    public class PengVector3 : PengLevelVar
    {
        public Vector3 value;

        public PengVector3(string name, int index)
        {
            this.name = name;
            this.index = index;
            type = PengLevelRuntimeLevelScriptVariableType.Vector3;
        }
    }

    public class PengInt : PengLevelVar
    {
        public int value;

        public PengInt(string name, int index)
        {
            this.name = name;
            this.index = index;
            type = PengLevelRuntimeLevelScriptVariableType.Int;
        }
    }
}
