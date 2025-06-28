using System;
using System.Collections.Generic;
using GDLog;
using Godot;
namespace LF;

public class GlobalProcessManager:Manager<GlobalProcessManager>
{
    private readonly struct ProcessItem<T>(T process, int priority):IEquatable<ProcessItem<T>>
    {
        public readonly T Process = process;
        public readonly int Priority = priority;

        public bool Equals(ProcessItem<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Process, other.Process);
        }

        public override bool Equals(object obj)
        {
            return obj is ProcessItem<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Process.GetHashCode();
        }
    }
    private readonly List<ProcessItem<IProcess>> _processList = new(16);
    private readonly List<ProcessItem<IPhysicsProcess>> _physicsProcessList = new(16);
    protected override void OnInit()
    {
        base.OnInit();
        var scriptRes = ResourceLoader.Load<CSharpScript>("res://addons/LFFramework/Runtime/GlobalProcess/ProcessRunner.cs");
        var script = scriptRes.New();
        if (script.Obj is ProcessRunner runner)
        {
            runner.Name = "GlobalProcessRunner";
            runner.OnProcess += OnProcess;
            runner.OnPhysicsProcess += OnPhysicsProcess;
            (Engine.Singleton.GetMainLoop() as SceneTree)?.Root.CallDeferred("add_child",script);
        }
    }
    
    public void AddProcess(IProcess process, int priority = 100)
    {
        var item = new ProcessItem<IProcess>(process, priority);
        if (_processList.Contains(item))
        {
            GLog.DebugWarn("重复添加 IProcess");
            return;
        }
        var index = FindInsertIndex(_processList,ref item);
        _processList.Insert(index, item);
    }
    
    public void RemoveProcess(IProcess process)
    {
        _processList.Remove(new ProcessItem<IProcess>(process, 0));
    }
    
    public void AddPhysicsProcess(IPhysicsProcess process, int priority = 100)
    {
        var item = new ProcessItem<IPhysicsProcess>(process, priority);
        if (_physicsProcessList.Contains(item))
        {
            GLog.DebugWarn("重复添加 IPhysicsProcess");
            return;
        }
        var index = FindInsertIndex(_physicsProcessList,ref item);
        _physicsProcessList.Insert(index, item);
    }
    
    public void RemovePhysicsProcess(IPhysicsProcess process)
    {
        _physicsProcessList.Remove(new ProcessItem<IPhysicsProcess>(process, 0));
    }
    
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
    
    private void OnPhysicsProcess(double delta)
    {
        using var list = ListPool<ProcessItem<IPhysicsProcess>>.GetItem();
        list.Value.AddRange(_physicsProcessList);
        for (int i = 0; i < list.Value.Count; i++)
        {
            try
            {
                list.Value[i].Process.OnPhysicsProcess(delta);
            }
            catch (Exception e)
            {
                GLog.Exception(e);
            }
        }
    }
    
    private static int FindInsertIndex<T>(List<ProcessItem<T>> list,ref ProcessItem<T> item)
    {
        int low = 0;
        int high = list.Count;

        while (low < high)
        {
            int mid = low + (high - low) / 2;
            if (item.Priority <= list[mid].Priority)
                low = mid + 1;
            else
                high = mid;
        }

        return high;
    }
}