using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LF
{
    [RequireComponent(typeof(Canvas))]
    public class UIWindow : UIMonoBehaviour
    {
        public Canvas Canvas { get; private set; }
        private RectTransform _handlerTemplate;
        public List<PageHolder> Holders { get; private set; } = new();

        protected virtual void Awake()
        {
            Canvas = GetComponent<Canvas>();
            var template = new GameObject("HandlerTemplate");
            template.transform.SetParent(transform, false);
            _handlerTemplate = template.AddComponent<RectTransform>();
            _handlerTemplate.anchorMin = Vector2.zero;
            _handlerTemplate.anchorMax = Vector2.one;
            _handlerTemplate.sizeDelta = Vector2.zero;
        }

        public void Open(PageHolder holder)
        {
            var index = Holders.IndexOf(holder);
            if (index >= 0)
            {
                if (!holder.ReCreate())
                {
                    Close(holder);
                    return;
                }

                RemoveHolder(holder);
            }
            else
            {
                var root = Instantiate(_handlerTemplate, RectTransform);
                root.name = holder.GetType().Name;
                if (!holder.Create(this, root))
                {
                    Destroy(root);
                    return;
                }
            }

            holder.Root.SetAsLastSibling();
            Holders.Add(holder);
            RefreshHolderLayer();
            WaitPageShow(holder).Forget();
        }

        public PageHolder Open(int id,object args = null)
        {
            var holder = PageHolder.Allocate(id);
            if (holder == null)
            {
                return null;
            }

            holder.SetArgs(args);
            Open(holder);
            return holder;
        }

        public void Close(PageHolder holder)
        {
            CloseAsync(holder).Forget();
        }

        public async UniTask CloseAsync(PageHolder holder)
        {
            if (holder.State == PageHolder.HolderState.Closed)
            {
                return;
            }

            if (holder.State == PageHolder.HolderState.Closing)
            {
                while (holder.State != PageHolder.HolderState.Closed)
                {
                    await UniTask.NextFrame();
                }
                return;
            }
            RemoveHolder(holder);
            await holder.CloseHandle();
            Destroy(holder.Root.gameObject);
            RefreshHolderCover();
            RefreshHolderLayer();
        }

        private async UniTaskVoid WaitPageShow(PageHolder holder)
        {
            await holder.WaitShow();
            RefreshHolderCover();
        }

        private void RemoveHolder(PageHolder holder)
        {
            if (holder.CoverScreen)
            {
                CloseCoverPage(holder);
            }
            else
            {
                Holders.Remove(holder);
            }
        }

        private void CloseCoverPage(PageHolder holder)
        {
            if (!holder.CoverScreen)
            {
                return;
            }

            var index = Holders.IndexOf(holder);
            if (index == -1)
            {
                return;
            }

            var realIndex = index;
            var hasCover = false;
            // 全屏界面把它上边的非全屏界面全部干掉
            for (int i = index + 1; i < Holders.Count; i++)
            {
                var h = Holders[i];
                if (h.CoverScreen)
                {
                    hasCover = true;
                }

                if (!hasCover)
                {
                    Close(h);
                }
                else
                {
                    Holders[realIndex] = h;
                    realIndex++;
                }
            }

            Holders.RemoveRange(realIndex, Holders.Count - realIndex);
        }

        private void RefreshHolderCover()
        {
            var hasCover = false;
            for (int i = Holders.Count - 1; i >= 0; i--)
            {
                var holder = Holders[i];
                if (!hasCover)
                {
                    if (holder.BeCovered)
                    {
                        holder.OnReveal();
                    }

                    if (holder.CoverScreen)
                    {
                        hasCover = true;
                    }
                }
                else
                {
                    if (!holder.BeCovered)
                    {
                        holder.OnCovered();
                    }
                }
            }
        }

        private void RefreshHolderLayer()
        {
            for (int i = 0; i < Holders.Count; i++)
            {
                if (i == 0)
                {
                    Holders[0].Root.anchoredPosition3D = Vector3.zero;
                }
                else
                {
                    var lastHolder = Holders[i - 1];
                    Holders[0].Root.anchoredPosition3D = new Vector3(0, 0,
                        lastHolder.Root.anchoredPosition3D.z + lastHolder.ZThickness);
                }
            }
        }
    }
}