using System.Threading;
using Cysharp.Threading.Tasks;
using Godot;

namespace LF;
public static class ResourceLoaderExpansion
{
    public static async UniTask<T> LoadAsync<T>(this ResourceLoaderInstance loader,string path,
        string typeHint = "", bool useSubThreads = false, ResourceLoader.CacheMode cacheMode = ResourceLoader.CacheMode.Reuse,
        CancellationToken cancellationToken = default) where T:Resource
    {
        var error = loader.LoadThreadedRequest(path,typeHint,useSubThreads,cacheMode);
        if (error != Error.Ok)
        {
            return null;
        }

        ResourceLoader.ThreadLoadStatus status;
        do
        {
            await UniTask.Yield(cancellationToken);
            status = loader.LoadThreadedGetStatus(path);
        } while (status == ResourceLoader.ThreadLoadStatus.InProgress);
        
        if (status != ResourceLoader.ThreadLoadStatus.Loaded)
        {
            return null;
        }

        return loader.LoadThreadedGet(path) as T;
    }
}