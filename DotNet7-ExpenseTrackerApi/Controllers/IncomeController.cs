﻿using DotNet7_ExpenseTrackerApi.Models.Entities;
using DotNet7_ExpenseTrackerApi.Models.RequestModels.Income;
using DotNet7_ExpenseTrackerApi.Models.ResponseModels.Income;
using DotNet7_ExpenseTrackerApi.Queries;
using DotNet7_ExpenseTrackerApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;

namespace DotNet7_ExpenseTrackerApi.Controllers;
public class IncomeController : BaseController
{
    private readonly IConfiguration _configuration;
    private readonly AdoDotNetService _adoDotNetService;
    private readonly AppDbContext _appDbContext;

    public IncomeController(IConfiguration configuration, AdoDotNetService adoDotNetService, AppDbContext appDbContext)
    {
        _configuration = configuration;
        _adoDotNetService = adoDotNetService;
        _appDbContext = appDbContext;
    }

    [HttpGet]
    [Route("/api/income/{userID}")]
    public IActionResult GetIncomeListByUserId(long userID)
    {
        try
        {
            if (userID <= 0)
                return BadRequest("User Id cannot be empty.");

            string query = IncomeQuery.GetIncomeListByUserIdQuery();
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@UserId", userID),
                new SqlParameter("@IsActive", true)
            };
            List<IncomeResponseModel> lst = _adoDotNetService.Query<IncomeResponseModel>(query, parameters.ToArray());

            return Ok(lst);
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }

    [HttpPost]
    [Route("/api/income")]
    public async Task<IActionResult> CreateIncome([FromBody] IncomeRequestModel requestModel)
    {
        var transaction = await _appDbContext.Database.BeginTransactionAsync();
        try
        {
            #region Check Balance according to the User ID
            string query = BalanceQuery.GetBalanceByUserId();
            SqlParameter[] parameters = { new("@UserId", requestModel.UserId) };
            var dt = _adoDotNetService.QueryFirstOrDefault(query, parameters);

            if (dt.Rows.Count == 0)
                return NotFound("Balance not found.");

            decimal balance = Convert.ToDecimal(dt.Rows[0]["Amount"]);
            #endregion

            #region Check Income Category Valid
            var incomeCategory = await _appDbContext.Income_Category
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IncomeCategoryId == requestModel.IncomeCategoryId && x.IsActive);
            if (incomeCategory is null)
                return NotFound("Income Category Not Found or Inactive.");
            #endregion

            #region Check User Valid
            var user = await _appDbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == requestModel.UserId && x.IsActive);
            if (user is null)
                return NotFound("User Not Found or Inactive");
            #endregion

            decimal updatedBalance = balance + requestModel.Amount;

            #region Balance Update
            string balanceUpdateQuery = BalanceQuery.UpdateBalanceQuery();
            List<SqlParameter> balanceUpdateParams = new()
            {
                new SqlParameter("@Amount", updatedBalance),
                new SqlParameter("@UserId", Convert.ToInt64(dt.Rows[0]["UserId"])),
                new SqlParameter("@UpdateDate", DateTime.Now)
            };
            int balanceResult = _adoDotNetService.Execute(balanceUpdateQuery, balanceUpdateParams.ToArray());
            #endregion

            #region Create Income
            IncomeModel model = new()
            {
                Amount = requestModel.Amount,
                CreateDate = requestModel.CreateDate,
                IncomeCategoryId = requestModel.IncomeCategoryId,
                IsActive = true,
                UserId = requestModel.UserId
            };
            await _appDbContext.Income.AddAsync(model);
            int incomeResult = await _appDbContext.SaveChangesAsync();
            #endregion

            if (balanceResult > 0 && incomeResult > 0)
            {
                await transaction.CommitAsync();
                return StatusCode(201, "Creating Successful.");
            }

            await transaction.RollbackAsync();
            return BadRequest("Creating Fail.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return InternalServerError(ex);
        }
    }

    [HttpPatch]
    [Route("/api/income/{id}")]
    public async Task<IActionResult> UpdateIncome([FromBody] UpdateIncomeRequestModel requestModel, long id)
    {
        var transaction = await _appDbContext.Database.BeginTransactionAsync();
        try
        {
            #region Check Income
            var item = await _appDbContext.Income
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IncomeId == id && x.IsActive);
            if (item is null)
                return NotFound("Income Not Found.");
            #endregion

            #region Check Income Category
            var incomeCategory = await _appDbContext.Income_Category
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IncomeCategoryId == requestModel.IncomeCategoryId && x.IsActive);
            if (incomeCategory is null)
                return NotFound("Income Category Not Found or Inactive.");
            #endregion

            #region Check User Valid
            var user = await _appDbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == requestModel.UserId && x.IsActive);
            if (user is null)
                return NotFound("User Not Found or Inactive");
            #endregion

            #region Check Balance
            var balance = await _appDbContext.Balance
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == requestModel.UserId);
            if (balance is null)
                return NotFound("Balance Not Found.");
            #endregion

            decimal oldBalance = balance.Amount;
            decimal oldIncome = item.Amount;
            decimal newIncome = requestModel.Amount;
            decimal incomeDifference = 0;

            decimal newBalance = 0;

            if (newIncome > oldIncome)
            {
                incomeDifference = newIncome - oldIncome;
                newBalance = oldBalance + incomeDifference;
            }
            else
            {
                incomeDifference = oldIncome - newIncome;
                newBalance = oldBalance - incomeDifference;
            }

            #region Update Balance
            balance.Amount = newBalance;
            _appDbContext.Entry(balance).State = EntityState.Modified;
            int balanceResult = await _appDbContext.SaveChangesAsync();
            #endregion

            #region Update Income
            item.Amount = newIncome;
            _appDbContext.Entry(item).State = EntityState.Modified;
            int result = await _appDbContext.SaveChangesAsync();
            #endregion

            if (balanceResult > 0 && result > 0)
            {
                await transaction.CommitAsync();
                return StatusCode(202, "Income Updated.");
            }

            await transaction.RollbackAsync();
            return BadRequest("Updating Fail.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return InternalServerError(ex);
        }
    }

    [HttpDelete]
    [Route("/api/income/{id}")]
    public async Task<IActionResult> DeleteIncome(long id)
    {
        var transaction = await _appDbContext.Database.BeginTransactionAsync();
        try
        {
            if (id <= 0)
                return BadRequest("Id cannot be empty.");

            #region Check Income
            var income = await _appDbContext.Income
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IncomeId == id && x.IsActive);
            if (income is null)
                return NotFound("Income Not Found.");
            #endregion

            long userID = income.UserId;

            #region Check Balance
            var balance = await _appDbContext.Balance
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userID);
            if (balance is null)
                return NotFound("Balance Not Found.");
            #endregion

            decimal balanceAmount = balance.Amount;
            decimal updatedBalance = balanceAmount - income.Amount;

            #region Balance Update
            balance.Amount = updatedBalance;
            _appDbContext.Entry(balance).State = EntityState.Modified;
            int balanceResult = await _appDbContext.SaveChangesAsync();
            #endregion

            #region Delete Income
            income.IsActive = false;
            _appDbContext.Entry(income).State = EntityState.Modified;
            int result = await _appDbContext.SaveChangesAsync();
            #endregion

            if (balanceResult > 0 && result > 0)
            {
                await transaction.CommitAsync();
                return StatusCode(202, "Income Deleted.");
            }

            await transaction.RollbackAsync();
            return StatusCode(202, "Deleting Fail.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return InternalServerError(ex);
        }
    }
}
