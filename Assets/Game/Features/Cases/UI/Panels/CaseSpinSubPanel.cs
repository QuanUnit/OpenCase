using System;
using QCore.UISystem.Panels;
using Features.SlotMachine.UI;
using Shared.UI;
using Shared.UI.Effects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Cases.UI.Panels
{
    public class CaseSpinSubPanel : Panel
    {
        public event Action SpinClicked;
        public event Action BackClicked;

        public HorizontalSlotMachine SlotMachine => _slotMachine;

        [SerializeField] private HorizontalSlotMachine _slotMachine;
        [SerializeField] private Button _spinButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _caseName;
        [SerializeField] private TextedCurrencyView _spinPrice;
        [SerializeField] private GameObject _idleHint;
        [SerializeField] private GameObject _reward;
        [SerializeField] private TMP_Text _rewardName;
        [SerializeField] private TextedCurrencyView _rewardPrice;
        [SerializeField] private Image _rewardRarity;
        [SerializeField] private ScreenFlash _screenFlash;

        private RewardData _rewardData;

        private void OnEnable()
        {
            _spinButton.onClick.AddListener(SpinClickHandle);
            _backButton.onClick.AddListener(BackClickHandle);
            _slotMachine.SpinCompleted += SpinCompleteHandle;

            _idleHint.SetActive(true);
            _reward.SetActive(false);
        }

        public void SetCaseView(string caseName, float price)
        {
            _caseName.text = caseName;
            _spinPrice.SetView(price);
        }

        public void SetReward(RewardData data)
        {
            _rewardData = data;

            _idleHint.SetActive(false);
            _reward.SetActive(false);
        }

        private void SpinCompleteHandle(Slot winSlot)
        {
            _rewardName.text = _rewardData.SkinName;
            _rewardPrice.SetView(_rewardData.Price);
            _rewardRarity.color = _rewardData.RarityColor;
            _reward.SetActive(true);

            _screenFlash.Play(_rewardData.RarityColor);
        }

        private void BackClickHandle()
        {
            BackClicked?.Invoke();
        }

        private void SpinClickHandle()
        {
            SpinClicked?.Invoke();
        }

        private void OnDisable()
        {
            _spinButton.onClick.RemoveListener(SpinClickHandle);
            _backButton.onClick.RemoveListener(BackClickHandle);
            _slotMachine.SpinCompleted -= SpinCompleteHandle;
        }

        public struct RewardData
        {
            public string SkinName;
            public Color RarityColor;
            public float Price;
        }
    }
}
