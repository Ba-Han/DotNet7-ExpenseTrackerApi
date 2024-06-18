namespace DotNet7_ExpenseTrackerApi.Models.ResponseModels.Expense;
public class ExpenseResponseModel
{
    public long ExpenseId { get; set; }
    public string ExpenseCategoryName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string CreateDate { get; set; } = null!;
    public bool IsActive { get; set; }
}
