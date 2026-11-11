using Kernel.Flow;
using UnityEngine;
using Zenject;

namespace Kernel.Bootstrap
{
    public class Bootstrapper : MonoBehaviour
    {
        private GlobalStateMachineFactory _stateMachineFactory;

        [Inject]
        public void Construct(GlobalStateMachineFactory stateMachineFactory)
        {
            _stateMachineFactory = stateMachineFactory;
        }

        public void Start()
        {
            var stateMachine = _stateMachineFactory.Create();
            stateMachine.EnterFirstState();
        }
    }
}