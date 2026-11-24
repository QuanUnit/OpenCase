using Features.SlotMachine.UI;
using Shared.UI.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Cases.UI.Panels
{
    public class SlotMachineCellView : MonoBehaviour
    {
        public LayoutElement LayoutElement => _layoutElement;
        public HorizontalSlotMachine SlotMachine => _slotMachine;
        public DropResultCard DropResultCard => _dropResultCard;
        public float DesignHeight => _scaleFitter.Target.rect.height;

        [SerializeField] private LayoutElement _layoutElement;
        [SerializeField] private UniformScaleFitter _scaleFitter;
        [SerializeField] private HorizontalSlotMachine _slotMachine;
        [SerializeField] private DropResultCard _dropResultCard;
    }
}
