using System;
using System.Collections.Generic;

namespace SecondHandSharing.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalItems { get; set; }
        public int TotalActiveItems { get; set; }
        public int TotalFavorites { get; set; }
        public int TotalViewHistories { get; set; }

        public List<User> LatestUsers { get; set; } = new();
        public List<Item> LatestItems { get; set; } = new();
    }
}
