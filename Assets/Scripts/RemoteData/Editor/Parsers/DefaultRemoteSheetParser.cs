using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using RemoteData.Editor.Models;

namespace RemoteData.Editor.Parsers
{
    [Serializable]
    public class DefaultRemoteSheetParser : RemoteSheetParser
    {
        protected override List<IEntityModel> ParseDataInternal(string csvRawString)
        {
            CultureInfo customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ",";
            
            var config = new CsvConfiguration(customCulture);

            var stringReader = new StringReader(csvRawString);
            var csvReader = new CsvReader(stringReader, config);
            return csvReader.GetRecords(GetModelType()).Cast<IEntityModel>().ToList();
        }
    }
}