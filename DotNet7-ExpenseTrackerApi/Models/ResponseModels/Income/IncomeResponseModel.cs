namespace DotNet7_ExpenseTrackerApi.Models.ResponseModels.Income;
public class IncomeResponseModel
{
    public long IncomeId { get; set; }
    public string UserName { get; set; } = null!;
    public string IncomeCategoryName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string CreateDate { get; set; } = null!;
    public bool IsActive { get; set; }
}
