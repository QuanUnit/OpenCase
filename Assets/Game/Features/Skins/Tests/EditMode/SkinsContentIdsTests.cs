using System.Collections.Generic;
using System.Linq;
using Features.Skins.Configs;
using Features.Skins.Data;
using NUnit.Framework;
using Shared.Tools.AssetsExtensions;

namespace Features.Skins.Tests.EditMode
{
    public class SkinsContentIdsTests
    {
        [Test]
        public void SkinConfigs_Ids_AreFilledAndUnique()
        {
            var ids = AssetDatabaseTools.FindAllAssets<SkinConfig>().Select(config => config.Id).ToList();

            Assert.That(ids, Is.Not.Empty);
            Assert.That(ids.Where(id => id.IsEmpty == true), Is.Empty);
            Assert.That(ids.GroupBy(id => id).Where(group => group.Count() > 1).Select(group => group.Key.Value), Is.Empty);
        }

        [Test]
        public void CollectionsRegistry_Ids_AreFilledUniqueAndPrefixed()
        {
            var entries = AssetDatabaseTools.FindAllAssets<SkinCollectionsConfig>().Single().Collections;

            Assert.That(entries.Where(entry => entry.Id.IsEmpty == true || string.IsNullOrEmpty(entry.Name) == true), Is.Empty);
            Assert.That(entries.Where(entry => entry.Id.Value.StartsWith(SkinCollectionId.EntityName + "_") == false).Select(entry => entry.Id.Value), Is.Empty);
            Assert.That(entries.GroupBy(entry => entry.Id).Where(group => group.Count() > 1).Select(group => group.Key.Value), Is.Empty);
        }

        [Test]
        public void SkinConfigs_Collections_ExistInRegistry()
        {
            var registered = new HashSet<SkinCollectionId>(AssetDatabaseTools.FindAllAssets<SkinCollectionsConfig>().Single().Collections.Select(entry => entry.Id));

            var unknown = AssetDatabaseTools
                .FindAllAssets<SkinConfig>()
                .Where(config => config.Collection.IsEmpty == false && registered.Contains(config.Collection) == false)
                .Select(config => config.Id.Value + " -> " + config.Collection.Value);

            Assert.That(unknown, Is.Empty);
        }
    }
}
