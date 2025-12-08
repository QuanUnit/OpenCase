using AdvancedLayoutGroups.Abstraction.Groups;
using AdvancedLayoutGroups.Abstraction.Modules;
using AdvancedLayoutGroups.Implementation.Modules;

namespace AdvancedLayoutGroups.Implementation.Groups
{
    public class AdvancedVerticalLayoutGroup : AdvancedOneAxisLayoutGroup
    {
        protected override ILayoutElementsPositionCalculator CreateLayoutElementsPositionCalculator()
        {
            return new VerticalLayoutElementsPositionCalculator(LayoutElementsProvider, CreateLayoutCalculationSettings);
        }
    }
}