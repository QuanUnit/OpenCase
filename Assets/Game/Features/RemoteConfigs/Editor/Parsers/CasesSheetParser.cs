using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using Features.RemoteConfigs.Editor.Models;

namespace Features.RemoteConfigs.Editor.Parsers
{
    [Serializable]
    public class CasesSheetParser : RemoteSheetParser
    {
        protected override List<IEntityModel> ParseDataInternal(string csvRawString)
        {
            var customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ",";

            var config = new CsvConfiguration(customCulture);
            var stringReader = new StringReader(csvRawString);
            var csvReader = new CsvReader(stringReader, config);

            var caseDataModels = new List<IEntityModel>(50);

            csvReader.Read();

            CaseDataModel currentCase = null;

            while (csvReader.Read())
            {
                var id = csvReader.GetField(0);
                var name = csvReader.GetField(1);
                var priceStr = csvReader.GetField(2);
                var skinId = csvReader.GetField(3);
                var weightStr = csvReader.GetField(4);

                if (!string.IsNullOrEmpty(id))
                {
                    currentCase = new CaseDataModel
                    {
                        Id = id,
                        Name = name,
                        Price = float.TryParse(priceStr, NumberStyles.Float, customCulture, out var price) ? price : 0f,
                        SkinsMap = new Dictionary<string, float>()
                    };

                    caseDataModels.Add(currentCase);
                }

                if (currentCase != null && !string.IsNullOrEmpty(skinId))
                {
                    var weight = float.TryParse(weightStr, NumberStyles.Float, customCulture, out var w) ? w : 0f;
                    currentCase.SkinsMap[skinId] = weight;
                }
            }

            return caseDataModels;
        }
    }
}