using System;
using UnityEngine;

namespace Sparkfire.Utility
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class BlankAttribute : PropertyAttribute
    {
        public BlankAttribute(params object[] args) { }
    }
}
