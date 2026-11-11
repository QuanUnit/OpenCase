using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Models;

namespace Features.RemoteConfigs.Editor.Interpreters
{
    public interface IInterpreter
    {
        public void Interpret(List<IEntityModel> parsedData);
    }
}