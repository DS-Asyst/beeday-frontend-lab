namespace BeeDayLab.Web.Components.Behaviors.DragDrop;

public static class SortableOrder
{
    public static IReadOnlyList<Guid> Move(
        IReadOnlyList<Guid> currentOrder,
        Guid itemId,
        Guid targetItemId,
        bool placeAfter)
    {
        ArgumentNullException.ThrowIfNull(currentOrder);

        var result = currentOrder.ToList();
        var sourceIndex = result.IndexOf(itemId);
        var targetIndex = result.IndexOf(targetItemId);

        if (sourceIndex < 0 || targetIndex < 0 || itemId == targetItemId)
        {
            return result;
        }

        result.RemoveAt(sourceIndex);
        targetIndex = result.IndexOf(targetItemId);
        var insertIndex = placeAfter ? targetIndex + 1 : targetIndex;
        result.Insert(Math.Clamp(insertIndex, 0, result.Count), itemId);

        return result;
    }
}
