using System;
using System.Collections.Generic;
using System.Linq;

namespace Features.RemoteConfigs.Editor.Interpreters
{
    public class ImportIdsComparer
    {
        public List<string> FindMissing(IEnumerable<string> existingIds, IEnumerable<string> importedIds)
        {
            var importedSet = new HashSet<string>(importedIds);

            return existingIds
                .Where(id => importedSet.Contains(id) == false)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();
        }
    }
}
