using System;
using System.Collections.Generic;
using System.Linq;
using Config;
using UnityEngine;

namespace LF.Runtime
{
    public partial class PageHolder
    {
        private static readonly Dictionary<string, HashSet<PageHolder>> Holders = new();
        public static PageHolder Allocate(int configId)
        {
            var config = Tables.PageTable.GetOrDefault(configId);
            if (config == null)
            {
                Debug.LogError($"界面配置不存在:{configId}");
                return null;
            }
            
            var type = !string.IsNullOrWhiteSpace(config.HolderType)
                ?  ReflectionUtil.GetTypeByName(config.HolderType) 
                : typeof(PageHolder);
            if (type == null)
            {
                Debug.LogError($"当前程序集中找不到界面 {config} 中配置的 holder 类型:{config.HolderType}");
                return null;
            }

            var holder = Allocate(config.ResPath, type);
            if (holder == null)
            {
                Debug.LogError($"界面配置 {config} 中配置的 Holder 创建失败:{config.HolderType}");
                return null;
            }
            
            holder.LoadConfig(config);
            return holder;
        }
        public static T Allocate<T>(string path) where T : PageHolder
        {
            return Allocate(path, typeof(T)) as T;
        }

        public static T Allocate<T>(int id) where T : PageHolder
        {
            return Allocate(id) as T;
        }

        public static PageHolder Allocate(string path, Type type)
        {
            if(!Holders.TryGetValue(path, out var set))
            {
                set = new();
                Holders.Add(path,set);
            }
            
            var cache = set.FirstOrDefault();
            if (cache?.Multiple == false)
            {
                return cache;
            }
            
            if (Activator.CreateInstance(type) is PageHolder holder)
            {
                 holder._isValid = true;
                 holder.AssetPath = path;
                 set.Add(holder);
                 return holder;
            }
            return null;
        }
    }
}