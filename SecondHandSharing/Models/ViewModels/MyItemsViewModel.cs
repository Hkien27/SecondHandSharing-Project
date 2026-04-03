namespace SecondHandSharing.Models.ViewModels
{
    public class MyItemsViewModel
    {
        public List<Item> ApprovedItems { get; set; } = new();
        public List<Item> PendingItems  { get; set; } = new();
        public List<Item> RejectedItems { get; set; } = new();
    }
}
