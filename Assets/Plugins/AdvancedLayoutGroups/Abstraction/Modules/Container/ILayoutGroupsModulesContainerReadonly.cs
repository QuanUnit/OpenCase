using System.Collections.Generic;

namespace AdvancedLayoutGroups.Abstraction.Modules.Container
{
    public interface ILayoutGroupsModulesContainerReadonly
    {
        public IEnumerable<LayoutGroupModuleBase> Modules { get; }
        public int ModulesCount { get; }

        public T Get<T>();
    }
}