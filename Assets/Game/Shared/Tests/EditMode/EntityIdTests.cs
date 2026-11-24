using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shared.Identifiers;
using UnityEngine;
using EntityId = Shared.Identifiers.EntityId;

namespace Shared.Tests.EditMode
{
    public class EntityIdTests
    {
        [Test]
        public void Equals_SameTypeAndValue_ReturnsTrue()
        {
            var first = new FirstTestId("Entity_Variant");
            var second = new FirstTestId("Entity_Variant");

            Assert.That(first == second, Is.True);
            Assert.That(first.Equals(second), Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }

        [Test]
        public void Equals_DifferentValue_ReturnsFalse()
        {
            var first = new FirstTestId("Entity_First");
            var second = new FirstTestId("Entity_Second");

            Assert.That(first != second, Is.True);
        }

        [Test]
        public void Equals_DifferentCase_ReturnsFalse()
        {
            Assert.That(new FirstTestId("Entity_Variant") == new FirstTestId("entity_variant"), Is.False);
        }

        [Test]
        public void Equals_DifferentTypesWithSameValue_ReturnsFalse()
        {
            EntityId first = new FirstTestId("Entity_Variant");
            EntityId second = new SecondTestId("Entity_Variant");

            Assert.That(first == second, Is.False);
            Assert.That(first.Equals((object)second), Is.False);
        }

        [Test]
        public void EqualityOperator_Null_ComparesByNull()
        {
            FirstTestId missing = null;

            Assert.That(missing == null, Is.True);
            Assert.That(new FirstTestId("Entity_Variant") == null, Is.False);
            Assert.That(null == new FirstTestId("Entity_Variant"), Is.False);
        }

        [Test]
        public void Dictionary_KeyWithEqualValue_FindsEntry()
        {
            var map = new Dictionary<FirstTestId, int>()
            {
                { new FirstTestId("Entity_Variant"), 7 },
            };

            Assert.That(map[new FirstTestId("Entity_Variant")], Is.EqualTo(7));
        }

        [Test]
        public void IsEmpty_DefaultId_ReturnsTrue()
        {
            var id = new FirstTestId();

            Assert.That(id.IsEmpty, Is.True);
            Assert.That(id.Value, Is.EqualTo(string.Empty));
            Assert.That(id, Is.EqualTo(new FirstTestId(null)));
        }

        [Test]
        public void Parse_ToStringValue_RoundTrips()
        {
            var id = new FirstTestId("Entity_Variant");

            var parsed = EntityIds.Parse<FirstTestId>(id.ToString());

            Assert.That(parsed, Is.EqualTo(id));
        }

        [Test]
        public void CreateUnique_TwoCalls_ReturnsDifferentIdsWithEntityName()
        {
            var first = EntityIds.CreateUnique<FirstTestId>("Entity");
            var second = EntityIds.CreateUnique<FirstTestId>("Entity");

            Assert.That(first, Is.Not.EqualTo(second));
            Assert.That(first.Value, Does.StartWith("Entity_"));
        }

        [TestCase("The Vertigo", "The_Vertigo")]
        [TestCase("Dreams & Nightmares", "Dreams_And_Nightmares")]
        [TestCase("St. Marc", "St_Marc")]
        [TestCase("  Dust 2  ", "Dust_2")]
        public void ToVariant_Text_ReturnsUnderscoredWords(string text, string expected)
        {
            Assert.That(EntityIds.ToVariant(text), Is.EqualTo(expected));
        }

        [Test]
        public void Compose_EntityAndVariant_JoinsWithSeparator()
        {
            var id = EntityIds.Compose<FirstTestId>("Collection", "Dust 2");

            Assert.That(id.Value, Is.EqualTo("Collection_Dust_2"));
        }

        [Test]
        public void JsonUtility_Container_RoundTripsId()
        {
            var container = new TestContainer() { Id = new FirstTestId("Entity_Variant") };

            var restored = JsonUtility.FromJson<TestContainer>(JsonUtility.ToJson(container));

            Assert.That(restored.Id, Is.EqualTo(container.Id));
        }

        [Test]
        public void ConcreteIds_AllSealedWithStringConstructor()
        {
            var types = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(assembly => assembly.GetName().Name.StartsWith("Features") || assembly.GetName().Name.StartsWith("Shared"))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsAbstract == false && typeof(EntityId).IsAssignableFrom(type))
                .ToList();

            Assert.That(types, Is.Not.Empty);

            foreach (var type in types)
            {
                Assert.That(type.IsSealed, Is.True, type.FullName);
                Assert.That(type.GetConstructor(new[] { typeof(string) }), Is.Not.Null, type.FullName);
                Assert.That(type.GetConstructor(Type.EmptyTypes), Is.Not.Null, type.FullName);
                Assert.That(type.IsDefined(typeof(SerializableAttribute), false), Is.True, type.FullName);
            }
        }

        [Serializable]
        public sealed class FirstTestId : EntityId
        {
            public FirstTestId()
            {
            }

            public FirstTestId(string value) : base(value)
            {
            }
        }

        [Serializable]
        public sealed class SecondTestId : EntityId
        {
            public SecondTestId()
            {
            }

            public SecondTestId(string value) : base(value)
            {
            }
        }

        [Serializable]
        private class TestContainer
        {
            public FirstTestId Id;
        }
    }
}
