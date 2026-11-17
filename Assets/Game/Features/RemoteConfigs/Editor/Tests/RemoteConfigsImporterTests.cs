using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Features.RemoteConfigs.Editor.Models;
using Features.RemoteConfigs.Editor.Parsers;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class RemoteConfigsImporterTests
    {
        private SynchronizationContext _previousContext;
        private RemoteConfigsImporter _importer;
        private List<string> _log;

        [SetUp]
        public void SetUp()
        {
            _previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);

            _importer = new RemoteConfigsImporter();
            _log = new List<string>();
        }

        [TearDown]
        public void TearDown()
        {
            SynchronizationContext.SetSynchronizationContext(_previousContext);
        }

        [Test]
        public void Import_FirstPageNotDownloadedYet_SecondPageNotStarted()
        {
            var firstPageSource = new TaskCompletionSource<List<IEntityModel>>();
            var pages = new List<EditorRemoteConfigsData.Page>
            {
                CreatePage("Skins", firstPageSource.Task),
                CreatePage("Cases", Task.FromResult(new List<IEntityModel>())),
            };

            var importTask = _importer.Import("sheet", pages, (name, index) => { });

            Assert.That(_log, Is.EqualTo(new[] { "parse:Skins" }));

            firstPageSource.SetResult(new List<IEntityModel>());

            Assert.That(importTask.IsCompleted, Is.True);
            Assert.That(_log, Is.EqualTo(new[] { "parse:Skins", "interpret:Skins", "parse:Cases", "interpret:Cases" }));
        }

        [Test]
        public void Import_PageDownloadFails_ErrorReportedAndNextPageImported()
        {
            var failedDownload = Task.FromException<List<IEntityModel>>(new InvalidOperationException("Sheet page could not be downloaded or is empty"));
            var pages = new List<EditorRemoteConfigsData.Page>
            {
                CreatePage("Skins", failedDownload),
                CreatePage("Cases", Task.FromResult(new List<IEntityModel>())),
            };

            var reports = _importer.Import("sheet", pages, (name, index) => { }).Result;

            Assert.That(reports.Count, Is.EqualTo(2));
            Assert.That(reports[0].PageName, Is.EqualTo("Skins"));
            Assert.That(reports[0].Errors.Count, Is.EqualTo(1));
            Assert.That(reports[0].Errors[0], Does.Contain("could not be downloaded"));
            Assert.That(reports[1].PageName, Is.EqualTo("Cases"));
            Assert.That(reports[1].Errors, Is.Empty);
            Assert.That(_log, Is.EqualTo(new[] { "parse:Skins", "parse:Cases", "interpret:Cases" }));
        }

        [Test]
        public void ParseCsv_NullOrEmpty_ThrowsClearError()
        {
            var parser = new SkinsSheetParser();

            Assert.That(() => parser.ParseCsv(null), Throws.InvalidOperationException.With.Message.Contains("could not be downloaded"));
            Assert.That(() => parser.ParseCsv(string.Empty), Throws.InvalidOperationException);
        }

        private EditorRemoteConfigsData.Page CreatePage(string name, Task<List<IEntityModel>> parseResult)
        {
            return new EditorRemoteConfigsData.Page
            {
                Name = name,
                Id = name,
                Parser = new FakeSheetParser(name, _log, parseResult),
                Interpreter = new FakeInterpreter(name, _log),
            };
        }
    }
}
