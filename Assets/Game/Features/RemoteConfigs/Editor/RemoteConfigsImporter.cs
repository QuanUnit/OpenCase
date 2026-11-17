using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Features.RemoteConfigs.Editor.Interpreters;

namespace Features.RemoteConfigs.Editor
{
    public class RemoteConfigsImporter
    {
        public async Task<List<ImportReport>> Import(string sheetId, IReadOnlyList<EditorRemoteConfigsData.Page> pages, Action<string, int> pageStarted)
        {
            var reports = new List<ImportReport>(pages.Count);

            for (var index = 0; index < pages.Count; index++)
            {
                pageStarted(pages[index].Name, index);

                var report = await ImportPage(sheetId, pages[index]);
                reports.Add(report);
            }

            return reports;
        }

        private async Task<ImportReport> ImportPage(string sheetId, EditorRemoteConfigsData.Page page)
        {
            ImportReport report;

            try
            {
                var parsedEntities = await page.Parser.ParseData(sheetId, page.Id);
                report = page.Interpreter.Interpret(parsedEntities);
            }
            catch (Exception exception)
            {
                report = new ImportReport();
                report.Errors.Add($"{exception.GetType().Name}: {exception.Message}");
            }

            report.PageName = page.Name;
            return report;
        }
    }
}
