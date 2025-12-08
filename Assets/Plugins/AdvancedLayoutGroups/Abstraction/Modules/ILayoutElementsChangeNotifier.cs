using System;

namespace AdvancedLayoutGroups.Abstraction.Modules
{
    public interface ILayoutElementsChangeNotifier
    {
        public event Action Changed;
        public void Update(float deltaTime);
    }
}