using System.Collections.Generic;
using SecondHandSharing.Models;

namespace SecondHandSharing.Models.ViewModels
{
    public class BuyBoostViewModel
    {
        // ✅ Danh sách tin của user để chọn tin cần đẩy
        public List<Item> MyItems { get; set; } = new List<Item>();

        // ✅ Các gói BOOST (ServicePackage có Type = "BOOST")
        public List<ServicePackage> BoostPackages { get; set; } = new List<ServicePackage>();
    }
}
