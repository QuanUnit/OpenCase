using System.Collections.Generic;
using System.Linq;
using Features.RemoteConfigs.Editor.Icons;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class SkinIconsPlannerTests
    {
        private const string Folder = "Assets/Weapons/AK47";

        private SkinIconsPlanner _planner;
        private Dictionary<string, SkinIconSource> _sources;
        private HashSet<string> _existingFiles;

        [SetUp]
        public void SetUp()
        {
            _planner = new SkinIconsPlanner();
            _sources = new Dictionary<string, SkinIconSource>();
            _existingFiles = new HashSet<string>();
        }

        [Test]
        public void Build_OnlyMissing_PlansOnlyConfigsWithoutIcon()
        {
            AddSource("AK-47_A");
            AddSource("AK-47_B");
            var configs = new List<SkinIconConfigState>
            {
                CreateConfig("AK-47_A", null),
                CreateConfig("AK-47_B", $"{Folder}/imgi_9_AK-47_Other.png"),
            };

            var plan = Build(configs, SkinIconsMode.OnlyMissing);

            Assert.That(plan.Items.Select(item => item.Id), Is.EqualTo(new[] { "AK-47_A" }));
            Assert.That(plan.Items[0].TargetPath, Is.EqualTo($"{Folder}/AK-47_A.png"));
            Assert.That(plan.Items[0].Action, Is.EqualTo(SkinIconAction.Download));
            Assert.That(plan.Items[0].ImageUrl, Is.EqualTo("url-AK-47_A"));
            Assert.That(plan.Items[0].ConfigPath, Is.EqualTo("config-AK-47_A"));
            Assert.That(plan.GetSkippedIds(SkinIconSkipReason.AlreadyOk), Is.EqualTo(new[] { "AK-47_B" }));
        }

        [Test]
        public void Build_TargetFileExists_ReusedInsteadOfDownloaded()
        {
            AddSource("AK-47_A");
            _existingFiles.Add($"{Folder}/AK-47_A.png");

            var plan = Build(new List<SkinIconConfigState> { CreateConfig("AK-47_A", null) }, SkinIconsMode.OnlyMissing);

            Assert.That(plan.Items[0].Action, Is.EqualTo(SkinIconAction.ReuseExisting));
            Assert.That(plan.DownloadsCount, Is.EqualTo(0));
            Assert.That(plan.ReusesCount, Is.EqualTo(1));
        }

        [Test]
        public void Build_RepairSharedIconWithBrowserName_OwnerKeepsItAndOtherGetsOwnFile()
        {
            AddSource("AK-47_Predator");
            AddSource("M249_Predator");
            var sharedPath = $"{Folder}/imgi_121_AK-47_Predator.png";
            _existingFiles.Add(sharedPath);
            var configs = new List<SkinIconConfigState>
            {
                CreateConfig("AK-47_Predator", sharedPath),
                CreateConfig("M249_Predator", sharedPath, "M249"),
            };

            var plan = Build(configs, SkinIconsMode.RepairMismatches);

            Assert.That(plan.Items.Select(item => item.TargetPath), Is.EqualTo(new[] { "Assets/Weapons/M249/M249_Predator.png" }));
            Assert.That(plan.GetSkippedIds(SkinIconSkipReason.AlreadyOk), Is.EqualTo(new[] { "AK-47_Predator" }));
        }

        [Test]
        public void Build_RepairSharedIconOfAnotherSkin_EachConfigGetsOwnFileAndSharedFileUntouched()
        {
            AddSource("AK-47_A");
            AddSource("AK-47_B");
            var sharedPath = $"{Folder}/imgi_5_AWP_Other.png";
            _existingFiles.Add(sharedPath);
            var configs = new List<SkinIconConfigState>
            {
                CreateConfig("AK-47_A", sharedPath),
                CreateConfig("AK-47_B", sharedPath),
            };

            var plan = Build(configs, SkinIconsMode.RepairMismatches);

            Assert.That(plan.Items.Select(item => item.TargetPath), Is.EqualTo(new[] { $"{Folder}/AK-47_A.png", $"{Folder}/AK-47_B.png" }));
            Assert.That(plan.Items.All(item => item.Action == SkinIconAction.Download), Is.True);
            Assert.That(plan.Items.Any(item => item.TargetPath == sharedPath), Is.False);
        }

        [Test]
        public void Build_RepairSharedIconNamedAfterOneConfig_OwnerKeepsIt()
        {
            AddSource("AK-47_Predator");
            AddSource("M249_Predator");
            var sharedPath = $"{Folder}/AK-47_Predator.png";
            _existingFiles.Add(sharedPath);
            var configs = new List<SkinIconConfigState>
            {
                CreateConfig("AK-47_Predator", sharedPath),
                CreateConfig("M249_Predator", sharedPath, "M249"),
            };

            var plan = Build(configs, SkinIconsMode.RepairMismatches);

            Assert.That(plan.Items.Select(item => item.Id), Is.EqualTo(new[] { "M249_Predator" }));
            Assert.That(plan.GetSkippedIds(SkinIconSkipReason.AlreadyOk), Is.EqualTo(new[] { "AK-47_Predator" }));
        }

        [Test]
        public void Build_RepairMismatch_BrowserPrefixIgnoredAndWrongNameRepaired()
        {
            AddSource("AK-47_Good");
            AddSource("AK-47_Wrong");
            var configs = new List<SkinIconConfigState>
            {
                CreateConfig("AK-47_Good", $"{Folder}/imgi_60_AK-47_Good.png"),
                CreateConfig("AK-47_Wrong", $"{Folder}/imgi_61_AWP_Pink_DDPAT.png"),
                CreateConfig("AK-47_Missing", null),
            };
            AddSource("AK-47_Missing");

            var plan = Build(configs, SkinIconsMode.RepairMismatches);

            Assert.That(plan.Items.Select(item => item.Id), Is.EqualTo(new[] { "AK-47_Wrong", "AK-47_Missing" }));
            Assert.That(plan.GetSkippedIds(SkinIconSkipReason.AlreadyOk), Is.EqualTo(new[] { "AK-47_Good" }));
        }

        [Test]
        public void Build_ReplaceAll_EveryConfigPointsToOwnFileWithoutOverwriting()
        {
            AddSource("AK-47_A");
            AddSource("AK-47_B");
            AddSource("AK-47_C");
            _existingFiles.Add($"{Folder}/AK-47_B.png");
            var configs = new List<SkinIconConfigState>
            {
                CreateConfig("AK-47_A", $"{Folder}/imgi_1_AK-47_A.png"),
                CreateConfig("AK-47_B", $"{Folder}/imgi_2_AK-47_B.png"),
                CreateConfig("AK-47_C", $"{Folder}/AK-47_C.png"),
            };

            var plan = Build(configs, SkinIconsMode.ReplaceAll);

            Assert.That(plan.Items.Count, Is.EqualTo(2));
            Assert.That(plan.Items[0].Action, Is.EqualTo(SkinIconAction.Download));
            Assert.That(plan.Items[1].Action, Is.EqualTo(SkinIconAction.ReuseExisting));
            Assert.That(plan.GetSkippedIds(SkinIconSkipReason.AlreadyOk), Is.EqualTo(new[] { "AK-47_C" }));
        }

        [Test]
        public void Build_IdNotInSource_SkippedNoSourceAndOthersContinue()
        {
            AddSource("AK-47_A");
            var configs = new List<SkinIconConfigState>
            {
                CreateConfig("CZ-Auto_Vendetta", null, "CZ-Auto"),
                CreateConfig("Desert_Eagle_Sunset_Strom", null, "Desert_Eagle"),
                CreateConfig("AK-47_A", null),
            };

            var plan = Build(configs, SkinIconsMode.OnlyMissing);

            Assert.That(plan.GetSkippedIds(SkinIconSkipReason.NoSource), Is.EqualTo(new[] { "CZ-Auto_Vendetta", "Desert_Eagle_Sunset_Strom" }));
            Assert.That(plan.Items.Select(item => item.Id), Is.EqualTo(new[] { "AK-47_A" }));
        }

        [Test]
        public void Build_PhasedConfig_UsesPhaseSource()
        {
            _sources["Glock-18_Gamma_Doppler_Phase_4"] = new SkinIconSource("Glock-18_Gamma_Doppler_Phase_4", "url-phase-4");
            _sources["Glock-18_Gamma_Doppler"] = new SkinIconSource("Glock-18_Gamma_Doppler", "url-emerald");

            var plan = Build(new List<SkinIconConfigState> { CreateConfig("Glock-18_Gamma_Doppler_Phase_4", null, "Glock-18") }, SkinIconsMode.OnlyMissing);

            Assert.That(plan.Items[0].ImageUrl, Is.EqualTo("url-phase-4"));
            Assert.That(plan.Items[0].TargetPath, Is.EqualTo("Assets/Weapons/Glock-18/Glock-18_Gamma_Doppler_Phase_4.png"));
        }

        [Test]
        public void Build_NoImageOrNoFolder_Skipped()
        {
            _sources["AK-47_NoImage"] = new SkinIconSource("AK-47_NoImage", string.Empty);
            AddSource("Unknown_Weapon_Skin");
            var configs = new List<SkinIconConfigState>
            {
                CreateConfig("AK-47_NoImage", null),
                CreateConfig("Unknown_Weapon_Skin", null, "Unknown"),
            };

            var plan = _planner.Build(configs, _sources, SkinIconsMode.OnlyMissing, weapon => weapon == "AK-47" ? Folder : null, path => false);

            Assert.That(plan.GetSkippedIds(SkinIconSkipReason.NoImage), Is.EqualTo(new[] { "AK-47_NoImage" }));
            Assert.That(plan.GetSkippedIds(SkinIconSkipReason.NoFolder), Is.EqualTo(new[] { "Unknown_Weapon_Skin" }));
        }

        [Test]
        public void Build_TwoIdsSameFileName_SecondSkippedAsSharedFile()
        {
            AddSource("AK-47_A/B");
            AddSource("AK-47_A_B");
            var configs = new List<SkinIconConfigState>
            {
                CreateConfig("AK-47_A/B", null),
                CreateConfig("AK-47_A_B", null),
            };

            var plan = Build(configs, SkinIconsMode.OnlyMissing);

            Assert.That(plan.Items.Select(item => item.Id), Is.EqualTo(new[] { "AK-47_A/B" }));
            Assert.That(plan.GetSkippedIds(SkinIconSkipReason.SharedFile), Is.EqualTo(new[] { "AK-47_A_B" }));
        }

        [Test]
        public void Build_DuplicateConfigsSameId_SecondReusesPlannedFile()
        {
            AddSource("AK-47_A");
            var configs = new List<SkinIconConfigState>
            {
                CreateConfig("AK-47_A", null),
                new SkinIconConfigState("AK-47_A", "AK-47", null, "config-copy"),
            };

            var plan = Build(configs, SkinIconsMode.OnlyMissing);

            Assert.That(plan.Items.Select(item => item.Action), Is.EqualTo(new[] { SkinIconAction.Download, SkinIconAction.ReuseExisting }));
            Assert.That(plan.Items[1].ConfigPath, Is.EqualTo("config-copy"));
        }

        [Test]
        public void FindUnreferenced_ReturnsIconsWithoutConfigs()
        {
            var unreferenced = _planner.FindUnreferenced(new[] { "A/b.png", "A/a.png", "A/c.png" }, new[] { "A/B.png" });

            Assert.That(unreferenced, Is.EqualTo(new[] { "A/a.png", "A/c.png" }));
        }

        private SkinIconsPlan Build(List<SkinIconConfigState> configs, SkinIconsMode mode)
        {
            return _planner.Build(configs, _sources, mode, GetFolder, path => _existingFiles.Contains(path));
        }

        private string GetFolder(string weapon)
        {
            return weapon == "AK-47" ? Folder : $"Assets/Weapons/{weapon}";
        }

        private void AddSource(string id)
        {
            _sources[id] = new SkinIconSource(id, $"url-{id}");
        }

        private SkinIconConfigState CreateConfig(string id, string iconPath, string weapon = "AK-47")
        {
            return new SkinIconConfigState(id, weapon, iconPath, $"config-{id}");
        }
    }
}
