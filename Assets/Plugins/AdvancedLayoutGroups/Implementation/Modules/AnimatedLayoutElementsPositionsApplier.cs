using System;
using System.Collections.Generic;
using AdvancedLayoutGroups.Abstraction.Modules;
using AdvancedLayoutGroups.Implementation.Data;
using AdvancedLayoutGroups.Tools.Layout;
using UnityEngine;

namespace AdvancedLayoutGroups.Implementation.Modules
{
    [Serializable]
    public class AnimatedLayoutElementsPositionsApplier : LayoutGroupModuleBase, ILayoutElementsPositionsApplier, IUpdatableLayoutModule
    {
        private LayoutElementsPlacementAnimationSettings Settings => _animationSettingsProvider.Invoke();
        
        private Func<LayoutElementsPlacementAnimationSettings> _animationSettingsProvider;
        
        private IReadOnlyDictionary<RectTransform, Vector3> _targetPositions;
        private readonly Dictionary<RectTransform, Vector3> _fromPositions = new Dictionary<RectTransform, Vector3>();
        
        private bool NeedUpdateAnimation => _elapsedTime < Settings.Duration;
        
        private float _elapsedTime;

        public AnimatedLayoutElementsPositionsApplier(Func<LayoutElementsPlacementAnimationSettings> animationSettingsProvider)
        {
            _animationSettingsProvider = animationSettingsProvider;
        }
        
        public void Apply(IReadOnlyDictionary<RectTransform, Vector3> targetPositions)
        {
            _targetPositions = targetPositions;
            
            _fromPositions.Clear();

            foreach (var element in _targetPositions.Keys)
            {
                _fromPositions.Add(element, element.anchoredPosition);
            }
            
            ResetTime();
        }

        public void Update(float deltaTime)
        {
            if(_targetPositions == null) return;
            if(NeedUpdateAnimation == false) return;

            LayoutElementsPlacementAnimationSettings settings = Settings;
            
            _elapsedTime += deltaTime;

            _elapsedTime = Mathf.Clamp(_elapsedTime, 0, settings.Duration);

            float t = _elapsedTime / settings.Duration;
            float evaluatedT = settings.Curve.Evaluate(t);

            foreach (var pair in _targetPositions)
            {
                RectTransform target = pair.Key;
                Vector3 from = _fromPositions[target];
                Vector3 to = pair.Value;
                
                ApplyAnimation(target, from, to, evaluatedT);
            }
        }

        private void ApplyAnimation(RectTransform target, Vector3 from, Vector3 to, float t)
        {
            Vector3 newPosition = Vector3.Lerp(from, to, t);
            AdvancedLayoutTools.PrepareRectTransformForCorrectLayout(target);
            target.anchoredPosition = newPosition;
        }

        private void ResetTime()
        {
            _elapsedTime = 0;
        }
    }
}