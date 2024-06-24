namespace DotNet7_ExpenseTrackerApi.Resources
{
    public class ExpenseCategoryMessageResource
    {
        public static string Duplicate { get; } = "Expense Category Name already exists!";
        public static string SaveSuccess { get; } = "Expense Category Creating Successful.";
        public static string SaveFail { get; } = "Expense Category Creating Fail.";
        public static string RequiredMessage { get; } = "Expense Category Name cannot be empty.";
        public static string UpdateSuccess { get; } = "Updating Successful.";
        public static string UpdateFail { get; } = "Updating Fail.";
        public static string DeleteWarningMessage { get; } = "Expense with this category already exists! Cannot delete.";
        public static string DeleteSuccess { get; } = "Deleting Successful.";
        public static string DeleteFail { get; } = "Deleting Fail.";
    }
}
