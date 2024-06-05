using Integration.Common;
using Integration.Backend;
using System.Collections.Concurrent;

namespace Integration.Service;

public sealed class ItemIntegrationService
{
    // This is a dependency that is normally fulfilled externally.
    private ItemOperationBackend ItemIntegrationBackend { get; set; } = new();

    // Concurrent dictionary to track ongoing save operations
    private static readonly ConcurrentDictionary<string, bool> _savingItems = new ConcurrentDictionary<string, bool>();

    // This is called externally and can be called multithreaded, in parallel.
    // More than one item with the same content should not be saved. However,
    // calling this with different contents at the same time is OK, and should
    // be allowed for performance reasons.
    public Result SaveItem(string itemContent)
    {
        // Try to add the item content to the dictionary
        if (!_savingItems.TryAdd(itemContent, true))
        {
            // If it is already being processed, return a duplicate result
            return new Result(false, $"Duplicate item received with content {itemContent}.");
        }

        try
        {
            // Check the backend to see if the content is already saved.
            if (ItemIntegrationBackend.FindItemsWithContent(itemContent).Count != 0)
            {
                return new Result(false, $"Duplicate item received with content {itemContent}.");
            }

            var item = ItemIntegrationBackend.SaveItem(itemContent);

            return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");
        }
        catch (Exception ex)
        {
            return new Result(false, $"An error occured with item:{itemContent}  Error:{ex}");
        }
    }

    public List<Item> GetAllItems()
    {
        return ItemIntegrationBackend.GetAllItems();
    }
}