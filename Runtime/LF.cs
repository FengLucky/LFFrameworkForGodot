using Config;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace LF.Runtime
{
    public class LFInitializationParam
    {
        public string YooAssetPackageName { get; set; } = "Res";
    }
    public static class LF
    {
        public static async UniTask Initialization(LFInitializationParam param = null)
        {
            param ??= new LFInitializationParam();
            await InitPackage(param.YooAssetPackageName);
            Tables.LoadTables();
            Localization.Init();
        }
        
        private static async UniTask InitPackage(string packageName)
        {
            // 初始化资源系统
            YooAssets.Initialize();

            // 创建默认的资源包
            var package = YooAssets.CreatePackage(packageName);
            // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
            YooAssets.SetDefaultPackage(package);
            
#if UNITY_EDITOR
            var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
            var packageRoot = buildResult.PackageRootDirectory;
            var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            var initParameters = new EditorSimulateModeParameters();
            initParameters.EditorFileSystemParameters = editorFileSystemParams;
            var initOperation = package.InitializeAsync(initParameters);
            await initOperation.ToUniTask();
#else
            var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            var initParameters = new OfflinePlayModeParameters();
            initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
            var initOperation = package.InitializeAsync(initParameters);
            await initOperation.ToUniTask();
#endif
            var version = package.RequestPackageVersionAsync();
            await version.ToUniTask();
            if (version.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"资源包初始化失败：{version.Error}");
            }
            var update = package.UpdatePackageManifestAsync(version.PackageVersion);
            await update.ToUniTask();
            if (update.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"资源包更新失败：{update.Error}");
            }
            if (initOperation.Status == EOperationStatus.Succeed)
            {
                Debug.Log("资源包初始化成功！");
            }
            else
            {
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
            }
        }
    }
}