namespace DotNet7_ExpenseTrackerApi.Models.RequestModels.Income;
public class IncomeRequestModel
{
    public long IncomeCategoryId { get; set; }
    public long UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreateDate { get; set; }
}
