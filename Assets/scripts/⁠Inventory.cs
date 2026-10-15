using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Configuración de Slots")]
    [SerializeField] private int totalSlots = 5;
    [SerializeField] private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();

    private Item[] _slots;
    private int _selectedIndex = 0;

    private void Awake()
    {
        _slots = new Item[totalSlots];
    }

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        HandleHotbarSelection();
    }

    private void HandleHotbarSelection()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            _selectedIndex--;
            if (_selectedIndex < 0) _selectedIndex = totalSlots - 1;
            UpdateUI();
        }
        else if (scroll < 0f)
        {
            _selectedIndex++;
            if (_selectedIndex >= totalSlots) _selectedIndex = 0;
            UpdateUI();
        }

        for (int i = 0; i < totalSlots; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                _selectedIndex = i;
                UpdateUI();
                break;
            }
        }
    }

    public bool AgregarItem(Item item)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null)
            {
                _slots[i] = item;
                UpdateUI();
                return true;
            }
        }
        Debug.Log("Inventario lleno");
        return false;
    }

    public void RemoverItem(Item item)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == item)
            {
                _slots[i] = null;
                UpdateUI();
                return;
            }
        }
    }

    public Item GetSelectedItem()
    {
        return _slots[_selectedIndex];
    }

    public bool TieneItemEnMano(string itemID)
    {
        Item enMano = GetSelectedItem();
        return enMano != null && enMano.itemID == itemID;
    }

    public bool TieneItem(string itemID)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] != null && _slots[i].itemID == itemID)
            {
                return true;
            }
        }
        return false;
    }

    public void RemoverPorID(string itemID)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] != null && _slots[i].itemID == itemID)
            {
                _slots[i] = null;
                UpdateUI();
                return;
            }
        }
    }

    private void UpdateUI()
    {
        if (_slots == null)
        {
            Debug.LogError("El inventario no está inicializado.", this);
            return;
        }

        for (int i = 0; i < slotUIs.Count; i++)
        {
            if (slotUIs[i] == null)
            {
                Debug.LogError(
                    $"El Slot UI en la posición {i} no está asignado.",
                    this
                );
                continue;
            }

            if (i < _slots.Length)
            {
                slotUIs[i].SetItem(_slots[i]);
                slotUIs[i].SetSelected(i == _selectedIndex);
            }
        }
    }
    //private void UpdateUI()
    //{
    //    for (int i = 0; i < slotUIs.Count; i++)
    //    {
    //        if (i < _slots.Length)
    //        {
    //            slotUIs[i].SetItem(_slots[i]);
    //            slotUIs[i].SetSelected(i == _selectedIndex);
    //        }
    //    }
    //}
}