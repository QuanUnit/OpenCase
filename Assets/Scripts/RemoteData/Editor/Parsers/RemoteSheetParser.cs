using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RemoteData.Editor.Models;
using Rotorz.Games.Reflection;
using Tools.Web;
using UnityEngine;

namespace RemoteData.Editor.Parsers
{
    [Serializable]
    public abstract class RemoteSheetParser : IRemoteSheetParser
    {
        [SerializeField] [ClassExtends(typeof(IEntityModel))] protected ClassTypeReference _modelType;
    
        public async Task<List<IEntityModel>> ParseData(string sheetId, string pageId)
        {
            string csvData = await DownloadTools.DownloadGoogleSheetAsCsv(sheetId, pageId);
            List<IEntityModel> parsedData = ParseDataInternal(csvData);
            return parsedData;
        }

        protected abstract List<IEntityModel> ParseDataInternal(string csvRawString);

        public Type GetModelType()
        {
            return _modelType.Type;
        }
    }
}