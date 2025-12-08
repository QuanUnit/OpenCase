using AdvancedLayoutGroups.Abstraction.Groups;
using AdvancedLayoutGroups.Abstraction.Modules;
using AdvancedLayoutGroups.Implementation.Modules;

namespace AdvancedLayoutGroups.Implementation.Groups
{
    public class AdvancedHorizontalLayoutGroup : AdvancedOneAxisLayoutGroup
    {
        protected override ILayoutElementsPositionCalculator CreateLayoutElementsPositionCalculator()
        {
            return new HorizontalLayoutElementsPositionCalculator(LayoutElementsProvider, CreateLayoutCalculationSettings);
        }
    }
}