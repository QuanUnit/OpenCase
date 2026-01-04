using UnityEngine;
using UnityEngine.UI;

namespace Common.UI.SelectableSet
{
    public class SelectableSetControlBlock : MonoBehaviour
    {
        public Button StartSelectingButton => _startSelectingButton;
        public Button StopSelectingButton => stopSelectingButton;
        public Button SelectAllButton => _selectAllButton;
        public Button DeselectAllButton => _deselectAllButton;
        
        [SerializeField] private Button _startSelectingButton;
        [SerializeField] private Button stopSelectingButton;
        [SerializeField] private Button _selectAllButton;
        [SerializeField] private Button _deselectAllButton;
    }
}