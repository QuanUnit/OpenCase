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
            return ParseCsv(csvData);
        }

        public List<IEntityModel> ParseCsv(string csvRawString)
        {
            if (string.IsNullOrEmpty(csvRawString) == true)
            {
                throw new InvalidOperationException("Sheet page could not be downloaded or is empty");
            }

            return ParseDataInternal(csvRawString);
        }

        protected abstract List<IEntityModel> ParseDataInternal(string csvRawString);

        public Type GetModelType()
        {
            return _modelType.Type;
        }
    }
}