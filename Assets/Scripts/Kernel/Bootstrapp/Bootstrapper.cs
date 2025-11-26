using Kernel.Flow;
using UnityEngine;
using Zenject;

namespace Kernel.Bootstrapp
{
    public class Bootstrapper : MonoBehaviour
    {
        [Inject] private GlobalStateMachineFactory _stateMachineFactory;
        
        public void Start()
        {
            GlobalStateMachine stateMachine = _stateMachineFactory.Create();
            stateMachine.EnterFirstState();
        }
    }
}