// using System;
// using System.Collections.Generic;
//
// namespace LF;
// public class UIList<TComponent, TData> where TComponent : UIMonoBehaviour, IUIListItem<TData>
// {
//     private readonly List<TComponent> _pool = new();
//     private TComponent _template;
//     private RectTransform _parent;
//     public List<TComponent> ActiveList { get; } = new();
//     public event Action<TComponent> OnNewItem;
//
//     public void Init(TComponent template, RectTransform parent = null)
//     {
//         if (!template)
//         {
//             Debug.LogError("template is null");
//             return;
//         }
//
//         _template = template;
//         if (!parent)
//         {
//             parent = template.transform.parent as RectTransform;
//         }
//
//         _parent = parent;
//         template.SafeSetActive(false);
//     }
//
//     public void Refresh(IList<TData> dataList)
//     {
//         for (int i = _pool.Count; i < dataList.Count; i++)
//         {
//             var item = Object.Instantiate(_template, _parent);
//             OnNewItem.SafeInvoke(item);
//             _pool.Add(item);
//         }
//
//         ActiveList.Clear();
//         for (int i = 0; i < dataList.Count; i++)
//         {
//             _pool[i].SafeSetActive(true);
//             _pool[i].Refresh(dataList[i]);
//             ActiveList.Add(_pool[i]);
//         }
//
//         for (int i = dataList.Count; i < _pool.Count; i++)
//         {
//             _pool[i].SafeSetActive(false);
//         }
//     }
//
//     public TComponent Get(int index)
//     {
//         return ActiveList.TryGet(index);
//     }
//         
//     public void Clear()
//     {
//         foreach (var component in _pool)
//         {
//             component.SafeSetActive(false);
//         }
//     }
// }