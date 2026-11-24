using Features.Skins.Configs;

namespace Features.RemoteConfigs.Editor.Cases
{
    public class CaseSkinInfo
    {
        public readonly SkinConfig Config;
        public readonly string Weapon;
        public readonly string DisplayName;
        public readonly string SearchText;

        public CaseSkinInfo(SkinConfig config, string weapon)
        {
            Config = config;
            Weapon = weapon;
            DisplayName = $"{weapon} | {config.Name}";
            SearchText = $"{weapon} {config.Name} {config.Id.Value}".ToLowerInvariant();
        }
    }
}
