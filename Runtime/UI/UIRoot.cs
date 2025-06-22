using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace LF;
public class UIRoot:MonoBehaviourEx
{
    public int UILayer { get; private set; }
    public static UIRoot Instance { get; private set; }
    public Camera UICamera { get; private set; }
        
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        UICamera = GetComponentInChildren<Camera>(true);
        UILayer = LayerMask.GetMask("UI");
        if (!UICamera)
        {
            Debug.LogError("UIRoot 没有找到UI相机");
        }
    }

    private PointerEventData _eventData;
    public bool IsInUIRange(Vector2 screenPos)
    {
        _eventData ??= new PointerEventData(EventSystem<>.current);
        _eventData.position = screenPos;
        var results = ListPool<RaycastResult>.Get();
        EventSystem<>.current.RaycastAll(_eventData,results);
        var hit = results.Count > 0;
        ListPool<RaycastResult>.Release(results);
        return hit;
    }

    public bool IsInUIRange(RectTransform rect, Vector2 screenPos)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rect,screenPos,UICamera);
    }

    public Vector2 ScreenToLocal(RectTransform parent, Vector2 screenPos)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,screenPos,UICamera,out var pos) ? pos : Vector2.zero;
    }
}