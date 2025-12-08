using AdvancedLayoutGroups.Abstraction.Modules;
using AdvancedLayoutGroups.Abstraction.Modules.Container;
using AdvancedLayoutGroups.Implementation.Data;
using AdvancedLayoutGroups.Implementation.Modules;
using AdvancedLayoutGroups.Tools.Properties;
using NaughtyAttributes;
using UnityEngine;

namespace AdvancedLayoutGroups.Abstraction.Groups
{
    public abstract class AdvancedOneAxisLayoutGroup : AdvancedLayoutGroup
    {
        public NotifiedProperty<Padding> Padding = new NotifiedProperty<Padding>(new Padding());
        public NotifiedProperty<LayoutAnchor> Anchor;
        public NotifiedProperty<float> Spacing;
        
        [OnValueChanged(nameof(UseAnimationValueChangeHandle))] public NotifiedProperty<bool> UseAnimation;

        public NotifiedProperty<LayoutElementsPlacementAnimationSettings> AnimationSettings = 
            new NotifiedProperty<LayoutElementsPlacementAnimationSettings>(new LayoutElementsPlacementAnimationSettings()
        {
            Duration = 0.3f,
            Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f),
        });

        private PropertiesCombinedNotifier _propertiesCombinedNotifier;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            _propertiesCombinedNotifier = new PropertiesCombinedNotifier(Padding, Anchor, Spacing, AnimationSettings);
            _propertiesCombinedNotifier.Changed += RecalculateLayout;
        }

        protected override void InstallModules(LayoutGroupsModulesContainer container)
        {
            ILayoutElementsPositionCalculator positionCalculator = CreateLayoutElementsPositionCalculator();
            
            container.Add(positionCalculator, typeof(ILayoutElementsPositionCalculator));

            InstallLayoutPositionsApplier(container);
        }

        private void InstallLayoutPositionsApplier(LayoutGroupsModulesContainer container)
        {
            ILayoutElementsPositionsApplier positionsApplier = default;

            if (UseAnimation)
            {
                positionsApplier = new AnimatedLayoutElementsPositionsApplier(() => AnimationSettings);
            }
            else
            {
                positionsApplier = new ImmediateLayoutElementsPositionsApplier();
            }
            
            container.Add(positionsApplier, typeof(ILayoutElementsPositionsApplier));
        }
        
        protected abstract ILayoutElementsPositionCalculator CreateLayoutElementsPositionCalculator();

        protected OneAxisLayoutCalculationSettings CreateLayoutCalculationSettings()
        {
            return new OneAxisLayoutCalculationSettings()
            {
                Padding = Padding,
                Anchor = Anchor,
                Spacing = Spacing,
            };
        }

        private void UseAnimationValueChangeHandle()
        {
            if(UseAnimation && ModulesContainerInternal.Has<AnimatedLayoutElementsPositionsApplier>()) return;
            if(UseAnimation == false && ModulesContainerInternal.Has<ImmediateLayoutElementsPositionsApplier>()) return;

            ModulesContainerInternal.Remove<AnimatedLayoutElementsPositionsApplier>();
            ModulesContainerInternal.Remove<ImmediateLayoutElementsPositionsApplier>();

            InstallLayoutPositionsApplier(ModulesContainerInternal);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            _propertiesCombinedNotifier.Dispose();
        }
    }
}