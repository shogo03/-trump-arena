
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

    private GraphicRaycaster[] raycasters;
    private PointerEventData pointerData;

    private GameObject currentHovered;

    [Header("カーソル当たり判定")]
    [SerializeField] private float hitRadius = 20f;

    [Header("指先の判定位置")]
    [SerializeField] private Transform tipPoint;

    private readonly Vector2[] directions =
    {
        Vector2.zero,
        new Vector2(0, 1),
        new Vector2(0, -1),
        new Vector2(1, 0),
        new Vector2(-1, 0),
        new Vector2(1, 1).normalized,
        new Vector2(-1, 1).normalized,
        new Vector2(1, -1).normalized,
        new Vector2(-1, -1).normalized
    };

    private void Awake()
    {
        controller = GetComponentInParent<CursorController>();
        manager = FindAnyObjectByType<MemoryBurstManager>();

        Image image = GetComponent<Image>();

        if (image != null)
            image.raycastTarget = false;
    }

    private void OnEnable()
    {
        currentHovered = null;

        RefreshRaycasters();

        if (EventSystem.current != null)
        {
            pointerData = new PointerEventData(EventSystem.current);
        }
    }

    private void OnDisable()
    {
        ClearHover();
    }

    private void RefreshRaycasters()
    {
        raycasters = FindObjectsByType<GraphicRaycaster>();
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
        if (controller == null || pointerData == null)
            return;

        int index = controller.GamepadIndex;

        if (index < 0 || index >= Gamepad.all.Count)
            return;

        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            return;

        Camera eventCamera = null;

        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            eventCamera = canvas.worldCamera;

        // TipPointが設定されていれば、その位置を判定位置にする
        // 未設定の場合はカーソル本体の位置を使う
        Vector3 targetPosition = tipPoint != null
            ? tipPoint.position
            : transform.position;

        Vector2 screenPosition =
            RectTransformUtility.WorldToScreenPoint(
                eventCamera,
                targetPosition
            );

        pointerData.position = screenPosition;

        UpdateHover(screenPosition);

        if (Gamepad.all[index].buttonSouth.wasPressedThisFrame)
        {
            ClickUI(index, screenPosition);
        }
    }


    private GameObject FindHitObject(Vector2 centerPosition)
    {
        foreach (Vector2 direction in directions)
        {
            Vector2 samplePosition =
                centerPosition + direction * hitRadius;

            pointerData.position = samplePosition;

            foreach (GraphicRaycaster raycaster in raycasters)
            {
                if (raycaster == null ||
                    !raycaster.isActiveAndEnabled)
                    continue;

                results.Clear();
                raycaster.Raycast(pointerData, results);

                foreach (RaycastResult result in results)
                {
                    GameObject hitObject = result.gameObject;

                    if (hitObject.transform.IsChildOf(transform))
                        continue;

                    // PointerEnterを受け取れるUIを探す
                    GameObject target =
                        ExecuteEvents.GetEventHandler<IPointerEnterHandler>(
                            hitObject
                        );

                    if (target != null)
                        return hitObject;
                }
            }
        }

        return null;
    }


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
            }
        }

        pointerData.position = screenPosition;
    }

    private void ClickUI(int index, Vector2 screenPosition)
    {
        foreach (Vector2 direction in directions)
        {
            Vector2 samplePosition =
                screenPosition + direction * hitRadius;

            pointerData.position = samplePosition;

            foreach (GraphicRaycaster raycaster in raycasters)
            {
                if (raycaster == null ||
                    !raycaster.isActiveAndEnabled)
                    continue;

                results.Clear();
                raycaster.Raycast(pointerData, results);

                foreach (RaycastResult result in results)
                {
                    GameObject hitObject = result.gameObject;

                    if (hitObject.transform.IsChildOf(transform))
                        continue;

                    Button button =
                        hitObject.GetComponentInParent<Button>();

                    if (button != null)
                    {
                        if (!button.interactable ||
                            !button.gameObject.activeInHierarchy)
                            continue;

                        MemoryBurstCardView card =
                            button.GetComponentInParent<MemoryBurstCardView>();

                        if (card != null &&
                            (manager == null ||
                             !manager.CanControl(index)))
                            return;

                        button.onClick.Invoke();

                        pointerData.position = screenPosition;
                        return;
                    }

                    Transform current = hitObject.transform;

                    while (current != null)
                    {
                        IPointerClickHandler clickHandler =
                            current.GetComponent<IPointerClickHandler>();

                        if (clickHandler != null)
                        {
                            MemoryBurstCardView card =
                                current.GetComponentInParent<MemoryBurstCardView>();

                            if (card != null &&
                                (manager == null ||
                                 !manager.CanControl(index)))
                                return;

                            ExecuteEvents.Execute(
                                current.gameObject,
                                pointerData,
                                ExecuteEvents.pointerClickHandler
                            );

                            pointerData.position = screenPosition;
                            return;
                        }

                        current = current.parent;
                    }
                }
            }
        }

        pointerData.position = screenPosition;
    }
}
