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
        public string name;
        public PengVarType type;
    }

    public class PengFloat: PengVar
    {
        public float value;

        public PengFloat(string name, float value) 
        {
            this.name = name;
            type = PengVarType.Float;
            this.value = value;
        }
    }

}
