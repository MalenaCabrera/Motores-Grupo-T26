using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Configuración de Slots")]
    [SerializeField] private int totalSlots = 5;

    [Tooltip("Opcional. Al empezar, los slots se buscan solos dentro del objeto 'Hotbar' (en orden de jerarquía). " +
             "Esta lista solo se usa si no existe un objeto llamado 'Hotbar'.")]
    [SerializeField] private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();

    private Item[] _slots;
    private int _selectedIndex = 0;
    private Item _lastSelected;

    // Se dispara cuando cambia el ítem que tenés en la mano (null = mano vacía)
    public event System.Action<Item> OnSelectedItemChanged;

    public Item SelectedItem => (_slots != null && _selectedIndex < _slots.Length) ? _slots[_selectedIndex] : null;

    private void Awake()
    {
        _slots = new Item[totalSlots];
    }

    private void Start()
    {
        FindSlotUIs();
        UpdateUI();
    }

    private void Update()
    {
        HandleHotbarSelection();
    }

    // Busca los slots solos: se toman en el orden en que están en la jerarquía (Slot1, Slot2, ...).
    private void FindSlotUIs()
    {
        GameObject hotbar = GameObject.Find("Hotbar");

        if (hotbar != null)
        {
            InventorySlotUI[] found = hotbar.GetComponentsInChildren<InventorySlotUI>(true);
            if (found.Length > 0)
            {
                slotUIs = new List<InventorySlotUI>(found);
            }
        }

        if (slotUIs.Count != totalSlots)
        {
            Debug.LogWarning($"Inventory: hay {slotUIs.Count} slots en la UI pero totalSlots = {totalSlots}.", this);
        }
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
        if (_slots == null) return;

        for (int i = 0; i < slotUIs.Count; i++)
        {
            if (slotUIs[i] == null) continue;

            Item item = (i < _slots.Length) ? _slots[i] : null;

            if (item != null && item.icon == null)
            {
                Debug.LogWarning($"Inventory: el ítem '{item.itemName}' no tiene ícono (Sprite) asignado.", item);
            }

            slotUIs[i].SetItem(item);
            slotUIs[i].SetSelected(i == _selectedIndex);
        }

        // Avisar si cambió lo que hay en la mano (cambio de slot, agarrar o sacar un ítem)
        Item selected = SelectedItem;
        if (selected != _lastSelected)
        {
            _lastSelected = selected;
            OnSelectedItemChanged?.Invoke(selected);
        }
    }
}



//using System.Collections.Generic;
//using UnityEngine;

//public class Inventory : MonoBehaviour
//{
//    [Header("Configuración de Slots")]
//    [SerializeField] private int totalSlots = 5;

//    [Tooltip("Opcional. Al empezar, los slots se buscan solos dentro del objeto 'Hotbar' (en orden de jerarquía). " +
//             "Esta lista solo se usa si no existe un objeto llamado 'Hotbar'.")]
//    [SerializeField] private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();

//    private Item[] _slots;
//    private int _selectedIndex = 0;

//    private void Awake()
//    {
//        _slots = new Item[totalSlots];
//    }

//    private void Start()
//    {
//        FindSlotUIs();
//        UpdateUI();
//    }

//    private void Update()
//    {
//        HandleHotbarSelection();
//    }

//    // Busca los slots solos: se toman en el orden en que están en la jerarquía (Slot1, Slot2, ...).
//    private void FindSlotUIs()
//    {
//        GameObject hotbar = GameObject.Find("Hotbar");

//        if (hotbar != null)
//        {
//            InventorySlotUI[] found = hotbar.GetComponentsInChildren<InventorySlotUI>(true);
//            if (found.Length > 0)
//            {
//                slotUIs = new List<InventorySlotUI>(found);
//            }
//        }

//        if (slotUIs.Count != totalSlots)
//        {
//            Debug.LogWarning($"Inventory: hay {slotUIs.Count} slots en la UI pero totalSlots = {totalSlots}.", this);
//        }
//    }

//    private void HandleHotbarSelection()
//    {
//        float scroll = Input.GetAxis("Mouse ScrollWheel");

//        if (scroll > 0f)
//        {
//            _selectedIndex--;
//            if (_selectedIndex < 0) _selectedIndex = totalSlots - 1;
//            UpdateUI();
//        }
//        else if (scroll < 0f)
//        {
//            _selectedIndex++;
//            if (_selectedIndex >= totalSlots) _selectedIndex = 0;
//            UpdateUI();
//        }

//        for (int i = 0; i < totalSlots; i++)
//        {
//            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
//            {
//                _selectedIndex = i;
//                UpdateUI();
//                break;
//            }
//        }
//    }

//    public bool AgregarItem(Item item)
//    {
//        for (int i = 0; i < _slots.Length; i++)
//        {
//            if (_slots[i] == null)
//            {
//                _slots[i] = item;
//                UpdateUI();
//                return true;
//            }
//        }
//        Debug.Log("Inventario lleno");
//        return false;
//    }

//    public void RemoverItem(Item item)
//    {
//        for (int i = 0; i < _slots.Length; i++)
//        {
//            if (_slots[i] == item)
//            {
//                _slots[i] = null;
//                UpdateUI();
//                return;
//            }
//        }
//    }

//    public Item GetSelectedItem()
//    {
//        return _slots[_selectedIndex];
//    }

//    public bool TieneItemEnMano(string itemID)
//    {
//        Item enMano = GetSelectedItem();
//        return enMano != null && enMano.itemID == itemID;
//    }

//    public bool TieneItem(string itemID)
//    {
//        for (int i = 0; i < _slots.Length; i++)
//        {
//            if (_slots[i] != null && _slots[i].itemID == itemID)
//            {
//                return true;
//            }
//        }
//        return false;
//    }

//    public void RemoverPorID(string itemID)
//    {
//        for (int i = 0; i < _slots.Length; i++)
//        {
//            if (_slots[i] != null && _slots[i].itemID == itemID)
//            {
//                _slots[i] = null;
//                UpdateUI();
//                return;
//            }
//        }
//    }

//    private void UpdateUI()
//    {
//        if (_slots == null) return;

//        for (int i = 0; i < slotUIs.Count; i++)
//        {
//            if (slotUIs[i] == null) continue;

//            Item item = (i < _slots.Length) ? _slots[i] : null;

//            if (item != null && item.icon == null)
//            {
//                Debug.LogWarning($"Inventory: el ítem '{item.itemName}' no tiene ícono (Sprite) asignado.", item);
//            }

//            slotUIs[i].SetItem(item);
//            slotUIs[i].SetSelected(i == _selectedIndex);
//        }
//    }
//}


