using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Shared.Editor.Identifiers
{
    public class EntityIdDropdown : AdvancedDropdown
    {
        private const string NoneLabel = "<none>";
        private const float MinHeight = 320f;

        private readonly string _title;
        private readonly IReadOnlyList<EntityIdOption> _options;
        private readonly Action<string> _selected;

        public EntityIdDropdown(string title, IReadOnlyList<EntityIdOption> options, Action<string> selected) : base(new AdvancedDropdownState())
        {
            _title = title;
            _options = options;
            _selected = selected;

            minimumSize = new Vector2(minimumSize.x, MinHeight);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(_title);
            var groups = new Dictionary<string, AdvancedDropdownItem>();

            root.AddChild(new Item(NoneLabel, string.Empty));

            foreach (var option in _options.OrderBy(option => option.Group).ThenBy(option => option.Value))
            {
                var parent = root;

                if (string.IsNullOrEmpty(option.Group) == false)
                {
                    if (groups.TryGetValue(option.Group, out parent) == false)
                    {
                        parent = new AdvancedDropdownItem(option.Group);
                        groups.Add(option.Group, parent);
                        root.AddChild(parent);
                    }
                }

                parent.AddChild(new Item(option.Value, option.Value));
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is Item idItem)
            {
                _selected.Invoke(idItem.Value);
            }
        }

        private class Item : AdvancedDropdownItem
        {
            public readonly string Value;

            public Item(string name, string value) : base(name)
            {
                Value = value;
            }
        }
    }
}
