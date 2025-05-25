using System;
using System.Threading;
using Config;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace LF.Runtime
{
    [ClassName("通用界面")]
    public partial class PageHolder
    {
        public enum HolderState
        {
            New,
            Created,
            Closing,
            Closed,
        }

        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath { get; protected set; }

        /// <summary>
        /// 该界面能遮盖整个屏幕，
        /// </summary>
        public virtual bool CoverScreen { get; protected set; }

        /// <summary>
        /// 界面 z 轴厚度
        /// </summary>
        public virtual int ZThickness { get; protected set; }

        /// <summary>
        /// 界面是否常驻内存(被覆盖时不销毁资源)
        /// </summary>
        public virtual bool AlwaysInMemory { get; protected set; }

        /// <summary>
        /// 该界面可以同时存在多个实例
        /// </summary>
        public virtual bool Multiple { get; protected set; }

        /// <summary>
        /// 管理该界面的 Window
        /// </summary>
        public UIWindow Window { get; private set; }

        /// <summary>
        /// 该 Holder 的根节点
        /// </summary>
        public RectTransform Root { get; private set; }

        /// <summary>
        /// 被覆盖
        /// </summary>
        public bool BeCovered { get; private set; }
        
        public object Args { get; private set; }

        protected bool PageOpened { get; set; }
        public HolderState State { get; private set; } = HolderState.New;

        private UIPage _page;
        private AssetHandle _assetHandle;
        private bool _isValid;
        private CancellationTokenSource _cancellation = new();

        public virtual void SetArgs(object args)
        {
            Args = args;
        }

        public T GetArgs<T>()
        {
            try
            {
                return (T)Args;
            }
            catch (Exception)
            {
                return default;
            }
        }
        
        public async UniTask<T> GetPage<T>() where T : UIPage
        {
            if (State == HolderState.New)
            {
                Debug.LogError($"{AssetPath} 未打开，无法获取界面");
                return null;
            }
            if (PageOpened)
            {
                return _page as T;
            }

            if (State >= HolderState.Closing)
            {
                return null;
            }

            await UniTask.WaitWhile(() => PageOpened, cancellationToken: _cancellation.Token)
                .SuppressCancellationThrow();
            return _page as T;
        }

        public async UniTask WaitShow()
        {
            if (State == HolderState.New)
            {
                Debug.LogError($"{AssetPath} 未打开，无法等待显示");
                return;
            }
            if (PageOpened || State >= HolderState.Closing)
            {
                return;
            }

            await UniTask.WaitWhile(() => PageOpened, cancellationToken: _cancellation.Token)
                .SuppressCancellationThrow();
        }

        public async UniTask WaitClose()
        {
            if (State == HolderState.New)
            {
                Debug.LogError($"{AssetPath} 未打开，无法等待关闭");
                return;
            }
            if (State >= HolderState.Closing)
            {
                return;
            }

            await UniTask.WaitUntil(() => State >= HolderState.Closing, cancellationToken: _cancellation.Token)
                .SuppressCancellationThrow();
        }

        /// <summary>
        /// 在 window 中创建
        /// </summary>
        /// <param name="window"></param>
        /// <param name="root"></param>
        public bool Create(UIWindow window, RectTransform root)
        {
            if (!_isValid)
            {
                Debug.LogWarning($"{GetType().Name} 必须使用 {nameof(PageHolder)}.{nameof(Allocate)} 分配，不可以自己 new()");
                return false;
            }

            if (!CanOpen())
            {
                return false;
            }

            Window = window;
            Root = root;
            BeCovered = false;

            LoadPage().Forget();
            State = HolderState.Created;
            return true;
        }

        public bool ReCreate()
        {
            return Create(Window, Root);
        }

        public void Close()
        {
            CloseAsync().Forget();
        }

        public UniTask CloseAsync()
        {
            return Window.CloseAsync(this);
        }

        public async UniTask CloseHandle()
        {
            State = HolderState.Closing;
            if (Holders.TryGetValue(AssetPath, out var set))
            {
                set.Remove(this);
            }
            _cancellation.Cancel();
            if (_page)
            {
                if (!BeCovered)
                {
                    await _page.CloseAnimation();
                }

                _page.CloseHandle();
                _assetHandle?.Release();
                _assetHandle = null;
                _page = null;
            }

            OnClose();
            PageOpened = false;
            State = HolderState.Closed;
        }

        #region 生命周期

        /// <summary>
        /// 被遮盖
        /// </summary>
        public virtual void OnCovered()
        {
            BeCovered = true;
            if (!AlwaysInMemory)
            {
                DestroyPage();
                _cancellation = new();
            }
        }

        /// <summary>
        /// 恢复遮盖
        /// </summary>
        public virtual void OnReveal()
        {
            BeCovered = false;
            if (!AlwaysInMemory)
            {
                LoadPage().Forget();
            }
        }

        public virtual void OnClose()
        {
        }

        #endregion

        /// <summary>
        /// 界面是否可以打开
        /// </summary>
        /// <returns></returns>
        protected virtual bool CanOpen()
        {
            return true;
        }

        private void LoadConfig(PageBean bean)
        {
            CoverScreen = bean.CoverScreen;
            ZThickness = bean.ZThickness;
            AlwaysInMemory = bean.AlwaysInMemory;
            Multiple = bean.Multiple;
        }

        private async UniTaskVoid LoadPage()
        {
            var cancellation = _cancellation;
            if (!PageOpened)
            {
                _assetHandle = YooAssets.LoadAssetAsync<GameObject>(AssetPath);
                var op = _assetHandle.InstantiateAsync(Root);
                var canceled = await op.ToUniTask(cancellationToken: _cancellation.Token).SuppressCancellationThrow();
                if (canceled)
                {
                    op.Cancel();
                    _assetHandle.Release();
                    _assetHandle = null;
                    await CloseAsync();
                    return;
                }
                
                if (!op.Result)
                {
                    Debug.LogError(op.Error);
                    _assetHandle.Release();
                    _assetHandle = null;
                    await CloseAsync();
                    return;
                }

                var page = op.Result.GetComponent<UIPage>();
                if (!page)
                {
                    Debug.LogError($"资源:{AssetPath} 没挂 {nameof(UIPage)} 组件");
                    Object.Destroy(op.Result);
                    _assetHandle?.Release();
                    _assetHandle = null;
                    await CloseAsync();
                    return;
                }

                _page = page;
            }

            var pageTransform = _page.transform as RectTransform;
            if (!pageTransform)
            {
                Debug.LogError($"{_page} is not has RectTransform");
                Object.Destroy(_page.gameObject);
                _assetHandle?.Release();
                _assetHandle = null;
                _page = null;
                await CloseAsync();
                return;
            }

            if (!PageOpened)
            {
                pageTransform.anchoredPosition = new Vector3(9999, 9999);
                await _page.Open(this);
                if (cancellation.IsCancellationRequested)
                {
                    return;
                }
            }

            pageTransform.anchoredPosition = Vector3.zero;
            pageTransform.localScale = Vector3.one;
            await _page.Show(cancellation.Token);
            if (cancellation.IsCancellationRequested)
            {
                return;
            }

            PageOpened = true;
        }

        private void DestroyPage()
        {
            _cancellation.Cancel();
            if (_page)
            {
                _page.CloseHandle();
                _assetHandle?.Release();
                _assetHandle = null;
                Object.Destroy(_page.gameObject);
                _page = null;
            }

            PageOpened = false;
        }
    }
}