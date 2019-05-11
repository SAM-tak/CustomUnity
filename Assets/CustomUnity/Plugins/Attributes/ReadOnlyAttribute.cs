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

        public string ComparedPropertyName { get; private set; }
        public object ComparedValue { get; private set; }
        public Comparison ComparisonType { get; private set; }

        public ReadOnlyIfAttribute(string comparedPropertyName, object comparedValue, Comparison comparisonType = Comparison.Equals)
        {
            ComparedPropertyName = comparedPropertyName;
            ComparedValue = comparedValue;
            ComparisonType = comparisonType;
        }
    }
}
