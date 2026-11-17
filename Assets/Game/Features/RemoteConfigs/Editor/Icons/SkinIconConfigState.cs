namespace Features.RemoteConfigs.Editor.Icons
{
    public class SkinIconConfigState
    {
        public readonly string Id;
        public readonly string Weapon;
        public readonly string CurrentIconPath;
        public readonly string ConfigPath;

        public SkinIconConfigState(string id, string weapon, string currentIconPath, string configPath)
        {
            Id = id;
            Weapon = weapon;
            CurrentIconPath = currentIconPath;
            ConfigPath = configPath;
        }
    }
}
