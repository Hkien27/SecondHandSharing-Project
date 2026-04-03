namespace SecondHandSharing.Models.ViewModels
{
    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Total { get; set; }
        public int Count { get; set; }
    }

    public class MyRevenueViewModel
    {
        public List<Item> SoldItems { get; set; } = new();
        public decimal TotalRevenue { get; set; }

        // Thống kê doanh thu theo tháng (cho 1 năm)
        public List<MonthlyRevenueDto> MonthlyStats { get; set; } = new();

        // Dùng cho bộ lọc
        public int? SelectedYear { get; set; }
        public int? SelectedMonth { get; set; }
        public List<int> Years { get; set; } = new();
    }
}
