using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Cases;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class CasesSheetGuardTests
    {
        [Test]
        public void FindUnknownIds_SheetHasForeignCases_ReturnsThemOnce()
        {
            var sheet = new List<List<string>>
            {
                new List<string> { "Id", "Name", "Price", "SkinId", "Weight" },
                new List<string> { "Case_A", "A", "1", "Skin", "1" },
                new List<string> { "", "", "", "Skin2", "1" },
                new List<string> { "Case_Old", "Old", "1", "Skin", "1" },
                new List<string> { "Case_Old", "Old", "1", "Skin", "1" },
                new List<string>(),
            };

            var unknownIds = new CasesSheetGuard().FindUnknownIds(sheet, new HashSet<string> { "Case_A" });

            Assert.That(unknownIds, Is.EqualTo(new[] { "Case_Old" }));
        }

        [Test]
        public void FindUnknownIds_OnlyHeader_ReturnsEmpty()
        {
            var sheet = new List<List<string>> { new List<string> { "Id" } };

            var unknownIds = new CasesSheetGuard().FindUnknownIds(sheet, new HashSet<string>());

            Assert.That(unknownIds, Is.Empty);
        }
    }
}
