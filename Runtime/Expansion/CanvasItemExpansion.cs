using System.Runtime.CompilerServices;
using Godot;

namespace LF;

public static class CanvasItemExpansion
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Show(this CanvasItem item)
    {
        SafeSetVisible(item, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hide(this CanvasItem item)
    {
        SafeSetVisible(item, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvertVisible(this CanvasItem item)
    {
        if (item?.IsValid() == true)
        {
            item.Visible = !item.Visible;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SafeSetVisible(this CanvasItem item, bool active)
    {
        if (item?.IsValid() == true)
        {
            item.Visible = active;
        }
    }
}