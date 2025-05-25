using Config;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using YooAsset;

namespace LF.Runtime
{
    public sealed class UIImage : Image
    {
        private AssetHandle _handle;
        private SpriteBean _bean;
        private int _asyncVersion = 0;
        
        public void SetSprite(SpriteBean bean)
        {
            if (!bean.IsValid())
            {
                return;
            }
            
            if (_bean == bean)
            {
                return;
            }
            
            _asyncVersion++;
            _handle?.Release();
            _handle = YooAssets.LoadAssetSync(bean.GetAtlasPath());
            var obj = _handle.AssetObject;
            if (obj)
            {
                if (obj is AtlasAsset atlasAsset)
                {
                    sprite = atlasAsset.GetSprite(bean.Sprite);
                }
                else if (obj is SpriteAtlas atlas)
                {
                    sprite = atlas.GetSprite(bean.Sprite);
                }
                else
                {
                    Debug.LogError($"{obj.GetType()} is not AtlasAsset or SpriteAtlas");
                }
            }
            _bean = bean;
        }
        
        public async UniTask SetSpriteAsync(SpriteBean bean)
        {
            if (!bean.IsValid())
            {
                return;
            }
            
            if (_bean == bean)
            {
                return;
            }
            
            _asyncVersion++;
            var version = _asyncVersion;
            _bean = bean;
            var handle = YooAssets.LoadAssetAsync(bean.GetAtlasPath());
            await handle.ToUniTask();
            if (!gameObject || version != _asyncVersion)
            {
                handle.Release();
                return;
            }

            _handle?.Release();
            _handle = handle;
            
            var obj = handle.AssetObject;
            if (obj)
            {
                if (obj is AtlasAsset atlasAsset)
                {
                    sprite = atlasAsset.GetSprite(bean.Sprite);
                }
                else if (obj is SpriteAtlas atlas)
                {
                    sprite = atlas.GetSprite(bean.Sprite);
                }
                else
                {
                    Debug.LogError($"{obj.GetType()} is not AtlasAsset or SpriteAtlas");
                }
            }
        }

        public void ClearSprite()
        {
            _asyncVersion++;
            _handle?.Release();
            _handle?.Release();
            _handle = null;
            _bean = default;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearSprite();
        }
    }
}