using System.Collections.Generic;

namespace MudServer.Entity {
public class Inventory : IInventory {
    private List<Item> _items = new List<Item>();
    public Item[] GetItems() {
        return _items.ToArray();
    }
    public bool TryGetItem(string name, out Item item) {
        item = _items.Find(i => i.Name == name);
        return item != null;
    }
    public void AddItem(Item item) {
        if (item == null) return;
        if (TryGetItem(item.Name, out Item _)) return;
        
        _items.Add(item);
    }
    public void RemoveItem(Item item) {
        if (item == null) return;
        _items.Remove(item);
    }
    public void RemoveItem(string name) {
        if (TryGetItem(name, out Item item)) {
            _items.Remove(item);
        }
    }
}
}
