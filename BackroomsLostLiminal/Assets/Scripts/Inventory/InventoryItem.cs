using UnityEngine;

namespace Backrooms.Inventory
{
    /// <summary>
    /// Representa um item no inventário
    /// </summary>
    [System.Serializable]
    public class InventoryItem
    {
        public string itemId;
        public string itemName;
        public string description;
        public ItemType itemType;
        public Sprite icon;
        public int maxStack = 1;
        public int currentStack = 1;
        public bool isEquipped = false;
        
        // Dados específicos do item
        public float batteryLevel = 100f; // Para itens eletrônicos
        public float durability = 100f;   // Para itens quebráveis
        public string customData = "";    // Para dados adicionais
        
        public InventoryItem Clone()
        {
            return (InventoryItem)this.MemberwiseClone();
        }
    }

    /// <summary>
    /// Tipos de itens possíveis
    /// </summary>
    public enum ItemType
    {
        Consumable,     // Comida, bebida, remédio
        Tool,           // Lanterna, rádio, chaves
        KeyItem,        // Itens de progresso (chaves, cartões)
        Document,       // Notas, documentos, lore
        Weapon,         // Armas (se aplicável)
        Misc            // Outros itens
    }
}
