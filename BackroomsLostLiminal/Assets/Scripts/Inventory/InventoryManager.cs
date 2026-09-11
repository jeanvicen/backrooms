using System.Collections.Generic;
using UnityEngine;

namespace Backrooms.Inventory
{
    /// <summary>
    /// Gerencia o inventário do jogador
    /// Sistema de 8 slots com suporte a stacking
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int maxSlots = 8;
        [SerializeField] private List<InventoryItem> inventory = new List<InventoryItem>();
        
        [Header("Starting Items")]
        [SerializeField] private InventoryItem[] startingItems;
        
        // Events
        public delegate void InventoryChangedHandler();
        public event InventoryChangedHandler OnInventoryChanged;
        
        public delegate void ItemAddedHandler(InventoryItem item);
        public event ItemAddedHandler OnItemAdded;
        
        public delegate void ItemRemovedHandler(InventoryItem item);
        public event ItemRemovedHandler OnItemRemoved;
        
        public delegate void ItemUsedHandler(InventoryItem item);
        public event ItemUsedHandler OnItemUsed;
        
        public List<InventoryItem> Inventory => inventory;
        public int SlotCount => maxSlots;

        private void Start()
        {
            InitializeInventory();
        }

        /// <summary>
        /// Inicializa inventário com itens iniciais
        /// </summary>
        private void InitializeInventory()
        {
            inventory.Clear();
            
            for (int i = 0; i < maxSlots; i++)
            {
                inventory.Add(null);
            }
            
            // Adicionar itens iniciais
            if (startingItems != null)
            {
                foreach (InventoryItem item in startingItems)
                {
                    AddItem(item.Clone());
                }
            }
            
            Debug.Log("[Inventory] Inventário inicializado");
        }

        /// <summary>
        /// Adiciona item ao inventário
        /// </summary>
        public bool AddItem(InventoryItem item)
        {
            if (item == null) return false;
            
            // Tentar stackar se for stackável
            if (item.maxStack > 1)
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    if (inventory[i] != null && 
                        inventory[i].itemId == item.itemId && 
                        inventory[i].currentStack < inventory[i].maxStack)
                    {
                        int spaceLeft = inventory[i].maxStack - inventory[i].currentStack;
                        int amountToAdd = Mathf.Min(item.currentStack, spaceLeft);
                        
                        inventory[i].currentStack += amountToAdd;
                        item.currentStack -= amountToAdd;
                        
                        OnInventoryChanged?.Invoke();
                        OnItemAdded?.Invoke(inventory[i]);
                        
                        if (item.currentStack <= 0)
                        {
                            return true;
                        }
                    }
                }
            }
            
            // Adicionar em slot vazio
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] == null)
                {
                    inventory[i] = item;
                    OnInventoryChanged?.Invoke();
                    OnItemAdded?.Invoke(item);
                    
                    Debug.Log($"[Inventory] Item adicionado: {item.itemName}");
                    return true;
                }
            }
            
            Debug.LogWarning("[Inventory] Inventário cheio!");
            return false;
        }

        /// <summary>
        /// Remove item do inventário
        /// </summary>
        public bool RemoveItem(string itemId, int amount = 1)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] != null && inventory[i].itemId == itemId)
                {
                    if (inventory[i].currentStack <= amount)
                    {
                        OnItemRemoved?.Invoke(inventory[i]);
                        inventory[i] = null;
                    }
                    else
                    {
                        inventory[i].currentStack -= amount;
                    }
                    
                    OnInventoryChanged?.Invoke();
                    Debug.Log($"[Inventory] Item removido: {itemId}");
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Remove item por índice de slot
        /// </summary>
        public bool RemoveItemAt(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventory.Count) return false;
            
            if (inventory[slotIndex] != null)
            {
                OnItemRemoved?.Invoke(inventory[slotIndex]);
                inventory[slotIndex] = null;
                OnInventoryChanged?.Invoke();
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Verifica se tem item no inventário
        /// </summary>
        public bool HasItem(string itemId)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] != null && inventory[i].itemId == itemId)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Obtém quantidade de um item
        /// </summary>
        public int GetItemCount(string itemId)
        {
            int count = 0;
            
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] != null && inventory[i].itemId == itemId)
                {
                    count += inventory[i].currentStack;
                }
            }
            
            return count;
        }

        /// <summary>
        /// Obtém item por ID
        /// </summary>
        public InventoryItem GetItem(string itemId)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] != null && inventory[i].itemId == itemId)
                {
                    return inventory[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Obtém item por slot
        /// </summary>
        public InventoryItem GetItemAt(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < inventory.Count)
            {
                return inventory[slotIndex];
            }
            return null;
        }

        /// <summary>
        /// Usa item do inventário
        /// </summary>
        public bool UseItem(string itemId)
        {
            InventoryItem item = GetItem(itemId);
            
            if (item != null)
            {
                OnItemUsed?.Invoke(item);
                
                // Consumir se for consumível
                if (item.itemType == ItemType.Consumable)
                {
                    RemoveItem(itemId, 1);
                }
                
                Debug.Log($"[Inventory] Item usado: {item.itemName}");
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Equipa/Desequipa item
        /// </summary>
        public void ToggleEquip(string itemId)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] != null)
                {
                    if (inventory[i].itemId == itemId)
                    {
                        inventory[i].isEquipped = !inventory[i].isEquipped;
                    }
                    else if (inventory[i].isEquipped)
                    {
                        // Apenas um item equipado por vez (para ferramentas)
                        if (inventory[i].itemType == ItemType.Tool)
                        {
                            inventory[i].isEquipped = false;
                        }
                    }
                }
            }
            
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Limpa todo inventário
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                inventory[i] = null;
            }
            
            OnInventoryChanged?.Invoke();
            Debug.Log("[Inventory] Inventário limpo");
        }

        /// <summary>
        /// Conta slots vazios
        /// </summary>
        public int GetEmptySlotCount()
        {
            int count = 0;
            
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] == null)
                {
                    count++;
                }
            }
            
            return count;
        }

        /// <summary>
        /// Verifica se inventário está cheio
        /// </summary>
        public bool IsFull()
        {
            return GetEmptySlotCount() == 0;
        }

        /// <summary>
        /// Move item entre slots
        /// </summary>
        public bool MoveItem(int fromSlot, int toSlot)
        {
            if (fromSlot < 0 || fromSlot >= inventory.Count ||
                toSlot < 0 || toSlot >= inventory.Count)
            {
                return false;
            }
            
            InventoryItem temp = inventory[toSlot];
            inventory[toSlot] = inventory[fromSlot];
            inventory[fromSlot] = temp;
            
            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Troca item com mundo (para drop)
        /// </summary>
        public InventoryItem DropItem(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < inventory.Count)
            {
                InventoryItem droppedItem = inventory[slotIndex];
                inventory[slotIndex] = null;
                OnInventoryChanged?.Invoke();
                return droppedItem;
            }
            return null;
        }
    }
}
