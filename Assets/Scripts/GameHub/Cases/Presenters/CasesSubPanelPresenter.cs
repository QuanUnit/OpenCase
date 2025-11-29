using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameHub.Cases.Panels;
using QCore.Tools.Extensions;
using QCore.Tools.Identifiers;
using QCore.UISystem.Presenters;
using Services.Skins;
using Services.Skins.Configs;
using SlotMachines.Horizontal;
using UnityEngine;
using Zenject;

namespace GameHub.Cases.Presenters
{
    public class CasesSubPanelPresenter : PanelPresenter<CasesSubPanel>
    {
        [Inject] private ISkinsDatabaseService _skinsDatabaseService;
        [Inject] private ISkinsUIContextDataService _skinsUIContextDataService;
        
        protected override void StartPresentingInternal()
        {
            Panel.SpinClicked += SpinClickHandle;
        }

        private void SpinClickHandle()
        {
            IReadOnlyList<HorizontalSlotMachine> slotMachines = Panel.HorizontalSlotMachines;

            if (slotMachines.Any(a => a.IsSpinning)) return;

            float additionalDuration = 0;
            
            foreach (var slotMachine in slotMachines)
            {
                SkinConfig winSkin = _skinsDatabaseService.AllSkinsConfigs.First();
                SlotData winSkinData = new SlotData(winSkin.Id);

                HorizontalSlotMachine.ExtraData extraData = new HorizontalSlotMachine.ExtraData(additionalDuration);
                
                slotMachine.StopSpinAndClear();
                slotMachine.Spin(winSkinData, DefineSecondarySlotData, CreateSlot, extraData).Forget();

                additionalDuration += 1f;
            }
        }

        private SlotData DefineSecondarySlotData()
        {
            SkinConfig randomSkin = _skinsDatabaseService.AllSkinsConfigs.GetRandomUnique(1).First();

            return new SlotData(randomSkin.Id);
        }
        
        private Slot CreateSlot(Slot prefab, SlotData slotData, Transform viewport)
        {
            QGuid skinId = slotData.ItemId;
            SkinConfig skinConfig = _skinsDatabaseService.GetSkinConfigById(skinId);

            SkinSlot slot = (SkinSlot)Object.Instantiate(prefab, viewport);

            Sprite skinSprite = skinConfig.ContextData.SpriteSource.GetAsset();
            RarityVisual rarityVisual = _skinsUIContextDataService.GetRarityVisual(skinConfig.Rarity);
            
            slot.SetData(skinSprite, rarityVisual);

            return slot;
        }

        protected override void StopPresentingInternal()
        {
            Panel.SpinClicked -= SpinClickHandle;
        }
    }
}