using UnityEngine;

namespace Services.Wallet.Configs
{
    public abstract class WalletConfig : ScriptableObject
    {
        public float DefaultValue => _defaultValue;
        
        [SerializeField] [Min(0)] private float _defaultValue;
    }
}