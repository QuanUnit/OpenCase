using Features.RemoteConfigs.Editor.Interpreters;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class ImportReportTests
    {
        [Test]
        public void GetFullSummary_WithProblems_ListsThem()
        {
            var report = new ImportReport { PageName = "Cases" };
            report.Updated.Add("Case_1");
            report.UnknownReferences.Add("Case_1: X");
            report.Missing.Add("Case_Old");

            var summary = report.GetFullSummary();

            Assert.That(report.HasProblems, Is.True);
            Assert.That(summary, Does.Contain("Cases: created 0, updated 1, not in sheet 1, unknown references 1, errors 0"));
            Assert.That(summary, Does.Contain("Case_1: X"));
            Assert.That(summary, Does.Contain("Case_Old"));
        }

        [Test]
        public void HasProblems_OnlyUpdates_False()
        {
            var report = new ImportReport { PageName = "Skins" };
            report.Updated.Add("AK-47_Redline");

            Assert.That(report.HasProblems, Is.False);
        }
    }
}
