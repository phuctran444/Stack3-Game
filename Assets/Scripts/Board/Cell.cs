using UnityEngine;

public class Cell : MonoBehaviour
{
    private SpriteRenderer _spriteRendererChild;

    public void GetChildSpriteRenderer()
    {
        if (_spriteRendererChild == null)
        {
            _spriteRendererChild = transform.GetChild(0).GetComponent<SpriteRenderer>();
        }
    }

    public void Highlight(Color highlightColor)
    {
        _spriteRendererChild.color = highlightColor;
    }

    public void Unhighlight()
    {
        _spriteRendererChild.color = Color.clear;
    }



}//class
