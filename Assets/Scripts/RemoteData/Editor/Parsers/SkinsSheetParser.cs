using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using RemoteData.Editor.Models;
using Services.Skins;
using Tools.Mathematics;

namespace RemoteData.Editor.Parsers
{
    [Serializable]
    public class SkinsSheetParser : RemoteSheetParser
    {
        private Regex _rangeRegex = new Regex(@"(\d+)-(\d+)");
        
        protected override List<IEntityModel> ParseDataInternal(string csvRawString)
        {
            CultureInfo customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ",";
            
            var config = new CsvConfiguration(customCulture);
            var stringReader = new StringReader(csvRawString);
            var csvReader = new CsvReader(stringReader, config);

            List<IEntityModel> skinDataModels = new List<IEntityModel>(100);

            csvReader.Read();
            
            while (csvReader.Read())
            {
                SkinDataModel skinData = new SkinDataModel();
                
                skinData.Id = csvReader.GetField(0);
                skinData.Name = csvReader.GetField(1);
                skinData.Rarity = csvReader.GetField<Rarity>(2);

                string priceRange = csvReader.GetField(3);
                Match match = _rangeRegex.Match(priceRange);

                skinData.Price = new IntRange(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
                skinData.AvailableStatTrak = csvReader.GetField<bool>(4);
                skinData.AvailableSouvenir = csvReader.GetField<bool>(5);
                
                skinDataModels.Add(skinData);
            }

            return skinDataModels;
        }
    }
}