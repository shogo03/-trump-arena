
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pointer : MonoBehaviour
{
    private CursorController controller;
    private MemoryBurstManager manager;

    private PointerEventData pointerData;

    private readonly List<RaycastResult> results = new();

    private GameObject currentHovered;

    [Header("指先の判定位置")]
    [SerializeField] private Transform tipPoint;

    private void Awake()
    {
        controller = GetComponentInParent<CursorController>();

        manager = FindAnyObjectByType<MemoryBurstManager>();

        Image image = GetComponent<Image>();

        if (image != null)
        {
            image.raycastTarget = false;
        }
    }

    private void OnEnable()
    {
        currentHovered = null;

        InitializePointerData();
    }

    private void OnDisable()
    {
        ClearHover();
    }

    // PointerEventDataの初期化
    private void InitializePointerData()
    {
        if (EventSystem.current == null)
            return;

        pointerData = new PointerEventData(EventSystem.current);

        pointerData.button = PointerEventData.InputButton.Left;
    }

    private void ClearHover()
    {
        if (currentHovered != null && pointerData != null)
        {
            ExecuteEvents.Execute(
                currentHovered,
                pointerData,
                ExecuteEvents.pointerExitHandler
            );
        }

        currentHovered = null;
    }

    private void Update()
    {
        if (controller == null)
            return;

        if (EventSystem.current == null)
            return;

        if (pointerData == null)
        {
            InitializePointerData();

            if (pointerData == null)
                return;
        }

        int index = controller.GamepadIndex;

        if (index < 0 || index >= Gamepad.all.Count)
            return;

        // Canvasの取得
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            return;

        Camera eventCamera = null;

        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = canvas.worldCamera;
        }

        // TipPointの位置を取得
        Vector3 targetPosition = tipPoint != null
            ? tipPoint.position
            : transform.position;

        // ワールド座標をスクリーン座標に変換
        Vector2 screenPosition =
            RectTransformUtility.WorldToScreenPoint(
                eventCamera,
                targetPosition
            );

        pointerData.position = screenPosition;

        // ホバー処理
        UpdateHover(screenPosition);

        // Aボタンでクリック
        if (Gamepad.all[index].buttonSouth.wasPressedThisFrame)
        {
            ClickUI(index, screenPosition);
        }
    }

    // 指定位置にあるUIを取得
    private GameObject FindHitObject(Vector2 screenPosition)
    {
        pointerData.position = screenPosition;

        results.Clear();

        // すべてのUIをまとめて判定
        EventSystem.current.RaycastAll(
            pointerData,
            results
        );

        // 手前にあるUIから確認
        foreach (RaycastResult result in results)
        {
            GameObject hitObject = result.gameObject;

            // 自分のカーソルは無視
            if (hitObject.transform.IsChildOf(transform))
                continue;

            // ホバーを受け取れるUIを取得
            GameObject target =
                ExecuteEvents.GetEventHandler<IPointerEnterHandler>(
                    hitObject
                );

            if (target != null)
                return hitObject;
        }

        return null;
    }

    // ホバー処理

    private void UpdateHover(Vector2 screenPosition)
    {
        GameObject newHovered = null;

        GameObject hitObject = FindHitObject(screenPosition);

        if (hitObject != null)
        {
            newHovered =
                ExecuteEvents.GetEventHandler<IPointerEnterHandler>(
                    hitObject
                );
        }

        if (currentHovered != newHovered)
        {
            ClearHover();

            if (newHovered != null)
            {
                pointerData.position = screenPosition;

                ExecuteEvents.Execute(
                    newHovered,
                    pointerData,
                    ExecuteEvents.pointerEnterHandler
                );

                currentHovered = newHovered;

                Debug.Log(
                    $"ホバー対象：{newHovered.name} / 座標：{screenPosition}"
                );
            }
            else
            {
                Debug.Log("ホバー対象：なし");
            }
        }

        pointerData.position = screenPosition;
    }

    // クリック処理
    private void ClickUI(int index, Vector2 screenPosition)
    {
        pointerData.position = screenPosition;

        results.Clear();

        // UI全体をまとめて判定
        EventSystem.current.RaycastAll(
            pointerData,
            results
        );

        // 手前のUIから確認
        foreach (RaycastResult result in results)
        {
            GameObject hitObject = result.gameObject;

            // 自分のカーソルは無視
            if (hitObject.transform.IsChildOf(transform))
                continue;

            // クリックを受け取るオブジェクトを取得
            GameObject clickTarget =
                ExecuteEvents.GetEventHandler<IPointerClickHandler>(
                    hitObject
                );

            if (clickTarget != null)
            {
                Debug.Log(
                    $"クリック判定：{clickTarget.name} / 座標：{screenPosition}"
                );
            }

            if (clickTarget == null)
                continue;

            // Buttonの操作可能状態を確認
            Selectable selectable =
                clickTarget.GetComponent<Selectable>();

            if (selectable != null &&
                !selectable.IsInteractable())
                continue;

            if (!clickTarget.activeInHierarchy)
                continue;

            // カード操作の制限
            MemoryBurstCardView card =
                clickTarget.GetComponentInParent<MemoryBurstCardView>();

            if (card != null)
            {
                if (manager == null ||
                    !manager.CanControl(index))
                {
                    return;
                }
            }

            // Button・TMP Dropdownなどのクリック処理
            pointerData.button = PointerEventData.InputButton.Left;

            ExecuteEvents.Execute(
                clickTarget,
                pointerData,
                ExecuteEvents.pointerClickHandler
            );

            return;
        }
    }
}
