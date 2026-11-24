namespace Shared.Editor.Identifiers
{
    public readonly struct EntityIdOption
    {
        public readonly string Value;
        public readonly string Group;

        public EntityIdOption(string value, string group)
        {
            Value = value;
            Group = group;
        }
    }
}
