using Cysharp.Threading.Tasks;

namespace Shared.Lifecycle
{
    public interface IAssetsLoader
    {
        public UniTask LoadAssets();
        public void ReleaseAssets();
    }
}
