using System;
using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Interpreters;
using Features.RemoteConfigs.Editor.Parsers;
using UnityEngine;

namespace Features.RemoteConfigs.Editor
{
    [CreateAssetMenu(fileName = "Editor Remote Configs Data", menuName = "Configs/Editor/Editor Remote Configs Data")]
    public class EditorRemoteConfigsData: ScriptableObject
    {
        public string SheetId;
        
        public List<Page> Pages;
        
        [Serializable]
        public struct Page
        {
            public string Name;
            public string Id;

            [SerializeReference] [SubclassSelector] public IRemoteSheetParser Parser;
            [SerializeReference] [SubclassSelector] public IInterpreter Interpreter;
        }
    }
}