namespace SecondHandSharing.Models.ViewModels
{
    public class VipPackage
    {
        public string Name { get; set; }   // VD: "1 tháng VIP"
        public int Months { get; set; }
        public decimal Price { get; set; } // Giá gói
    }

    public class BoostPackage
    {
        public string Name { get; set; }   // VD: "Đẩy tin 3 ngày"
        public int Days { get; set; }
        public decimal Price { get; set; }
    }

    public class VipViewModel
    {
        public bool IsVip { get; set; }
        public DateTime? VipExpireAt { get; set; }
        public List<VipPackage> Packages { get; set; } = new();
    }

    public class BoostViewModel
    {
        public Item Item { get; set; }
        public List<BoostPackage> Packages { get; set; } = new();
    }
}
