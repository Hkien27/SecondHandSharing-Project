// Models/ViewModels/UserRevenueDto.cs
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace SecondHandSharing.Models.ViewModels
{
public class UserRevenueDto
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public int Count { get; set; }
}

// Models/ViewModels/AdminRevenueViewModel.cs
public class AdminRevenueViewModel
{
    public List<Transaction> Transactions { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public List<UserRevenueDto> PerUserRevenue { get; set; } = new();

    public int? SelectedYear { get; set; }
    public int? SelectedMonth { get; set; }
    public int? SelectedUserId { get; set; }
}
}