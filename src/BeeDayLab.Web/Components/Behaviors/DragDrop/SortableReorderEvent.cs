namespace BeeDayLab.Web.Components.Behaviors.DragDrop;

public sealed record SortableReorderEvent(
    string ItemId,
    string TargetItemId,
    bool PlaceAfter);
