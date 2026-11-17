using Features.RemoteConfigs.Editor.Interpreters;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class ImportIdsComparerTests
    {
        [Test]
        public void FindMissing_ExistingIdsAbsentInImport_ReturnedSorted()
        {
            var missing = new ImportIdsComparer().FindMissing(new[] { "C", "A", "B", "D" }, new[] { "B", "E" });

            Assert.That(missing, Is.EqualTo(new[] { "A", "C", "D" }));
        }
    }
}
