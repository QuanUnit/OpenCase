using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using RemoteData.Editor.Models;

namespace RemoteData.Editor.Parsers
{
    [Serializable]
    public class CasesSheetParser : RemoteSheetParser
    {
        protected override List<IEntityModel> ParseDataInternal(string csvRawString)
        {
            CultureInfo customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ",";

            var config = new CsvConfiguration(customCulture);
            var stringReader = new StringReader(csvRawString);
            var csvReader = new CsvReader(stringReader, config);

            List<IEntityModel> caseDataModels = new List<IEntityModel>(50);

            csvReader.Read();

            CaseDataModel currentCase = null;

            while (csvReader.Read())
            {
                string id = csvReader.GetField(0);
                string name = csvReader.GetField(1);
                string priceStr = csvReader.GetField(2);
                string skinId = csvReader.GetField(3);
                string weightStr = csvReader.GetField(4);

                if (!string.IsNullOrEmpty(id))
                {
                    currentCase = new CaseDataModel
                    {
                        Id = id,
                        Name = name,
                        Price = float.TryParse(priceStr, NumberStyles.Float, customCulture, out float price) ? price : 0f,
                        SkinsMap = new Dictionary<string, float>()
                    };

                    caseDataModels.Add(currentCase);
                }

                if (currentCase != null && !string.IsNullOrEmpty(skinId))
                {
                    float weight = float.TryParse(weightStr, NumberStyles.Float, customCulture, out float w) ? w : 0f;
                    currentCase.SkinsMap[skinId] = weight;
                }
            }

            return caseDataModels;
        }
    }
}