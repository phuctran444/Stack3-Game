using System.Collections;
using UnityEngine;

public class MoveUI : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Vector2 _midPosition;
    [SerializeField] private Vector2 _endPosition;
    [SerializeField] private float _timeToMove;

    public bool IsReached { get; private set; }

    public void MoveIn()
    {
        Vector2 currentPosition = _rectTransform.anchoredPosition;
        StartCoroutine(MoveCoroutine(currentPosition, _midPosition, _timeToMove));
    }

    public void MoveOut()
    {
        Vector2 currentPosition = _rectTransform.anchoredPosition;
        StartCoroutine(MoveCoroutine(currentPosition, _endPosition, _timeToMove));
    }

    private IEnumerator MoveCoroutine(Vector2 startPosition, Vector2 destination, float timeToMove)
    {
        IsReached = false;
        float elapsedTime = 0f;

        while (Vector2.Distance(_rectTransform.anchoredPosition, destination) > 0.01f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / timeToMove;
            _rectTransform.anchoredPosition = Vector2.Lerp(startPosition, destination, t);
            yield return null;
        }

        _rectTransform.anchoredPosition = destination;
        IsReached = true;

        if (_rectTransform.anchoredPosition == _endPosition)
        {
            gameObject.SetActive(false);
        }
    }







}//class
