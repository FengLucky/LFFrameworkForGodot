using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GDLog;
using Godot;
using Godot.Collections;

namespace LF;
public partial class UIPage : Control
{
    [Export]
    protected Array<Button> CloseButton;
    public PageHolder Holder { get; private set; }

    private CanvasGroup _canvasGroup;

    public void Create(PageHolder holder)
    {
        Holder = holder;
        if (CloseButton.IsNotNullOrEmpty())
        {
            foreach (var button in CloseButton)
            {
                if (button.IsValid())
                {
                    button.Pressed += OnClickClose;
                }
            }
        }

        try
        {
            OnCreate();
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }
    }

    public async UniTask Preload(CancellationToken cancellationToken = default)
    {
        try
        {
            await OnPreload(cancellationToken).SuppressCancellationThrow();
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }
    }

    public void Open()
    {
        try
        {
            OnOpen();
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }

        RefreshArgs();
    }

    public void RefreshArgs()
    {
        try
        {
            OnArgsRefresh();
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }
    }
    
    public async UniTask CloseAnimation()
    {
        try
        {
            await OnCloseAnimation().SuppressCancellationThrow();
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }
    }

    public void Close()
    {
        Holder.Close();
    }

    public UniTask CloseAsync()
    {
        return Holder.CloseAsync();
    }
    
    public void CloseHandle()
    {
        try
        {
            OnClose();
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }
    }

    public void SetCloseButtonDisabled(bool disabled)
    {
        foreach (var button in CloseButton)
        {
            if (button.IsValid())
            {
                button.Disabled = disabled;
            }
        }
    }

    #region 生命周期
    /// <summary>
    /// 资源创建完毕
    /// </summary>
    protected virtual void OnCreate(){}
    /// <summary>
    /// 加载完毕后预加载资源
    /// </summary>
    /// <returns></returns>
    protected virtual UniTask OnPreload(CancellationToken cancellationToken = default) => UniTask.CompletedTask;
    /// <summary>
    /// 界面进入可视状态
    /// </summary>
    protected virtual void OnOpen() {}
    /// <summary>
    /// 界面打开后和参数刷新时调用一次
    /// </summary>
    protected virtual void OnArgsRefresh(){}
    /// <summary>
    /// 进入可视状态时播放动画
    /// </summary>
    /// <returns></returns>
    protected virtual UniTask OnShowAnimation(CancellationToken cancellationToken = default) => UniTask.CompletedTask;
    /// <summary>
    /// 关闭之前播放动画
    /// </summary>
    /// <returns></returns>
    protected virtual UniTask OnCloseAnimation()=> UniTask.CompletedTask;
    /// <summary>
    /// 关闭界面后销毁前回调
    /// </summary>
    protected virtual void OnClose(){}
    /// <summary>
    /// 点击关闭按钮
    /// </summary>
    protected virtual void OnClickClose() => Close();
    
    #endregion
}