using System.Collections.Generic;
using System.Threading.Tasks;
using RemoteData.Editor.Models;

namespace RemoteData.Editor.Parsers
{
    public interface IRemoteSheetParser
    {
        public Task<List<IEntityModel>> ParseData(string sheetId, string pageId);
    }
}