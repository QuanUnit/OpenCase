using System.Linq;
using Features.Cases.Configs;
using Features.Cases.Data;
using NUnit.Framework;
using Shared.Tools.AssetsExtensions;

namespace Features.Cases.Tests.EditMode
{
    public class CasesContentIdsTests
    {
        [Test]
        public void CaseConfigs_Ids_AreFilledUniqueAndPrefixed()
        {
            var ids = AssetDatabaseTools.FindAllAssets<CaseConfig>().Select(config => config.Id).ToList();

            Assert.That(ids, Is.Not.Empty);
            Assert.That(ids.Where(id => id.Value.StartsWith(CaseId.EntityName + "_") == false).Select(id => id.Value), Is.Empty);
            Assert.That(ids.GroupBy(id => id).Where(group => group.Count() > 1).Select(group => group.Key.Value), Is.Empty);
        }
    }
}
