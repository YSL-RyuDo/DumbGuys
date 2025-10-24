using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class EventTriggerHandler : MonoBehaviour, IPointerClickHandler
{
    public static EventTriggerHandler Instance;
    public event Action<GameObject> OnScreenClicked;

    private void Awake()
    {
        Instance = this;
    }

    // 실제 클릭 감지
    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject clickedObj = eventData.pointerCurrentRaycast.gameObject;
        Debug.Log("[EventTriggerHandler] Click detected on: " + (clickedObj != null ? clickedObj.name : "null"));
        OnScreenClicked?.Invoke(clickedObj);
    }

}
