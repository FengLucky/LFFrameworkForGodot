
# 关于

专为 `Godot CSharp` 设计的游戏开发框架 <br>
开发中。。。谨慎使用

# 安装
1. 将 `LFFramework` 文件夹放入 `Godot` 项目的 `addons` 文件夹中
2. 项目->项目设置->插件->启用 `LFFramework`
3. 项目->工具->LFFramework->安装依赖
   
	[GodotSharpLog](https://github.com/FengLucky/GodotSharpLog) : `Godot CSharp` 日志插件

	[UniTaskForGodot](https://github.com/FengLucky/UniTaskForGodot) : `UniTask` 的 `Godot` 适配

	[LubanExtend](https://github.com/FengLucky/LubanExtend) : `Luban` 配置工具的扩展应用

4. 项目->工具->打表

# 使用说明

## 框架初始化
```csharp
await LFFramework.Initialization();
GD.Print("框架初始化完成");
```

## 配置管理
框架内配置管理主要使用 `Luban` 进行，详细使用方法可参考 [Luban 使用文档](https://luban.doc.code-philosophy.com/docs/intro) 和 [LubanExtend](https://github.com/FengLucky/LubanExtend)

安装`Luban`依赖后会自动在项目根目录下创建 `Config` 文件夹用于存放配置文件

推荐在开发时使用 `打表-json` 进行配置生成，发布时使用 `打表-bin` 进行配置生成

代码中获取配置通过 `Tables.表名` 访问,例如:
```csharp
var bean = Tables.PageTable.GetOrDefault(1);
```

## 事件系统
`EventSystem<T>` 事件系统组件
- `Register` : 注册事件
- `UnRegister` : 取消注册事件
- `Send` : 发送事件
- `DeferSend` : 延迟一帧发送事件
- `DeferUnRegister` : 延迟一帧取消注册事件

`AsyncEventSystem<T>` 异步实现的 `EventSystem<T>`

`StaticEventSystem<T>` 静态实现的 `EventSystem<T>`
```csharp
public enmu UIEvent
{
	PageOpen,
}

public class UIEventSystem : StaticEventSystem<UIEvent>;

UIEventSystem.Register(UIEvent.PageOpen, (page) =>{GD.Print("界面打开")})

UIEventSystem.Send(UIEvent.PageOpen, page);
```
## 状态机
`FSMStateBase<TState,TEnum>` 状态基类
- `OnEnter` : 状态进入时调用
- `OnUpdate` : 状态更新时调用
- `OnExit` : 状态退出时调用
- `CanSwitchTo` : 切换状态前调用，用来判断是否可以切换至目标状态

`FSMControl<TState,TEnum>` 状态机控制基类
- `RegisterState` : 注册状态
- `SwitchState` : 切换到指定状态，注意：状态是在下一帧切换，调用完该方法之后内部不会立刻切换状态
- `Update` : 更新状态机，如果状态需要每帧更新，需要业务层主动调用 `FSMControl.Update`

```csharp
public enum PlayerState
{
	Idle,
	Run
}

public abstract class PlayerStateBase : FSMStateBase<PlayerStateBase, PlayerState>
{
	protected PlayerStateBase(FSMControl<PlayerStateBase, PlayerState> control, PlayerState type) : base(control, type)
	{
	}
}

public class IdelState : PlayerStateBase
{
	public IdelState(FSMControl<PlayerStateBase, PlayerState> control) : base(control, PlayerState.Idle)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Control.SwitchState(PlayerState.Run);
	}
}

public class RunState : PlayerStateBase
{
	public RunState(FSMControl<PlayerStateBase, PlayerState> control) : base(control, PlayerState.Run)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		GD.Print("当前状态为 Run");
	}
}

public class PlayerStateControl : FSMControl<PlayerStateBase, PlayerState>
{
	public void Init()
	{
		RegisterState(new IdelState(this),true); // 默认状态
		RegisterState(new RunState(this));
	}
}
```
## 单例
`Manager<T>` 单例基类
```csharp
public class GameManager : Manager<GameManager>
{
	public bool GameStarted { get; private set; }
}

GD.Print("游戏状态：" + GameManager.Instance.GameStarted);
```
## 全局 Process
godot 中如果脚本重写了 _Process 或者 _PhysicsProcess 方法，那么每个脚本每帧都会产生一次跨语言调用，对性能并不友好。

如果脚本中的 _Process 对调用时机并不敏感，建议使用 `GlobalProcess` 来替代, 多个脚本使用 `GlobalProcess` 每帧也只会产生一次跨语言调用

`Manager` 单例也可以通过实现接口的方式使用 `GlobalProcess` 每帧运行更新逻辑

使用 `GlobalProcess` 需脚本实现以下两个接口中的一个或两个
- `IProcess` : 等同于 `_Process`
- `IPhysicsProcess` : 等同于 `_PhysicsProcess`

使用 `GlobalProcess` 需要手动开启和关闭更新
```csharp
public partial class ProcessSample : Node,IProcess,IPhysicsProcess
{
	public override void _EnterTree()
	{
		base._EnterTree();
		this.EnableProcess();
		this.EnablePhysicsProcess();
	}

	public void OnProcess(double delta)
	{
		GD.Print("Process");
	}

	public void OnPhysicsProcess(double delta)
	{
		GD.Print("PhysicsProcess");
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		this.DisableProcess();
		this.DisablePhysicsProcess();
	}
}
```
## 对象池
- `BasePool<T>` : 通用对象池，需传入创建方法、回收方法
- `Pool<T>` : 继承自`BasePool`，只能使用实现 `IPool` 接口的对象
- `BaseNodePool<T>` : 专为继承`Node`的脚本使用的对象池
- `NodePool<T>` : 继承自`BaseNodePool`，只能使用实现 `INodePool` 接口的对象
- `BaseNodePoolAsync<T>` : `BaseNodePool<T>` 的异步实现
- `NodePoolAsync<T>` : `NodePool<T>` 的异步实现
- `StaticPool<T>` : `Pool<T>` 的静态实现
- `StringBuilderPool` : `StringBuilder` 的静态对象池
- `CollectionPool<TCollection, TItem>` 集合类型的静态对象池
- `ListPool<T>` : `List<T>` 的静态对象池
- `DictionaryPool<TKey, TValue>` : `Dictionary<TKey, TValue>` 的静态对象池
- `HashSetPool<T>` : `HashSet<T>` 的静态对象池

以下是 `GlobalProcess` 内部实现中使用对象池的示例:
```csharp
private void OnProcess(double delta)
{
  using var list = ListPool<ProcessItem<IProcess>>.GetItem();
  list.Value.AddRange(_processList);
  for (int i = 0; i < list.Value.Count; i++)
  {
	  try
	  {
		  list.Value[i].Process.OnProcess(delta);
	  }
	  catch (Exception e)
	  {
		  GLog.Exception(e);
	  }
  }
}
```

## Settings
对 `ConfigFile` 的一个静态实例包装

## 存档
使用 `C#` 内置 `json` 实现的数据持久化功能
- `ArchiveBase<T,ST>` 存档的基类, `T` 为存档数据类型, `ST` 为简单存档数据类型
- `SimpleArchiveBase` 简单存单的基类,主要用于存档列表的显示
```csharp
public class Archive:ArchiveBase<Archive,SimpleArchive>
{
    [JsonInclude]
    public DateTime ArchiveTime { get; private set; }

    protected override void OnSave()
    {
        base.OnSave();
        ArchiveTime = DateTime.Now;
    }

    protected override void FillSimpleData(SimpleArchive data)
    {
        data.FillData(this);
    }
}

public class SimpleArchive:SimpleArchiveBase
{
    [JsonInclude]
    public DateTime ArchiveTime { get; private set; }

    public void FillData(Archive archive)
    {
        ArchiveTime = archive.ArchiveTime;
    }
}

if (Archive.HasArchive(0))
{
    // 从 0 号存档位加载存档
	Archive.LoadArchive(0);
	GLog.Info("存档时间:"+Archive.Instance.ArchiveTime);
}
else
{
    // 创建一个新的存档
	Archive.CreateArchive();
}

// 将当前存档保存到指定存档位
// 注意：Archive.Instance 在加载或创建存档后才不为 null
// 保存存档时会将 Archive.Instance 整个序列化为 json
Archive.Instance.Save(0);
```

## UI 界面管理
`PageManager` : 界面管理器单例
- `Open` : 打开一个界面
- `SetTheme` : 设置界面的根主题

`PageHolder` : 一个界面的句柄，使用 `PageManager` 打开界面时会先生成一个 `PageHolder` 立即返回，然后异步加载界面资源
- `GetPage<T>` : 获取 `UIPage` 界面实例
- `SetArgs` : 设置界面参数,`UIPage` 如果已经打开会立即调用 `UIPage.RefreshArgs`,如果没有打开会保存到打开之后调用
- `WaitOpen` : 等待界面打开
- `WaitClose` : 等待界面关闭
- `Close` : 关闭界面

`UIPage` : 界面基类
- `Close` : 关闭界面
- `OnCreate` : 资源创建完毕后回调
- `OnPreload` : 资源创建完毕后回调，可以在这里预加载资源
- `OnOpen` : 预加载结束后回调，这时界面真正可见
- `OnShowAnimation` : `OnOpen` 调用后回调，用于播放界面打开动画
- `OnArgsRefresh` : `OnOpen` 调用后回调, 通过 `PageHolder.SetArgs` 设置参数后回调
- `OnCloseAnimation` : 关闭界面之前回调，用于播放界面关闭动画
- `OnClose` : `OnCloseAnimation` 结束后回调
- `OnClickClose` : 点击关闭按钮回调,默认实现为关闭界面

界面资源的根节点应挂载继承 `UIPage` 的脚本

`UIPage` 定义了几个通用变量方便使用,直接在 `Inspector` 中拖入即可
- `CloseButton` : 关闭界面的按钮，
- `AnimationPlayer` : 界面打开和关闭的动画播放器
- `ShowAnimationName` : 界面打开的动画名称
- `CloseAnimationName` : 界面关闭的动画名称

`PageManager` 打开的界面必须在 `Config/Excel/#界面配置.xlsx` 中配置

`PageTableConst` 配置生成的界面 `id` 常量
```csharp
PageManager.Instance.Open(PageTableConst.MainPage); // 打开主界面
```

## Loading
`LoadingPage` `Loading` 界面的基类
- `ProgressBar` 进度条，直接在 `Inspector` 中拖入即可
- `SetProgress` 如果 `Loading` 界面有进度条,使用这个方法设置进度
- `OnProgressChanged` 设置进度后的回调

`Loading` 静态工具类
- `Show` 显示指定的 `Loading` 界面,默认提供了一个不带进度条的简单黑屏过度
```csharp
var loadingPage = await Loading.Show();
// do something
loadingPage.Close();
```
## Toast
`Toast` 静态工具类
- `Show` 显示一个提示信息
```csharp
Toast.Show("提示信息");
```
