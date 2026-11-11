using System;
using System.Collections.Generic;
using Features.Inventory.Services;
using Features.Skins.Data;
using NUnit.Framework;
using QCore.Tools.Identifiers;
using UnityEngine;

namespace Features.Inventory.Tests.EditMode
{
    public class InventoryServiceTests
    {
        private Skin CreateSkin()
        {
            return new Skin(QGuid.CreateNew(), QGuid.CreateNew(), 0.1f, 5f, false, false);
        }

        [Test]
        public void Add_Skin_RaisesAddedAndStores()
        {
            var inventory = new InventoryService();
            var skin = CreateSkin();
            var added = new List<Skin>();
            inventory.Added += added.Add;

            inventory.Add(skin);

            Assert.AreEqual(1, added.Count);
            Assert.AreSame(skin, added[0]);
            Assert.AreSame(skin, inventory.Get(skin.Id));
        }

        [Test]
        public void Add_SameIdTwice_Throws()
        {
            var inventory = new InventoryService();
            var skin = CreateSkin();
            inventory.Add(skin);

            Assert.Throws<ArgumentException>(() => inventory.Add(skin));
        }

        [Test]
        public void Remove_ExistingSkin_ReturnsTrueAndRaisesRemoved()
        {
            var inventory = new InventoryService();
            var skin = CreateSkin();
            var removed = new List<Skin>();
            inventory.Add(skin);
            inventory.Removed += removed.Add;

            var result = inventory.Remove(skin);

            Assert.IsTrue(result == true);
            Assert.AreEqual(1, removed.Count);
            Assert.AreSame(skin, removed[0]);
            Assert.IsTrue(inventory.TryGet(skin.Id, out _) == false);
        }

        [Test]
        public void Remove_MissingId_ReturnsFalseAndDoesNotRaise()
        {
            var inventory = new InventoryService();
            var raised = false;
            inventory.Removed += skin => raised = true;

            var result = inventory.Remove(QGuid.CreateNew());

            Assert.IsTrue(result == false);
            Assert.IsTrue(raised == false);
        }

        [Test]
        public void Get_MissingId_ThrowsMissingReferenceException()
        {
            var inventory = new InventoryService();

            Assert.Throws<MissingReferenceException>(() => inventory.Get(QGuid.CreateNew()));
        }

        [Test]
        public void TryGet_MissingId_ReturnsFalse()
        {
            var inventory = new InventoryService();

            var result = inventory.TryGet(QGuid.CreateNew(), out _);

            Assert.IsTrue(result == false);
        }
    }
}
