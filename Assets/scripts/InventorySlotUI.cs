using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image highlightBorder;

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
        if (highlightBorder != null)
        {
            highlightBorder.enabled = isSelected;
        }
    }
}
