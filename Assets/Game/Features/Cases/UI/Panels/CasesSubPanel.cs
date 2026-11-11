using System;
using System.Collections.Generic;
using QCore.Tools.Collections;
using QCore.Tools.Identifiers;
using QCore.UISystem.Panels;
using Features.Cases.Configs;
using UnityEngine;

namespace Features.Cases.UI.Panels
{
    public class CasesSubPanel : Panel
    {
        public event Action<QGuid> CaseOpenClicked;
        public CaseSpinSubPanel CaseSpinSubPanel => _caseSpinSubPanel;
    
        [SerializeField] private CaseSpinSubPanel _caseSpinSubPanel;
        [SerializeField] private CasesCategoryTitle _casesCategoryTitlePrefab;
        [SerializeField] private CaseView _caseViewPrefab;
        [SerializeField] private GameObject _casesGridPrefab;

        [SerializeField] private Transform _viewport;

        private readonly Dictionary<QGuid, CaseView> _createdCasesMap = new Dictionary<QGuid, CaseView>();
        
        public void CreateCasesViews(IEnumerable<KeyValue<CasesCategory, List<CaseConfig>>> casesData)
        {
            if (_createdCasesMap.Count != 0)
            {
                throw new InvalidOperationException("Cannot to create view because it's not cleared");
            }
            
            foreach (var pair in casesData)
            {
                var category = pair.Key;

                CrateCategoryTitle(category, _viewport);
                var casesGrid = Instantiate(_casesGridPrefab, _viewport);
                
                foreach (var caseConfig in pair.Value)
                {
                    var caseView = CreateCaseView(caseConfig, casesGrid.transform);

                    caseView.OpenClicked += CaseOpenClickHandle;
                    _createdCasesMap.Add(caseConfig.Id, caseView);
                }
            }
        }

        private void CaseOpenClickHandle(CaseView sender)
        {
            CaseOpenClicked?.Invoke(sender.CaseId);
        }

        private CaseView CreateCaseView(CaseConfig caseConfig, Transform parent)
        {
            var result = Instantiate(_caseViewPrefab, parent);
            result.SetData(caseConfig);

            return result;
        }

        private CasesCategoryTitle CrateCategoryTitle(CasesCategory category, Transform parent)
        {
            var result = Instantiate(_casesCategoryTitlePrefab, parent);
            result.SetData(category.Name);

            return result;
        }

        public void ClearCasesViews()
        {
            if (_createdCasesMap.Count == 0) return;
            
            foreach (var caseView in _createdCasesMap.Values)
            {
                caseView.OpenClicked -= CaseOpenClickHandle;
            }
            
            foreach (Transform child in _viewport)
            {
                Destroy(child.gameObject);
            }
            
            _createdCasesMap.Clear();
        }

        private void OnDisable()
        {
            ClearCasesViews();
        }
    }
}