using System.Collections.Generic;
using SecondHandSharing.Models;
public class UserManagementViewModel
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public decimal TotalRevenue { get; set; } // Tổng doanh thu
    public bool IsLocked { get; set; } // Trạng thái khóa
    public List<Item> SoldItems { get; set; } // Danh sách sản phẩm đã bán
}