namespace MudServer.Entity {
public interface IInventory {
    Item[] GetItems();
    bool TryGetItem(string name, out Item item);
    void AddItem(Item item);
    void RemoveItem(Item item);
    void RemoveItem(string name);
}
}
