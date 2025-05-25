using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace LF.Runtime
{
    public class PrefabLoader:MonoBehaviourEx
    {
        [SerializeField]
        private string autoLoadPath;
        private AssetHandle _handle;
        private string _path;
        private int _asyncVersion = 0;
        private GameObject _obj;

        private void Awake()
        {
            if (!string.IsNullOrWhiteSpace(autoLoadPath))
            {
                Load(autoLoadPath);
            }
        }

        public GameObject Load(string path)
        {
            if (_path == path)
            {
                return null;
            }
            
            var info = YooAssets.GetAssetInfo(path);
            if (info.IsInvalid)
            {
                Debug.LogWarning(info.Error);
                return null;
            }
            
            _asyncVersion++;
            _handle?.Release();
            _handle = YooAssets.LoadAssetSync(path);
            if (_handle.IsValid)
            {
                Destroy(_obj);
                _obj = _handle.InstantiateSync(transform.position, transform.rotation, transform);
            }

            _path = path;
            return _obj;
        }
        
        public async UniTask<(bool canceled, GameObject obj)> LoadAsync(string path)
        {
            if (_path == path)
            {
                return (true,null);
            }
            
            var info = YooAssets.GetAssetInfo(path);
            if (info.IsInvalid)
            {
                Debug.LogWarning(info.Error);
                return (true,null);
            }
            
            _asyncVersion++;
            var version = _asyncVersion;
            var handle = YooAssets.LoadAssetAsync(path);
            await handle.ToUniTask();
            if (!gameObject || version != _asyncVersion)
            {
                handle.Release();
                return (true,null);
            }
            
            if (handle.IsValid)
            {
               var op = handle.InstantiateAsync(transform.position, transform.rotation, transform);
               await op.ToUniTask();
               if (!gameObject || version != _asyncVersion)
               {
                   handle.Release();
                   Destroy(op.Result);
                   return (true,null);
               }
               
               Destroy(_obj);
               _obj = op.Result;
            }
            
            _handle?.Release();
            _handle = handle;
            return (false, _obj);
        }

        public void Unload()
        {
            _path = null;
            _asyncVersion++;
            if (_obj)
            {
                Destroy(_obj);
                _obj = null;
            }
            _handle?.Release();
            _handle = null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _handle?.Release();
            _handle = null;
            _path = null;
        }
    }
}