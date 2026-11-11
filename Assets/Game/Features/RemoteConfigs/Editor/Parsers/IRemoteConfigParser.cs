using System.Collections.Generic;
using System.Threading.Tasks;
using Features.RemoteConfigs.Editor.Models;

namespace Features.RemoteConfigs.Editor.Parsers
{
    public interface IRemoteSheetParser
    {
        public Task<List<IEntityModel>> ParseData(string sheetId, string pageId);
    }
}