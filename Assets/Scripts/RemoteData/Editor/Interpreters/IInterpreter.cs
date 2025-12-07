using System.Collections.Generic;
using RemoteData.Editor.Models;

namespace RemoteData.Editor.Interpreters
{
    public interface IInterpreter
    {
        public void Interpret(List<IEntityModel> parsedData);
    }
}