using System.ComponentModel.DataAnnotations;

namespace DotNet7_ExpenseTrackerApi.Models.Entities;
public class ExpenseModel
{
    [Key]
    public long ExpenseId { get; set; }
    public long ExpenseCategoryId { get; set; }
    public long UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreateDate { get; set; }
    public bool IsActive { get; set; }
}
