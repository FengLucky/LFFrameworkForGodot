using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace LF.Runtime
{
    public sealed class UIRawImage : RawImage
    {
        private string _path;
        private AssetHandle _handle;
        private int _asyncVersion = 0;

        public void SetTexture(Texture2D t)
        {
            _asyncVersion++;
            _path = string.Empty;
            texture = t;
            _handle?.Release();
            _handle = null;
        }

        public void SetTexture(string path)
        {
            if (_path == path)
            {
                return;
            }

            _asyncVersion++;
            _handle?.Release();
            _path = path;
            _handle =  YooAssets.LoadAssetSync<Texture2D>(path);
            texture = _handle.GetAssetObject<Texture2D>();
        }
       
        public async UniTask SetTextureAsync(string path)
        {
            if (_path == path)
            {
                return;
            }
            
            _asyncVersion++;
            var version = _asyncVersion;
            _path = path;

            var handle = YooAssets.LoadAssetAsync<Texture2D>(path);
            await handle.ToUniTask();
            
            if (!gameObject || version != _asyncVersion)
            {
                handle.Release();
                return;
            }

            _handle?.Release();
            _handle = handle;
            texture = handle.GetAssetObject<Texture2D>();
        }

        public void ClearTexture()
        {
            _asyncVersion++;
            _handle?.Release();
            _handle = null;
            _path = null;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearTexture();
        }
    }
}