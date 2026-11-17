using System.Collections.Generic;
using System.Threading.Tasks;
using Features.RemoteConfigs.Editor.Models;
using Features.RemoteConfigs.Editor.Parsers;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class FakeSheetParser : IRemoteSheetParser
    {
        private readonly string _name;
        private readonly List<string> _log;
        private readonly Task<List<IEntityModel>> _result;

        public FakeSheetParser(string name, List<string> log, Task<List<IEntityModel>> result)
        {
            _name = name;
            _log = log;
            _result = result;
        }

        public Task<List<IEntityModel>> ParseData(string sheetId, string pageId)
        {
            _log.Add($"parse:{_name}");
            return _result;
        }
    }
}
