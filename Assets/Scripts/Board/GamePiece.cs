using System.Collections;
using UnityEngine;

public enum MatchingColor
{
    None,
    Blue,
    Green,
    Orange,
    Yellow,
    Purple,
}

[RequireComponent(typeof(BoxCollider2D))]
public class GamePiece : MonoBehaviour
{
    public int XCoordinate { get; set; }
    public int YCoordinate { get; set; }
    [field: SerializeField] public MatchingColor MatchingColor { get; private set; }

    private BoxCollider2D _boxCollider;
    public bool IsReached { get; private set; } = false;

    public void InitWhileFilling(int x, int y, Transform parent)
    {
        SetCoordinates(x, y);
        transform.SetParent(parent);
        DisableCollider();
    }

    private void OnMouseDown()
    {
        StopAllCoroutines();
    }

    private void OnMouseDrag()
    {
        BoardController.Instance.OnDrag(this);
    }

    private void OnMouseUp()
    {
        DisableCollider();
        BoardController.Instance.OnMouseReleased(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        StopAllCoroutines();
        DisableCollider();
        BoardController.Instance.SnapToGrid(this);
    }

    public void GetCollider()
    {
        if (_boxCollider == null)
        {
            _boxCollider = GetComponent<BoxCollider2D>();
        }
    }

    public void SetCoordinates(int x, int y)
    {
        XCoordinate = x;
        YCoordinate = y;
    }

    private void DisableCollider()
    {
        _boxCollider.enabled = false;
    }

    public void Move(Vector2 endPosition, float timeToMove)
    {
        StartCoroutine(MoveCoroutine(endPosition, timeToMove));
    }

    private IEnumerator MoveCoroutine(Vector2 endPosition, float timeToMove)
    {
        IsReached = false;
        float elapsedTime = 0f;
        Vector2 startPosition = transform.position;

        while (Vector2.Distance(transform.position, endPosition) > 0.01f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / timeToMove;
            transform.position = Vector2.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        transform.position = endPosition;
        IsReached = true;
    }




}//class
