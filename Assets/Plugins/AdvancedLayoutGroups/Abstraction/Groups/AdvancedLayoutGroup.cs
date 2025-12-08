using System.Collections.Generic;
using AdvancedLayoutGroups.Abstraction.Modules;
using AdvancedLayoutGroups.Abstraction.Modules.Container;
using AdvancedLayoutGroups.Implementation.Modules;
using UnityEngine;

namespace AdvancedLayoutGroups.Abstraction.Groups
{
    [ExecuteInEditMode]
    public abstract class AdvancedLayoutGroup : MonoBehaviour
    {
        public ILayoutGroupsModulesContainerReadonly ModulesContainer => _modulesContainer;
        
        protected LayoutGroupsModulesContainer ModulesContainerInternal => _modulesContainer;
        protected ILayoutElementsProvider LayoutElementsProvider => _layoutElementsProvider;

        private LayoutGroupsModulesContainer _modulesContainer;
        
        private ILayoutElementsProvider _layoutElementsProvider;
        private ILayoutElementsChangeNotifier _layoutElementsChangeNotifier;
        
        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            _modulesContainer = new LayoutGroupsModulesContainer();
            _layoutElementsProvider = CreateLayoutElementsProvider(); 
            _layoutElementsChangeNotifier = CreateLayoutElementsChangeNotifier(_layoutElementsProvider);

            InstallModules(_modulesContainer);
        }

        protected abstract void InstallModules(LayoutGroupsModulesContainer container);

        protected virtual void OnEnable()
        {
            
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                Initialize();
            }
#endif
            
            _layoutElementsChangeNotifier.Changed += RecalculateLayout;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            _layoutElementsChangeNotifier.Update(deltaTime);
            _modulesContainer.Update(deltaTime);
        }

        public void RecalculateLayout()
        {
            if (_layoutElementsProvider == null) return;
            
            _modulesContainer.TryGet(out ILayoutElementsPositionCalculator positionCalculator);
            _modulesContainer.TryGet(out ILayoutElementsPositionsApplier positionsApplier);

            if (positionsApplier == null)
            {
                Debug.LogError("Missing positions applier module");
                return;
            }
            
            if (positionCalculator == null)
            {
                Debug.LogError("Missing positions calculator module");
                return;
            }
            
            IReadOnlyDictionary<RectTransform, Vector3> recalculatedPositions = positionCalculator.Calculate((RectTransform)transform);
            positionsApplier.Apply(recalculatedPositions);

            Debug.Log("Layout RECALCULATED");
        }

        private ILayoutElementsProvider CreateLayoutElementsProvider()
        {
            return new ChildrenLayoutElementsProvider((RectTransform)transform);
        }

        private ILayoutElementsChangeNotifier CreateLayoutElementsChangeNotifier(ILayoutElementsProvider elementsProvider)
        {
            return new DefaultLayoutElementsChangeNotifier((RectTransform)transform, elementsProvider);
        }

        protected virtual void OnDisable()
        {
            _layoutElementsChangeNotifier.Changed -= RecalculateLayout;
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            RecalculateLayout();
        }

#endif
    }
}