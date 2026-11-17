using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Interpreters;
using Features.RemoteConfigs.Editor.Models;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class FakeInterpreter : IInterpreter
    {
        private readonly string _name;
        private readonly List<string> _log;

        public FakeInterpreter(string name, List<string> log)
        {
            _name = name;
            _log = log;
        }

        public ImportReport Interpret(List<IEntityModel> parsedData)
        {
            _log.Add($"interpret:{_name}");

            var report = new ImportReport();
            report.Updated.Add(_name);

            return report;
        }
    }
}
