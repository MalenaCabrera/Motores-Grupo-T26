using UnityEngine;


[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemID = "LlavePrincipal";
    public string itemName = "Llave";
    public Sprite icon;

    [Header("En la mano (opcional)")]
    [Tooltip("Prefab que aparece en la mano del personaje cuando este ítem está seleccionado en el inventario. " +
             "Dejalo vacío si el ítem no se muestra en la mano (por ejemplo la llave).")]
    public GameObject handPrefab;
}


//using UnityEngine;


//    [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
//    public class Item : ScriptableObject
//    {
//        public string itemID = "LlavePrincipal";
//        public string itemName = "Llave";
//        public Sprite icon;
//    }

