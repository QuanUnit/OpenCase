using System;

namespace AdvancedLayoutGroups.Tools.Properties
{
    public interface INotifiedProperty
    {
        public event Action Changed;
    }
}