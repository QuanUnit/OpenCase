using Cysharp.Threading.Tasks;
using Shared.Lifecycle;

namespace Kernel.Tests.EditMode
{
    public class FakeAssetsLoader : IAssetsLoader
    {
        public int LoadCount { get; private set; }
        public int ReleaseCount { get; private set; }

        public UniTask LoadAssets()
        {
            LoadCount++;

            return UniTask.CompletedTask;
        }

        public void ReleaseAssets()
        {
            ReleaseCount++;
        }
    }
}
