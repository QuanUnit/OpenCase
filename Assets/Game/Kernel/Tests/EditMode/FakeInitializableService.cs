using System.Collections.Generic;
using Shared.Lifecycle;

namespace Kernel.Tests.EditMode
{
    public class FakeInitializableService : IInitializableService
    {
        private readonly List<string> _callOrder;
        private readonly string _name;

        public int InitCount { get; private set; }

        public FakeInitializableService(string name, List<string> callOrder)
        {
            _name = name;
            _callOrder = callOrder;
        }

        public void Init()
        {
            InitCount++;
            _callOrder.Add(_name);
        }
    }
}
