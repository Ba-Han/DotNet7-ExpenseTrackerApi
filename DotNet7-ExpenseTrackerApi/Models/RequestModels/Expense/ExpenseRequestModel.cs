﻿namespace DotNet7_ExpenseTrackerApi.Models.RequestModels.Expense;
public class ExpenseRequestModel
{
   public long ExpenseCategoryId { get; set; }
   public long UserId { get; set; }
   public decimal Amount { get; set; }
   public DateTime CreateDate { get; set; }

}