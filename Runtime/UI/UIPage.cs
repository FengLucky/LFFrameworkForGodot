using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace LF.Runtime
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPage : UIMonoBehaviour
    {
        [Label("关闭按钮")]
        [SerializeField]
        protected UIButton[] closeButton;
        public PageHolder RawHolder { get; private set; }

        private CanvasGroup _canvasGroup;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (closeButton.IsNotNullOrEmpty())
            {
                foreach (var button in closeButton)
                {
                    button.OnActiveClick = OnClickClose;
                }
            }
        }

        public async UniTask Open(PageHolder holder)
        {
            RawHolder = holder;
            try
            {
                await OnOpen();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async UniTask Show(CancellationToken cancellationToken = default)
        {
            try
            {
                var cancel = await OnShowAnimation(cancellationToken).SuppressCancellationThrow();
                if (!cancel)
                {
                    OnShow();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Close()
        {
            RawHolder.Close();
        }

        public UniTask CloseAsync()
        {
            return RawHolder.CloseAsync();
        }

        public async UniTask CloseAnimation()
        {
            try
            {
                await OnCloseAnimation().SuppressCancellationThrow();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void CloseHandle()
        {
            try
            {
                OnClose();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void SetCloseButtonInteractable(bool interactable)
        {
            foreach (var button in closeButton)
            {
                if (button)
                {
                    button.interactable = interactable;
                }
            }
        }

        #region 生命周期

        /// <summary>
        /// 界面加载完毕时回调，用来处理资源加载等操作，该步骤结束后才能看到界面
        /// </summary>
        /// <returns></returns>
        protected virtual UniTask OnOpen()
        {
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 界面展示时播放的动画
        /// </summary>
        /// <returns></returns>
        protected virtual UniTask OnShowAnimation(CancellationToken cancellationToken = default)
        {
            _canvasGroup.alpha = 0.2f;
            RectTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            RectTransform.DOScale(1, 0.15f);
            var token = destroyCancellationToken;
            if (cancellationToken != CancellationToken.None)
            {
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,token);
                token = source.Token;
            }
            return _canvasGroup.DOFade(1, 0.15f).ToUniTask(cancellationToken: token);
        }

        /// <summary>
        /// 界面真正开始显示时的回调，执行该回调之前最好不要执行业务逻辑
        /// </summary>
        protected virtual void OnShow()
        {
        }

        /// <summary>
        /// 界面关闭之前的动画
        /// </summary>
        /// <returns></returns>
        protected virtual UniTask OnCloseAnimation()
        {
            RectTransform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0.15f);
            return _canvasGroup.DOFade(0.2f, 0.15f).ToUniTask(cancellationToken: destroyCancellationToken);
        }

        /// <summary>
        /// 界面关闭
        /// </summary>
        protected virtual void OnClose()
        {
        }

        /// <summary>
        /// 点击界面关闭按钮
        /// </summary>
        protected virtual void OnClickClose()
        {
            Close();
        }

        #endregion
    }
}