using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Features.SlotMachine.Configs;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Features.SlotMachine.Data;

namespace Features.SlotMachine.UI
{
    public class HorizontalSlotMachine : MonoBehaviour
    {
        public event Action<Slot> SpinCompleted;

        public bool IsSpinning => _spinTween != null;
        
        [SerializeField] private HorizontalSlotMachineConfig _slotMachineConfig;
        [SerializeField] private Slot _slotPrefab;
        [SerializeField] private Transform _slotsViewport;
        [SerializeField] private Transform _pointer;

        private Vector3 _startViewportLocalPosition;
        private List<Slot> _createdSlots = new List<Slot>();
        
        private Tween _spinTween;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            _startViewportLocalPosition = _slotsViewport.localPosition;
        }

        public async UniTask Spin(SlotData winSlotData, DefineSecondarySlotAction defineSecondarySlotAction,
            CreateSlotAction createSlotAction, ExtraData extraData = default)
        {
            if (IsSpinning)
            {
                throw new InvalidOperationException("Slot machine is spinning already");
            }
            
            var winSlotPositionIndex = _slotMachineConfig.WinSlotPositionIndex.Random();
            var totalSlotsCount = _slotMachineConfig.TotalSlotsCount.Random();

            CreateSlots(winSlotData, totalSlotsCount, winSlotPositionIndex, 
                defineSecondarySlotAction, createSlotAction);

            var winSlot = _createdSlots[winSlotPositionIndex];

            _cts = new CancellationTokenSource();

            await UniTask.Yield(cancellationToken: _cts.Token);
            
            var isSpinCompleted = false;

            _spinTween = GetSpinTween(winSlot, extraData);
            _spinTween.OnComplete(() => isSpinCompleted = true);
            await _spinTween.AwaitForComplete(cancellationToken: _cts.Token);

            if (isSpinCompleted == false)
            {
                return;
            }

            _cts = null;
            _spinTween = null;

            winSlot.PlayWin();
            SpinCompleted?.Invoke(winSlot);
        }
        
        public void StopSpin()
        {
            _spinTween?.Kill();
            _spinTween = null;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public void StopSpinAndClear()
        {
            StopSpin();

            foreach (var slot in _createdSlots)
            {
                Destroy(slot.gameObject);
            }
            
            _createdSlots.Clear();
        }

        private Tween GetSpinTween(Slot winSlot, ExtraData extraData)
        {
            var winSlotTransform = (RectTransform)winSlot.transform;
            
            var scaledSize = new Vector2(
                winSlotTransform.sizeDelta.x,
                winSlotTransform.sizeDelta.y
            );
            
            var halfSize = scaledSize.x / 2;
            var deviation = Random.Range(-halfSize, halfSize);
            
            _slotsViewport.localPosition = _startViewportLocalPosition;
            
            var localWinSlotPos = _slotsViewport.InverseTransformPoint(winSlotTransform.position);
            var localPointerPos = _slotsViewport.InverseTransformPoint(_pointer.position);

            var xDelta = Mathf.Abs(localWinSlotPos.x - localPointerPos.x);
            var duration = _slotMachineConfig.DurationRange.GetRandom() + extraData.AdditionalSpinDuration;

            _slotsViewport.localPosition += Vector3.right * deviation;
            
            return _slotsViewport.DOLocalMoveX(_slotsViewport.localPosition.x - xDelta, duration)
                .SetEase(_slotMachineConfig.SpinningCurve);
        }

        private void CreateSlots(SlotData winSlotData, int totalSlotsCount, int winSlotPositionIndex, 
            DefineSecondarySlotAction defineSecondarySlotAction, CreateSlotAction createSlotAction)
        {
            for (var i = 0; i < totalSlotsCount; i++)
            {
                Slot slot = default;
                
                if (i == winSlotPositionIndex)
                {
                    slot = createSlotAction.Invoke(_slotPrefab, winSlotData, _slotsViewport);
                }
                else
                {
                    var slotData = defineSecondarySlotAction.Invoke();
                    slot = createSlotAction.Invoke(_slotPrefab, slotData, _slotsViewport);
                }
                
                _createdSlots.Add(slot);
            }
        }

        private void OnDisable()
        {
            StopSpin();
        }

        public struct ExtraData
        {
            public float AdditionalSpinDuration;

            public ExtraData(float additionalSpinDuration)
            {
                AdditionalSpinDuration = additionalSpinDuration;
            }
        }
    }

    public delegate SlotData DefineSecondarySlotAction();
    
    public delegate Slot CreateSlotAction(Slot prefab, SlotData slotData, Transform viewport);
}