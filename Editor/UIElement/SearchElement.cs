using System;
using System.Collections.Generic;
using System.Linq;
using LF.Runtime;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LF.Editor
{
    public class SearchElement : VisualElement
    {
        public class SearchItemData
        {
            public Texture Texture { get; set; }
            public string Desc { get; set; }
            public object Value { get; set; }
    
            public SearchItemData(object value):this(value,value.ToString()) { }
    
            public SearchItemData(object value, string desc, Texture icon = null)
            {
                Value = value;
                Desc = desc;
                Texture = icon;
            }
    
            public override string ToString()
            {
                return Desc;
            }
        }
        
        private const float MinHeight = 200;
        private VisualElement _bindElement;
        private float _height;
        private Action<SearchItemData> _onSelect;
        private ListView _listView;
        private readonly List<SearchItemData> _originList = new();
        private readonly List<SearchItemData> _searchList = new();
    
        public static void OpenSearch(IList<SearchItemData> list,VisualElement bindElement,Action<SearchItemData> onSelect,bool hasNone = true ,float height = 400)
        {
            if (bindElement == null)
            {
                return;
            }
    
            var element = new SearchElement
            {
                _bindElement = bindElement,
                _height = height,
                _onSelect = onSelect
            };
    
            if (hasNone)
            {
                element._originList.Add(new SearchItemData(null,"None"));
            }
            element._originList.AddRange(list);
            element._searchList.AddRange(element._originList);
            bindElement.panel.visualTree.Children().First(item => item is not IMGUIContainer).Add(element);
        }
    
        public SearchElement()
        {
            focusable = true;
            style.position = new StyleEnum<Position>(Position.Absolute);
            RegisterCallback(new EventCallback<AttachToPanelEvent>(OnAttachToPanel));
            RegisterCallback(new EventCallback<FocusOutEvent>(OnOutFocus));
    
            style.backgroundColor = new StyleColor(new Color(0.219f,0.219f,0.219f));
            style.borderBottomColor = new StyleColor(Color.black);
            style.borderLeftColor = new StyleColor(Color.black);
            style.borderRightColor = new StyleColor(Color.black);
            style.borderTopColor = new StyleColor(Color.black);
            style.borderBottomWidth = 1;
            style.borderTopWidth = 1;
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;
        }
    
        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            var bindWorldBound = _bindElement.worldBound;
            style.width = bindWorldBound.width;
            style.left = bindWorldBound.x;
            
            var root = panel.visualTree.Children().First(item => item is not IMGUIContainer);
            var rootHeight = root.layout.height;
            var panelHeight = panel.visualTree.worldBound.height;
            var titleHeight = panelHeight - rootHeight;
            var topSpace = bindWorldBound.yMin;
            var bottomSpace = panelHeight - bindWorldBound.yMax;
            var height = 0f;
            if (topSpace < MinHeight && bottomSpace < MinHeight)
            {
                // 上下的空间都放不下
                height = Mathf.Min(_height, panelHeight);
                style.top = 0;
            }
            else if (topSpace > bottomSpace)
            {
                height = Mathf.Min(_height, topSpace);
                style.top = topSpace - height - titleHeight;
                style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.ColumnReverse);
            }
            else
            {
                height = Mathf.Min(_height, bottomSpace);
                style.top = bindWorldBound.yMax - titleHeight;
            }
            
            style.height = height;
            AddChild();
        }
    
        private void AddChild()
        {
            var searchField = new ToolbarSearchField();
            searchField.RegisterValueChangedCallback(OnSearchChange);
            searchField.style.width = _bindElement.worldBound.width - 8;
            Add(searchField);
            searchField.Focus();
    
            _listView = new ListView
            {
                showFoldoutHeader = false,
                itemsSource = _searchList,
                style =
                {
                    width = _bindElement.worldBound.width - 12,
                    marginLeft = 8,
                    marginTop = 10
                }
            };
            _listView.selectionChanged += objects =>
            {
                foreach (var obj in objects)
                {
                    _onSelect.SafeInvoke(obj as SearchItemData);
                }
                
                parent?.Remove(this);
            };
            Add(_listView);
        }
    
        private void OnOutFocus(FocusOutEvent evt)
        {
            if (evt.relatedTarget is VisualElement element)
            {
                if (element == this)
                {
                    return;
                }
                while (element.parent != null)
                {
                    if (element.parent == this)
                    {
                        return;
                    }
    
                    element = element.parent;
                }
            }
            parent?.Remove(this);
        }
        
        private void OnSearchChange(ChangeEvent<string> evt)
        {
            _searchList.Clear();
            if (string.IsNullOrWhiteSpace(evt.newValue))
            {
                _searchList.AddRange(_originList);
            }
            else
            {
                foreach (var data in _originList)
                {
                    if (data.Desc.Contains(evt.newValue, StringComparison.OrdinalIgnoreCase))
                    {
                        _searchList.Add(data);
                    }
                }
            }
            _listView.RefreshItems();
        }
    }
}