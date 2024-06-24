using DotNet7_ExpenseTrackerApi.Services;
using DotNet7_ExpenseTrackerApi.Queries;
using DotNet7_ExpenseTrackerApi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using DotNet7_ExpenseTrackerApi.Models.RequestModels.ExpenseCategory;
using System.Data;

namespace DotNet7_ExpenseTrackerApi.Controllers;

public class ExpenseCategoryController : BaseController
{
    private readonly AdoDotNetService _adoDotNetService;

    public ExpenseCategoryController(AdoDotNetService adoDotNetService)
    {
        _adoDotNetService = adoDotNetService;
    }

    [HttpGet]
    [Route("/api/expense-category")]
    public IActionResult GetList()
    {
        try
        {
            string query = ExpenseCategoryQuery.GetExpenseCategoryListQuery();
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@IsActive", true)
            };
            List<ExpenseCategoryModel> lst = _adoDotNetService.Query<ExpenseCategoryModel>(query, parameters.ToArray());

            return Content(lst);
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }

    [HttpPost]
    [Route("/api/expense-category")]
    public IActionResult CreateExpenseCategory([FromBody] ExpenseCategoryRequestModel requestModel)
    {
        try
        {
            //checkDuplicateCreateExpenseCategory
            if (string.IsNullOrEmpty(requestModel.ExpenseCategoryName))
                return BadRequest("Category name cannot be empty.");

            string duplicateQuery = ExpenseCategoryQuery.CheckCreateExpenseCategoryDuplicateQuery();
            List<SqlParameter> duplicateParams = new()
            {
                new SqlParameter("@ExpenseCategoryName", requestModel.ExpenseCategoryName),
                new SqlParameter("@IsActive", true)
            };
            DataTable dt = _adoDotNetService.QueryFirstOrDefault(duplicateQuery, duplicateParams.ToArray());
            if (dt.Rows.Count > 0)
                return Conflict("Expense Category Name already exists!");

            //createExpenseCategory
            string query = ExpenseCategoryQuery.CreateExpenseCategoryQuery();
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@ExpenseCategoryName", requestModel.ExpenseCategoryName),
                new SqlParameter("@IsActive", true)
            };
            int result = _adoDotNetService.Execute(query, parameters.ToArray());

            return result > 0 ? StatusCode(201, "Creating Successful!") : BadRequest("Creating Fail!");
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }

    [HttpPatch]
    [Route("/api/expense-category/{id}")]
    public IActionResult UpdateExpenseCategory([FromBody] ExpenseCategoryRequestModel requestModel, long id)
    {
        try
        {
            //checkDuplicateUpdateExpenseCategory
            if (string.IsNullOrEmpty(requestModel.ExpenseCategoryName))
                return BadRequest("Category name cannot be empty.");

            string duplicateQuery = ExpenseCategoryQuery.CheckUpdateExpenseCategoryDuplicateQuery();
            List<SqlParameter> duplicateParams = new()
            {
                new SqlParameter("@ExpenseCategoryName", requestModel.ExpenseCategoryName),
                new SqlParameter("@IsActive", true),
                new SqlParameter("@ExpenseCategoryId", id)
            };
            DataTable dt = _adoDotNetService.QueryFirstOrDefault(duplicateQuery, duplicateParams.ToArray());
            if (dt.Rows.Count > 0)
                return Conflict("Expense Category Name already exists.");

            //updateExpenseCategory
            string query = ExpenseCategoryQuery.UpdateExpenseCategoryQuery();
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@ExpenseCategoryName", requestModel.ExpenseCategoryName),
                new SqlParameter("@ExpenseCategoryId", id)
            };
            int result = _adoDotNetService.Execute(query, parameters.ToArray());

            return result > 0 ? StatusCode(202, "Updating Successful!") : BadRequest("Updating Fail!");
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }

    [HttpDelete]
    [Route("/api/expense-category/{id}")]
    public IActionResult DeleteExpenseCategory(long id)
    {
        try
        {
            //checkCanNotDeleteExpenseCategory
            string validateQuery = ExpenseCategoryQuery.CheckExpenseCategoryQuery();
            List<SqlParameter> validateParams = new()
            {
                new SqlParameter("@ExpenseCategoryId", id)
            };
            DataTable dt = _adoDotNetService.QueryFirstOrDefault(validateQuery, validateParams.ToArray());

            if (dt.Rows.Count > 0)
                return Conflict("Expense with this category already exists! Cannot delete.");

            //deleteExpenseCategoryQuery
            string query = ExpenseCategoryQuery.DeleteExpenseCategoryQuery();
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@ExpenseCategoryId", id),
                new SqlParameter("@IsActive", false)
            };
            int result = _adoDotNetService.Execute(query, parameters.ToArray());

            return result > 0 ? StatusCode(202, "Deleting Successful!") : BadRequest("Deleting Fail!");
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }
}
