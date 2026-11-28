using System.Collections.Generic;
using System.Linq;
using Features.Cases.Configs;
using Features.Cases.Data;
using Features.Cases.Services;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Features.Cases.Tests.EditMode
{
    public class CasesDatabaseServiceTests
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var createdObject in _createdObjects)
            {
                Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
        }

        [Test]
        public void AllCases_AfterInit_CasesSortedByPriceInsideCategory()
        {
            var config = CreateConfig(
                ("Weapons", new[] { CreateCase("Case_C", 30f), CreateCase("Case_A", 1f), CreateCase("Case_B", 5f) }),
                ("Knives", new[] { CreateCase("Case_K2", 500f), CreateCase("Case_K1", 100f) }));
            var service = new CasesDatabaseService(config);

            service.Init();

            Assert.That(service.AllCases.Select(pair => pair.Key.Name), Is.EqualTo(new[] { "Weapons", "Knives" }));
            Assert.That(service.AllCases[0].Value.Select(caseConfig => caseConfig.Id.Value), Is.EqualTo(new[] { "Case_A", "Case_B", "Case_C" }));
            Assert.That(service.AllCases[1].Value.Select(caseConfig => caseConfig.Id.Value), Is.EqualTo(new[] { "Case_K1", "Case_K2" }));
        }

        [Test]
        public void AllCases_AfterInit_ConfigOrderUntouched()
        {
            var config = CreateConfig(("Weapons", new[] { CreateCase("Case_C", 30f), CreateCase("Case_A", 1f) }));
            var service = new CasesDatabaseService(config);

            service.Init();

            Assert.That(config.CategoriesMap[0].Value.Select(caseConfig => caseConfig.Id.Value), Is.EqualTo(new[] { "Case_C", "Case_A" }));
        }

        private AllCasesConfig CreateConfig(params (string Name, CaseConfig[] Cases)[] categories)
        {
            var config = ScriptableObject.CreateInstance<AllCasesConfig>();
            _createdObjects.Add(config);
            var serialized = new SerializedObject(config);
            var map = serialized.FindProperty("_categoriesMap");
            map.arraySize = categories.Length;

            for (var index = 0; index < categories.Length; index++)
            {
                var element = map.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("Key").FindPropertyRelative("Name").stringValue = categories[index].Name;
                var cases = element.FindPropertyRelative("Value");
                cases.arraySize = categories[index].Cases.Length;

                for (var caseIndex = 0; caseIndex < categories[index].Cases.Length; caseIndex++)
                {
                    cases.GetArrayElementAtIndex(caseIndex).objectReferenceValue = categories[index].Cases[caseIndex];
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();

            return config;
        }

        private CaseConfig CreateCase(string id, float price)
        {
            var caseConfig = ScriptableObject.CreateInstance<CaseConfig>();
            caseConfig.Id = new CaseId(id);
            caseConfig.Price = price;
            _createdObjects.Add(caseConfig);

            return caseConfig;
        }
    }
}
