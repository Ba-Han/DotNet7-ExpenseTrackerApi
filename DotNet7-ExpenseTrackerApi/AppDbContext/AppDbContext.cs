﻿using DotNet7_ExpenseTrackerApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotNet7_ExpenseTrackerApi;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options) { }

    public DbSet<IncomeModel> Income { get; set; }
    public DbSet<IncomeCategoryModel> Income_Category { get; set; }
    public DbSet<ExpenseModel> Expense { get; set; }
    public DbSet<ExpenseCategoryModel> Expense_Category { get; set; }
    public DbSet<BalanceModel> Balance { get; set; }
    public DbSet<UserModel> Users { get; set; }
}

