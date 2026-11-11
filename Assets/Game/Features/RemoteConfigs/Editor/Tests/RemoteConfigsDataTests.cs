using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class RemoteConfigsDataTests
    {
        [Test]
        public void AllTypeReferences_Resolve()
        {
            var path = "Assets/Game/Features/RemoteConfigs/Editor/Content/Editor Remote Configs Data.asset";
            var text = File.ReadAllText(path);

            var classRefs = Regex.Matches(text, @"classRef: ([^\r\n]+)");
            var typeEntries = Regex.Matches(text, @"type: \{class: (\w+), ns: ([\w.]+), asm: ([\w.]+)\}");

            Assert.That(classRefs.Count + typeEntries.Count, Is.GreaterThan(0));

            foreach (Match match in classRefs)
            {
                Assert.That(Type.GetType(match.Groups[1].Value), Is.Not.Null, match.Value);
            }

            foreach (Match match in typeEntries)
            {
                var name = $"{match.Groups[2].Value}.{match.Groups[1].Value}, {match.Groups[3].Value}";
                Assert.That(Type.GetType(name), Is.Not.Null, match.Value);
            }
        }
    }
}
