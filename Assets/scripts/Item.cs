using UnityEngine;


    [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
    public class Item : ScriptableObject
    {
        public string itemID = "LlavePrincipal";
        public string itemName = "Llave";
        public Sprite icon;
    }

