using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Models;

namespace Features.RemoteConfigs.Editor.Exporters.Skins
{
    public class SkinsSheetMerger
    {
        public SkinsSheetMergeResult Merge(IReadOnlyList<SkinDataModel> existingRows, IReadOnlyList<SkinDataModel> sourceRows, ICollection<string> skippedIds)
        {
            var result = new SkinsSheetMergeResult();
            var sourceRowsById = new Dictionary<string, SkinDataModel>(sourceRows.Count);

            foreach (var sourceRow in sourceRows)
            {
                sourceRowsById[sourceRow.Id] = sourceRow;
            }

            var usedIds = new HashSet<string>();

            foreach (var existingRow in existingRows)
            {
                if (usedIds.Contains(existingRow.Id) == false && sourceRowsById.TryGetValue(existingRow.Id, out var sourceRow) == true)
                {
                    result.Rows.Add(sourceRow);
                    result.UpdatedIds.Add(existingRow.Id);
                    usedIds.Add(existingRow.Id);
                    continue;
                }

                result.Rows.Add(existingRow);

                if (skippedIds.Contains(existingRow.Id) == true)
                {
                    result.KeptSkippedIds.Add(existingRow.Id);
                }
                else
                {
                    result.KeptIds.Add(existingRow.Id);
                }
            }

            foreach (var sourceRow in sourceRows)
            {
                if (usedIds.Add(sourceRow.Id) == true)
                {
                    result.Rows.Add(sourceRow);
                    result.AddedIds.Add(sourceRow.Id);
                }
            }

            GroupRowsByWeapon(result.Rows);

            return result;
        }

        private void GroupRowsByWeapon(List<SkinDataModel> rows)
        {
            var rowsByWeapon = new Dictionary<string, List<SkinDataModel>>();
            var weaponsOrder = new List<string>();

            foreach (var row in rows)
            {
                var weapon = SkinIds.GetWeapon(row.Id, row.Name);

                if (rowsByWeapon.TryGetValue(weapon, out var weaponRows) == false)
                {
                    weaponRows = new List<SkinDataModel>();
                    rowsByWeapon.Add(weapon, weaponRows);
                    weaponsOrder.Add(weapon);
                }

                weaponRows.Add(row);
            }

            rows.Clear();

            foreach (var weapon in weaponsOrder)
            {
                rows.AddRange(rowsByWeapon[weapon]);
            }
        }
    }
}
