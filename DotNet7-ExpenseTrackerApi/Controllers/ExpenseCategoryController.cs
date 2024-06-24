using System.Data;
using System.Data.SqlClient;
using DotNet7_ExpenseTrackerApi.Models.Entities;
using DotNet7_ExpenseTrackerApi.Models.RequestModels.ExpenseCategory;
using DotNet7_ExpenseTrackerApi.Queries;
using DotNet7_ExpenseTrackerApi.Resources;
using DotNet7_ExpenseTrackerApi.Services;
using Microsoft.AspNetCore.Mvc;

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
            List<SqlParameter> parameters = new() { new SqlParameter("@IsActive", true) };
            List<ExpenseCategoryModel> lst = _adoDotNetService.Query<ExpenseCategoryModel>(
                query,
                parameters.ToArray()
            );

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
                return BadRequest(ExpenseCategoryMessageResource.RequiredMessage);

            string duplicateQuery = ExpenseCategoryQuery.CheckCreateExpenseCategoryDuplicateQuery();
            List<SqlParameter> duplicateParams =
                new()
                {
                    new SqlParameter("@ExpenseCategoryName", requestModel.ExpenseCategoryName),
                    new SqlParameter("@IsActive", true)
                };
            DataTable dt = _adoDotNetService.QueryFirstOrDefault(
                duplicateQuery,
                duplicateParams.ToArray()
            );
            if (dt.Rows.Count > 0)
                return Conflict(ExpenseCategoryMessageResource.Duplicate);

            //createExpenseCategory
            string query = ExpenseCategoryQuery.CreateExpenseCategoryQuery();
            List<SqlParameter> parameters =
                new()
                {
                    new SqlParameter("@ExpenseCategoryName", requestModel.ExpenseCategoryName),
                    new SqlParameter("@IsActive", true)
                };
            int result = _adoDotNetService.Execute(query, parameters.ToArray());

            return result > 0
                ? StatusCode(201, ExpenseCategoryMessageResource.SaveSuccess)
                : BadRequest(ExpenseCategoryMessageResource.SaveFail);
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }

    [HttpPatch]
    [Route("/api/expense-category/{id}")]
    public IActionResult UpdateExpenseCategory(
        [FromBody] ExpenseCategoryRequestModel requestModel,
        long id
    )
    {
        try
        {
            //checkDuplicateUpdateExpenseCategory
            if (string.IsNullOrEmpty(requestModel.ExpenseCategoryName))
                return BadRequest(ExpenseCategoryMessageResource.RequiredMessage);

            string duplicateQuery = ExpenseCategoryQuery.CheckUpdateExpenseCategoryDuplicateQuery();
            List<SqlParameter> duplicateParams =
                new()
                {
                    new SqlParameter("@ExpenseCategoryName", requestModel.ExpenseCategoryName),
                    new SqlParameter("@IsActive", true),
                    new SqlParameter("@ExpenseCategoryId", id)
                };
            DataTable dt = _adoDotNetService.QueryFirstOrDefault(
                duplicateQuery,
                duplicateParams.ToArray()
            );
            if (dt.Rows.Count > 0)
                return Conflict(ExpenseCategoryMessageResource.Duplicate);

            //updateExpenseCategory
            string query = ExpenseCategoryQuery.UpdateExpenseCategoryQuery();
            List<SqlParameter> parameters =
                new()
                {
                    new SqlParameter("@ExpenseCategoryName", requestModel.ExpenseCategoryName),
                    new SqlParameter("@ExpenseCategoryId", id)
                };
            int result = _adoDotNetService.Execute(query, parameters.ToArray());

            return result > 0
                ? StatusCode(202, ExpenseCategoryMessageResource.UpdateSuccess)
                : BadRequest(ExpenseCategoryMessageResource.UpdateFail);
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
            List<SqlParameter> validateParams =
                new() { new SqlParameter("@ExpenseCategoryId", id) };
            DataTable dt = _adoDotNetService.QueryFirstOrDefault(
                validateQuery,
                validateParams.ToArray()
            );

            if (dt.Rows.Count > 0)
                return Conflict(ExpenseCategoryMessageResource.DeleteWarningMessage);

            //deleteExpenseCategoryQuery
            string query = ExpenseCategoryQuery.DeleteExpenseCategoryQuery();
            List<SqlParameter> parameters =
                new()
                {
                    new SqlParameter("@ExpenseCategoryId", id),
                    new SqlParameter("@IsActive", false)
                };
            int result = _adoDotNetService.Execute(query, parameters.ToArray());

            return result > 0
                ? StatusCode(202, ExpenseCategoryMessageResource.DeleteSuccess)
                : BadRequest(ExpenseCategoryMessageResource.DeleteFail);
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }
}
