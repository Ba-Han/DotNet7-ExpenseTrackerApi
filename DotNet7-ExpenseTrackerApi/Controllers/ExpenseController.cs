using DotNet7_ExpenseTrackerApi.Models.Entities;
using DotNet7_ExpenseTrackerApi.Models.RequestModels.Expense;
using DotNet7_ExpenseTrackerApi.Models.ResponseModels.Expense;
using DotNet7_ExpenseTrackerApi.Queries;
using DotNet7_ExpenseTrackerApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;

namespace DotNet7_ExpenseTrackerApi.Controllers;
public class ExpenseController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly AdoDotNetService _adoDotNetService;
    private readonly AppDbContext _appDbContext;

    public ExpenseController (
        IConfiguration configuration,
        AdoDotNetService adoDotNetService,
        AppDbContext appDbContext
    )
    {
        _configuration = configuration;
        _adoDotNetService = adoDotNetService;
        _appDbContext = appDbContext;
    }

    [HttpGet]
    [Route("/api/expense/{userID}")]
    public IActionResult GetExpenseListByUserId(long userID)
    {
        try
        {
            if (userID <= 0)
                return BadRequest("User Id cannot be empty.");


            string query = ExpenseQuery.GetExpenseListByUserIdQuery();
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@UserId", userID),
                new SqlParameter("@IsActive", true)
            };
            List<ExpenseResponseModel> lst = _adoDotNetService.Query<ExpenseResponseModel>(query, parameters.ToArray());

            return Ok(lst);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpPost]
    [Route("/api/expense")]
    public async Task<IActionResult> CreateExpense([FromBody] ExpenseRequestModel requestModel)
    {
        var transaction = await _appDbContext.Database.BeginTransactionAsync();
        try
        {
            #region Check Expense Category

            var expenseCategory = await _appDbContext.Expense_Category
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ExpenseCategoryId == requestModel.ExpenseCategoryId && x.IsActive);
            if (expenseCategory is null)
                return NotFound("Expense Category Not Found or Inactive,");

            #endregion

            #region Check User

            var user = await _appDbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == requestModel.UserId && x.IsActive);
            if (user is null)
                return NotFound("User Not Found or Inactive.");

            #endregion

            #region Check Balance

            var balance = await _appDbContext.Balance
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == requestModel.UserId);
            if (balance is null)
                return NotFound("Balance Not Found.");

            #endregion

            decimal oldBalance = balance.Amount;
            decimal newBalance = oldBalance - requestModel.Amount;

            #region Balance Update

            balance.Amount = newBalance;
            _appDbContext.Entry(balance).State = EntityState.Modified;
            int balanceResult = await _appDbContext.SaveChangesAsync();

            #endregion

            #region Insert Expense

            ExpenseModel model = new()
            {
                UserId = requestModel.UserId,
                ExpenseCategoryId = requestModel.ExpenseCategoryId,
                Amount = requestModel.Amount,
                CreateDate = requestModel.CreateDate,
                IsActive = true
            };
            await _appDbContext.Expense.AddAsync(model);
            int result = await _appDbContext.SaveChangesAsync();

            #endregion

            if (balanceResult > 0 && result > 0)
            {
                await transaction.CommitAsync();
                return StatusCode(201, "Expense Created.");
            }

            await transaction.RollbackAsync();
            return BadRequest("Creating Fail.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception(ex.Message);
        }
    }

    [HttpPatch]
    [Route("/api/expense/{id}")]
    public async Task<IActionResult> UpdateExpense([FromBody] UpdateExpenseRequestModel requestModel, long id)
    {
        var transaction = await _appDbContext.Database.BeginTransactionAsync();
        try
        {
            #region Check Expense

            var expense = await _appDbContext.Expense
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ExpenseId == id && x.IsActive);
            if (expense is null)
                return NotFound("Expense Not Found or Inactive.");

            #endregion

            #region Check Expense Category

            var expenseCategory = await _appDbContext.Expense_Category
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ExpenseCategoryId == requestModel.ExpenseCategoryId && x.IsActive);
            if (expenseCategory is null)
                return NotFound("Expense Category Not Found or Inactive.");

            #endregion

            #region Check User

            var user = await _appDbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == requestModel.UserId && x.IsActive);
            if (user is null)
                return NotFound("User Not Found or Inactive.");

            #endregion

            #region Check Balance

            var balance = await _appDbContext.Balance
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == requestModel.UserId);
            if (balance is null)
                return NotFound("Balance Not Found.");

            #endregion

            decimal oldBalance = balance.Amount;
            decimal newBalance = 0;
            decimal oldExpense = expense.Amount;
            decimal newExpense = requestModel.Amount;
            decimal expenseDifference = 0;

            if (newExpense > oldExpense)
            {
                expenseDifference = newExpense - oldExpense;
                newBalance = oldBalance - expenseDifference;
            }
            else
            {
                expenseDifference = oldExpense - newExpense;
                newBalance = oldBalance + expenseDifference;
            }

            #region Update Balance

            balance.Amount = newBalance;
            _appDbContext.Entry(balance).State = EntityState.Modified;
            int balanceResult = await _appDbContext.SaveChangesAsync();

            #endregion

            #region Update Expense

            expense.Amount = requestModel.Amount;
            _appDbContext.Entry(expense).State = EntityState.Modified;
            int result = await _appDbContext.SaveChangesAsync();

            #endregion

            if (balanceResult > 0 && result > 0)
            {
                await transaction.CommitAsync();
                return StatusCode(202, "Expense Updated.");
            }

            await transaction.RollbackAsync();
            return BadRequest("Updating Fail.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception(ex.Message);
        }
    }

    [HttpDelete]
    [Route("/api/expense/{id}")]
    public async Task<IActionResult> DeleteExpense(long id)
    {
        var transaction = await _appDbContext.Database.BeginTransactionAsync();
        try
        {
            if (id <= 0)
                return BadRequest();

            #region Check Expense

            var expense = await _appDbContext.Expense
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ExpenseId == id && x.IsActive);
            if (expense is null)
                return NotFound("Expense Not Found or Inactive.");

            #endregion

            long userID = expense.UserId;

            #region Check Balance
            
            var balance = await _appDbContext.Balance
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userID);
            if (balance is null)
                return NotFound("Balance Not Found.");

            #endregion

            #region Update Balance

            decimal updatedBalance = balance.Amount + expense.Amount;
            balance.Amount = updatedBalance;
            _appDbContext.Entry(balance).State = EntityState.Modified;
            int balanceResult = await _appDbContext.SaveChangesAsync();

            #endregion

            #region Delete Expense

            expense.IsActive = false;
            _appDbContext.Entry(expense).State = EntityState.Modified;
            int result = await _appDbContext.SaveChangesAsync();

            #endregion

            if (balanceResult > 0 && result > 0)
            {
                await transaction.CommitAsync();
                return StatusCode(202, "Expense Deleted.");
            }

            await transaction.RollbackAsync();
            return BadRequest("Deleting Fail.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception(ex.Message);
        }
    }
}
