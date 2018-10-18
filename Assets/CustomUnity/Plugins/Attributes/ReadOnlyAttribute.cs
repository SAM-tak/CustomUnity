using System;
using UnityEngine;

namespace CustomUnity
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class ReadOnlyWhenPlayingAttribute : PropertyAttribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class ReadOnlyIfAttribute : PropertyAttribute
    {
        public enum Comparison
        {
            Equals,
            NotEqual,
            GreaterThan,
            LessThan,
            GreaterOrEqual,
            LessOrEqual
        }

        public string comparedPropertyName { get; private set; }
        public object comparedValue { get; private set; }
        public Comparison comparisonType { get; private set; }

        public ReadOnlyIfAttribute(string comparedPropertyName, object comparedValue, Comparison comparisonType = Comparison.Equals)
        {
            this.comparedPropertyName = comparedPropertyName;
            this.comparedValue = comparedValue;
            this.comparisonType = comparisonType;
        }
    }
}
