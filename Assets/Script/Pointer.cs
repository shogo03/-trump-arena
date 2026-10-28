
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
        // 親オブジェクトからカーソル制御を取得
        controller = GetComponentInParent<CursorController>();

        // ゲーム管理クラスを取得
        manager = FindAnyObjectByType<MemoryBurstManager>();
    }

    private void Update()
    {
        if (controller == null || manager == null)
            return;

        int index = controller.GamepadIndex;

        // コントローラーの接続確認
        if (index < 0 || index >= Gamepad.all.Count)
            return;

        // Aボタンが押されていなければ何もしない
        if (!Gamepad.all[index].buttonSouth.wasPressedThisFrame)
            return;

        // EventSystem の確認
        if (EventSystem.current == null)
        {
            Debug.LogError("EventSystem がありません");
            return;
        }

        // カーソルのワールド座標をスクリーン座標に変換
        Vector2 screenPosition =
            RectTransformUtility.WorldToScreenPoint(
                null,
                transform.position
            );

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position = screenPosition;

        // UI の GraphicRaycaster を取得
        GraphicRaycaster[] raycasters =
            FindObjectsByType<GraphicRaycaster>();

        foreach (GraphicRaycaster raycaster in raycasters)
        {
            if (raycaster == null || !raycaster.isActiveAndEnabled)
                continue;

            results.Clear();
            raycaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results)
            {
                GameObject hitObject = result.gameObject;

                // ターン判定：自分のターン以外は選択不可
                if (!manager.CanControl(index))
                    return;

                // Button があればクリック
                Button button =
                    hitObject.GetComponentInParent<Button>();

                if (button != null)
                {
                    if (button.interactable)
                    {
                        button.onClick.Invoke();
                        return;
                    }

                    continue;
                }

                // Button 以外のクリック可能な UI を検索
                Transform current = hitObject.transform;

                while (current != null)
                {
                    if (current.GetComponent<IPointerClickHandler>() != null)
                    {
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
