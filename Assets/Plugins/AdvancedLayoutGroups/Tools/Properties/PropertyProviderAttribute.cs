using System;

namespace AdvancedLayoutGroups.Tools.Properties
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class PropertyProviderAttribute : Attribute
    {
        public string ProvidingFieldName { get; }
        
        public PropertyProviderAttribute(string providingFieldName)
        {
            ProvidingFieldName = providingFieldName;
        }
    }
}