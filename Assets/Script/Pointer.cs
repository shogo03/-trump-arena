
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pointer : MonoBehaviour
{
    private CursorController controller;
    private MemoryBurstManager manager;

    private readonly List<RaycastResult> results = new();

    private void Awake()
    {
        controller = GetComponentInParent<CursorController>();
        manager = FindAnyObjectByType<MemoryBurstManager>();
    }

    private void Update()
    {
        if (controller == null)
            return;

        int index = controller.GamepadIndex;

        // 接続中のコントローラーか確認
        if (index < 0 || index >= Gamepad.all.Count)
            return;

        // Aボタンが押されたときだけ処理
        if (!Gamepad.all[index].buttonSouth.wasPressedThisFrame)
            return;

        if (EventSystem.current == null)
        {
            Debug.LogError("EventSystem がありません");
            return;
        }

        // カーソル位置をスクリーン座標に変換
        Canvas canvas = GetComponentInParent<Canvas>();

        Camera eventCamera = null;

        if (canvas != null &&
            canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = canvas.worldCamera;
        }

        Vector2 screenPosition =
            RectTransformUtility.WorldToScreenPoint(
                eventCamera,
                transform.position
            );

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position = screenPosition;

        // UI の GraphicRaycaster を検索
        GraphicRaycaster[] raycasters =
            FindObjectsByType<GraphicRaycaster>();

        foreach (GraphicRaycaster raycaster in raycasters)
        {
            if (raycaster == null || !raycaster.isActiveAndEnabled)
                continue;

            // カーソル位置にあるUIを取得
            results.Clear();
            raycaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results)
            {
                GameObject hitObject = result.gameObject;

                // Buttonを検索
                Button button =
                    hitObject.GetComponentInParent<Button>();

                if (button != null)
                {
                    if (!button.interactable ||
                        !button.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    // カード内のButtonはターン制限を適用
                    MemoryBurstCardView card =
                        button.GetComponentInParent<MemoryBurstCardView>();

                    if (card != null)
                    {
                        if (manager == null ||
                            !manager.CanControl(index))
                        {
                            return;
                        }
                    }

                    // 通常のUIボタンはターンに関係なく押せる
                    button.onClick.Invoke();
                    return;
                }

                // Button以外のクリック可能なUIを検索
                Transform current = hitObject.transform;

                while (current != null)
                {
                    IPointerClickHandler clickHandler =
                        current.GetComponent<IPointerClickHandler>();

                    if (clickHandler != null)
                    {
                        // カードなら自分のターンだけ操作可能
                        MemoryBurstCardView card =
                            current.GetComponentInParent<MemoryBurstCardView>();

                        if (card != null &&
                            (manager == null ||
                             !manager.CanControl(index)))
                        {
                            return;
                        }

                        ExecuteEvents.Execute(
                            current.gameObject,
                            pointerData,
                            ExecuteEvents.pointerClickHandler
                        );

                        return;
                    }

                    current = current.parent;
                }
            }
        }
    }
}
