using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Icons;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class SkinIconsReportTests
    {
        private const string Folder = "Assets/Weapons/AK47";

        [Test]
        public void Constructor_PlanWithSkips_CountsAndSkippedIds()
        {
            var sources = new Dictionary<string, SkinIconSource>
            {
                { "AK-47_A", new SkinIconSource("AK-47_A", "url-a") },
                { "AK-47_B", new SkinIconSource("AK-47_B", "url-b") },
            };
            var configs = new List<SkinIconConfigState>
            {
                new SkinIconConfigState("AK-47_A", "AK-47", null, "c1"),
                new SkinIconConfigState("AK-47_B", "AK-47", null, "c2"),
                new SkinIconConfigState("AK-47_Typo", "AK-47", null, "c3"),
                new SkinIconConfigState("AK-47_Ok", "AK-47", $"{Folder}/AK-47_Ok.png", "c4"),
            };
            var plan = new SkinIconsPlanner().Build(configs, sources, SkinIconsMode.OnlyMissing, weapon => Folder, path => path == $"{Folder}/AK-47_B.png");

            var report = new SkinIconsReport(plan, true);

            Assert.That(report.Planned, Is.EqualTo(2));
            Assert.That(report.PlannedDownloads, Is.EqualTo(1));
            Assert.That(report.PlannedReuses, Is.EqualTo(1));
            Assert.That(report.AlreadyOk, Is.EqualTo(1));
            Assert.That(report.NoSource, Is.EqualTo(new[] { "AK-47_Typo" }));
            Assert.That(report.GetShortSummary(), Does.Contain("Planned: 2 (download 1, reuse existing file 1)"));
            Assert.That(report.GetFullSummary(), Does.Contain("AK-47_Typo"));
        }

        [Test]
        public void GetFullSummary_DryRunWithFoldersToRegister_ListsThem()
        {
            var report = new SkinIconsReport(new SkinIconsPlan(SkinIconsMode.OnlyMissing), true);
            report.PlannedRegisteredFolders.Add("Assets/Weapons/Nova");

            var summary = report.GetFullSummary();

            Assert.That(summary, Does.Contain("Folders registered in Addressables: 1"));
            Assert.That(summary, Does.Contain("Assets/Weapons/Nova"));
        }
    }
}
