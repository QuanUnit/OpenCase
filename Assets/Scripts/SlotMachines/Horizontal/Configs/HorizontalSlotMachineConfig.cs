using QCore.Tools.Mathematics;
using Tools.Mathematics;
using UnityEngine;

namespace SlotMachines.Horizontal.Configs
{
    [CreateAssetMenu(menuName = "Configs/SlotMachines/Horizontal/" + nameof(HorizontalSlotMachineConfig), fileName = nameof(HorizontalSlotMachineConfig))]
    public class HorizontalSlotMachineConfig : ScriptableObject
    {
        public FloatRange DurationRange => _durationRange;
        public AnimationCurve SpinningCurve => _spinningCurve;
        public IntRange TotalSlotsCount => _totalSlotsCount;
        public IntRange WinSlotPositionIndex => _winSlotPositionIndex;
        
        [SerializeField] private FloatRange _durationRange;
        [SerializeField] private AnimationCurve _spinningCurve;
        [SerializeField] private IntRange _totalSlotsCount;
        [SerializeField] private IntRange _winSlotPositionIndex;
    }
}