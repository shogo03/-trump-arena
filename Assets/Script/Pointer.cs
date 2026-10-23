
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pointer : MonoBehaviour
{
    private CursorController controller;

    private readonly List<RaycastResult> results =
        new List<RaycastResult>();

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

        Vector2 screenPosition =
            RectTransformUtility.WorldToScreenPoint(
                null,
                transform.position
            );

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position = screenPosition;

        GraphicRaycaster[] raycasters =
            FindObjectsByType<GraphicRaycaster>();

        foreach (GraphicRaycaster raycaster in raycasters)
        {
            results.Clear();
            raycaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results)
            {
                GameObject hitObject = result.gameObject;

                // Buttonがある場合
                Button button =
                    hitObject.GetComponentInParent<Button>();

                if (button != null && button.interactable)
                {
                    Debug.Log("ボタンを押した：" + button.name);
                    button.onClick.Invoke();
                    return;
                }

                // Imageなど、クリックイベントを受け取る
                // コンポーネントがある場合
                GameObject target = null;

                Transform current = hitObject.transform;

                while (current != null)
                {
                    if (current.GetComponent<IPointerClickHandler>() != null)
                    {
                        target = current.gameObject;
                        break;
                    }

                    current = current.parent;
                }

                if (target != null)
                {
                    Debug.Log("クリックイベントを送信：" + target.name);

                    ExecuteEvents.Execute(
                        target,
                        pointerData,
                        ExecuteEvents.pointerClickHandler
                    );

                    return;
                }
            }
        }

        Debug.Log("カーソル位置にクリック可能なUIがありません");
    }
}
