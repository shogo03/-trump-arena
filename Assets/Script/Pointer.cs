
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pointer : MonoBehaviour
{
    private CursorController controller;

    private void Awake()
    {
        controller = GetComponentInParent<CursorController>();
    }

    private void Update()
    {
        if (controller == null)
            return;

        int index = controller.GamepadIndex;

        if (index < 0 || index >= Gamepad.all.Count)
            return;

        if (!Gamepad.all[index].buttonSouth.wasPressedThisFrame)
            return;

        if (EventSystem.current == null)
        {
            Debug.LogError("EventSystem がありません");
            return;
        }

        // TipPointのワールド座標を画面座標に変換
        Vector2 screenPosition =
            RectTransformUtility.WorldToScreenPoint(
                null,
                transform.position
            );

        Debug.Log("TipPointの画面座標：" + screenPosition);

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position = screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();

        GraphicRaycaster[] raycasters =
            FindObjectsByType<GraphicRaycaster>();

        foreach (GraphicRaycaster raycaster in raycasters)
        {
            results.Clear();
            raycaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results)
            {
                Button button =
                    result.gameObject.GetComponentInParent<Button>();

                if (button != null && button.interactable)
                {
                    Debug.Log("ボタンを押した：" + button.name);
                    button.onClick.Invoke();
                    return;
                }
            }
        }

        Debug.Log("TipPointの位置にUI Buttonがありません");
    }
}
