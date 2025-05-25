using System;

namespace LF.Runtime
{
    public abstract class StaticEventSystem<T> where T : Enum
    {
        private static readonly EventSystem<T> EventSystem = new();

        public static void Send(T eventId, IEventArgs args = null)
        {
            EventSystem.Send(eventId, args);
        }
        
        public static void DeferSend<TArgs>(T eventId, TArgs args = default,bool onceEvent = false) where TArgs : IEventArgs, IEquatable<TArgs>
        {
            EventSystem.DeferSend(eventId, args,onceEvent);
        }

        public static void Register(T eventId, Action<IEventArgs> onReceive)
        {
            EventSystem.Register(eventId, onReceive);
        }
        
        public static void DeferRegister(T eventId, Action<IEventArgs> onReceive)
        {
            EventSystem.DeferRegister(eventId, onReceive);
        }

        public static void UnRegister(T eventId, Action<IEventArgs> onReceive)
        {
            EventSystem.UnRegister(eventId, onReceive);
        }
        
        public static void DeferUnRegister(T eventId, Action<IEventArgs> onReceive)
        {
            EventSystem.DeferUnRegister(eventId, onReceive);
        }
    }
}