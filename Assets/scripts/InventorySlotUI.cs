using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("Referencias (las que ya tenías)")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image highlightBorder;

    [Header("Selección (nuevo)")]
    [Tooltip("Fondo del slot (el cuadradito). Opcional: si se asigna, cambia de color al seleccionarlo.")]
    [SerializeField] private Image background;
    [SerializeField] private float selectedScale = 1.25f;
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.45f);
    [Tooltip("Velocidad de la animación de agrandado. 0 = instantáneo.")]
    [SerializeField] private float animSpeed = 14f;

    private Vector3 targetScale = Vector3.one;

    private void Awake()
    {
        // Arranca como "no seleccionado" hasta que Inventory le avise
        ApplyVisuals(false);
        transform.localScale = Vector3.one;
    }

    private void Update()
    {
        if (animSpeed <= 0f)
        {
            transform.localScale = targetScale;
            return;
        }

        // unscaledDeltaTime: sigue funcionando si el juego está en pausa (Time.timeScale = 0)
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animSpeed);
    }

    public void SetItem(Item item)
    {
        if (item != null && item.icon != null)
        {
            iconImage.sprite = item.icon;
            iconImage.color = Color.white;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.color = Color.clear;
            iconImage.enabled = false;
        }
    }

    public void SetSelected(bool isSelected)
    {
        ApplyVisuals(isSelected);
    }

    private void ApplyVisuals(bool isSelected)
    {
        if (highlightBorder != null)
        {
            highlightBorder.enabled = isSelected;
        }

        if (background != null)
        {
            background.color = isSelected ? selectedColor : normalColor;
        }

        targetScale = isSelected ? Vector3.one * selectedScale : Vector3.one;
    }
}


//public class InventorySlotUI : MonoBehaviour
//{
//    [SerializeField] private Image iconImage;
//    [SerializeField] private Image highlightBorder;

//    public void SetItem(Item item)
//    {
//        if (item != null && item.icon != null)
//        {
//            iconImage.sprite = item.icon;
//            iconImage.color = Color.white;
//            iconImage.enabled = true;
//        }
//        else
//        {
//            iconImage.sprite = null;
//            iconImage.color = Color.clear;
//            iconImage.enabled = false;
//        }
//    }

//    public void SetSelected(bool isSelected)
//    {
//        if (highlightBorder != null)
//        {
//            highlightBorder.enabled = isSelected;
//        }
//    }
//}
