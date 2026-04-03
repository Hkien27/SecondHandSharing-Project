namespace SecondHandSharing.Models
{
    public class ChatResponseDto
    {
        public string Reply { get; set; }
        public List<ProductSuggestDto> Products { get; set; } = new();
    }

    public class ProductSuggestDto
    {
        public int ItemId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
    }
}
