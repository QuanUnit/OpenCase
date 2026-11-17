using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Features.RemoteConfigs.Editor.Models;
using Rotorz.Games.Reflection;
using Shared.Tools.Web;
using UnityEngine;

namespace Features.RemoteConfigs.Editor.Parsers
{
    [Serializable]
    public abstract class RemoteSheetParser : IRemoteSheetParser
    {
        [SerializeField] [ClassExtends(typeof(IEntityModel))] protected ClassTypeReference _modelType;
    
        public async Task<List<IEntityModel>> ParseData(string sheetId, string pageId)
        {
            var csvData = await DownloadTools.DownloadGoogleSheetAsCsv(sheetId, pageId);
            var parsedData = ParseDataInternal(csvData);
            return parsedData;
        }

        public List<IEntityModel> ParseCsv(string csvRawString)
        {
            return ParseDataInternal(csvRawString);
        }

        protected abstract List<IEntityModel> ParseDataInternal(string csvRawString);

        public Type GetModelType()
        {
            return _modelType.Type;
        }
    }
}