using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using YooAsset;

namespace LF.Runtime
{
    public class MonoBehaviourEx : MonoBehaviour
    {
        private Dictionary<GameObject, AssetHandle> _objsDict;
        private Transform _transform;
        private GameObject _gameObject;
        
        public new Transform transform
        {
            get
            {
                if (_transform == null && this)
                {
                    _transform = base.transform;
                }
                return _transform;
            }
        }
        
        public new GameObject gameObject
        {
            get
            {
                if (_gameObject == null && this)
                {
                    _gameObject = base.gameObject;
                }
                return _gameObject;
            }
        }
        
        public GameObject Instantiate(string path,Transform parent = null)
        {
            var handle = YooAssets.LoadAssetSync<GameObject>(path);
            var obj = handle.InstantiateSync(parent);
            RecordObject(obj,handle);
            return obj;
        }

        public GameObject[] Instantiates(string path, int count,Transform parent = null)
        {
            var objs = new GameObject[count];
            for (int i = 0; i < count; i++)
            {
                objs[i] = Instantiate(path,parent);
            }

            return objs;
        }

        public async UniTask<GameObject> InstantiateAsync(string path,Transform parent = null)
        {
            var handle = YooAssets.LoadAssetAsync<GameObject>(path);
            var op = handle.InstantiateAsync(parent);
            await op.ToUniTask();
            if (!gameObject)
            {
                DestroyObject(op.Result);
                handle.Release();
                return null;
            }

            if (!op.Result)
            {
                Debug.LogError(op.Error);
                handle.Release();
                return null;
            }

            RecordObject(op.Result,handle);
            return op.Result;
        }
        
        public async UniTask<GameObject[]> InstantiatesAsync(string path, int count,Transform parent = null)
        {
            var tasks = ListPool<UniTask<GameObject>>.Get();
            for (int i = 0; i < count; i++)
            {
                tasks.Add(InstantiateAsync(path,parent));
            }

            var objs = await UniTask.WhenAll(tasks);
            ListPool<UniTask<GameObject>>.Release(tasks);

            return objs;
        }

        protected void DestroyObject(GameObject obj)
        {
            if (_objsDict?.TryGetValue(obj, out var handle) == true)
            {
                handle.Release();
                Destroy(obj);
                _objsDict.Remove(obj);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_objsDict != null)
            {
                foreach (var kv in _objsDict)
                {
                    Destroy(kv.Key);
                    kv.Value.Release();
                }

                _objsDict = null;
            }
        }

        private void RecordObject(GameObject obj,AssetHandle handle)
        {
            if (_objsDict == null)
            {
                _objsDict = new();
            }

            _objsDict.Add(obj,handle);
        }
    }
}