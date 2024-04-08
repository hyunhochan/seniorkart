using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PreserveSpriteSize : MonoBehaviour
{
    void Start()
    {
        var image = GetComponent<Image>();
        var sprite = image.sprite;

        var rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
    }
}
