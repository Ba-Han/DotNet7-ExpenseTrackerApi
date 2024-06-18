namespace DotNet7_ExpenseTrackerApi.Models.RequestModels.Income;
public class UpdateIncomeRequestModel
{
    public long IncomeCategoryId { get; set; }
    public long UserId { get; set; }
    public decimal Amount { get; set; }
}
