using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Multi-State Button
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIClickable : MonoBehaviour
    , IDragHandler
    , IPointerEnterHandler
    , IPointerExitHandler
    , IPointerUpHandler
    , IPointerDownHandler
{
    public enum SelectionStates
    {
        Nothing = 0,
        Hovered = 1,
        Pressed = 2,
        Dragged = 4,
    }

    [ReadOnly]
    public SelectionStates selectionState;

    public bool Pressed { get; private set; }
    public bool Hovered { get; private set; }
    public bool Dragged { get; private set; }

    protected RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (Dragged)
        {
            selectionState = SelectionStates.Dragged;
        }
        else if (Pressed)
        {
            selectionState = SelectionStates.Pressed;
        }
        else if (Hovered)
        {
            selectionState = SelectionStates.Hovered;
        }
        else
        {
            selectionState = SelectionStates.Nothing;
        }
    }

    public virtual void OnDrag(PointerEventData e)
    {
        Dragged = true;
    }

    public virtual void OnPointerEnter(PointerEventData e)
    {
        Hovered = true;
    }

    public virtual void OnPointerExit(PointerEventData e)
    {
        Hovered = false;
    }

    public virtual void OnPointerUp(PointerEventData e)
    {
        Dragged = false;
        Pressed = false;
    }

    public virtual void OnPointerDown(PointerEventData e)
    {
        Pressed = true;
    }

    protected Vector2 NormalisedClickPosition(Vector2 position, bool clamp = false)
    {
        position = transform.InverseTransformPoint(position);
        position.x = position.x.LinearRemap(rectTransform.rect.xMin, rectTransform.rect.xMax);
        position.y = position.y.LinearRemap(rectTransform.rect.yMin, rectTransform.rect.yMax);
        if (clamp)
        {
            position.x = Mathf.Clamp01(position.x);
            position.y = Mathf.Clamp01(position.y);
        }
        return position;
    }
}