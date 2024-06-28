using DotNet7_ExpenseTrackerApi.Models.Entities;
using DotNet7_ExpenseTrackerApi.Models.RequestModels.IncomeCategory;
using DotNet7_ExpenseTrackerApi.Queries;
using DotNet7_ExpenseTrackerApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace DotNet7_ExpenseTrackerApi.Controllers;
public class IncomeCategoryController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly AdoDotNetService _adoDotNetService;

    public IncomeCategoryController (
        IConfiguration configuration,
        AdoDotNetService adoDotNetService
    )
    {
        _configuration = configuration;
        _adoDotNetService = adoDotNetService;
    }

    [HttpGet]
    [Route("/api/income-category")]
    public IActionResult GetIncomeCategory()
    {
        try
        {
            string query = IncomeCategoryQuery.GetIncomeCategoryListQuery();
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@IsActive", true)
            };
            List<IncomeCategoryModel> lst = _adoDotNetService.Query<IncomeCategoryModel>(query, parameters.ToArray());

            return Ok(lst);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpPost]
    [Route("/api/income-category")]
    public IActionResult CreateIncomeCategory([FromBody] IncomeCategoryRequestModel requestModel)
    {
        try
        {
            //checkCreateIncomeCategoryDuplicateQuery
            if (string.IsNullOrEmpty(requestModel.IncomeCategoryName))
                return BadRequest("Category Name cannot be empty.");

            string duplicateQuery = IncomeCategoryQuery.CheckCreateIncomeCategoryDuplicateQuery();
            List<SqlParameter> duplicateParams = new()
            {
                new SqlParameter("@IncomeCategoryName", requestModel.IncomeCategoryName),
                new SqlParameter("@IsActive", true)
            };
            DataTable category = _adoDotNetService.QueryFirstOrDefault(duplicateQuery, duplicateParams.ToArray());
            if (category.Rows.Count > 0)
                return Conflict("Income Category Name already exists.");

            //createIncomeCategoryQuery
            string query = IncomeCategoryQuery.CreateIncomeCategoryQuery();
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@IncomeCategoryName", requestModel.IncomeCategoryName),
                new SqlParameter("@IsActive", true)
            };
            int result = _adoDotNetService.Execute(query, parameters.ToArray());

            return result > 0 ? StatusCode(201, "Income Category Created.") : BadRequest("Creating Fail.");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpPatch]
    [Route("/api/income-category/{id}")]
    public IActionResult UpdateIncomeCategory([FromBody] IncomeCategoryRequestModel requestModel, long id)
    {
        try
        {
            //checkIncomeCategoryDuplicateQuery
            if (string.IsNullOrEmpty(requestModel.IncomeCategoryName))
                return BadRequest("Category name cannot be empty.");

            string duplicateQuery = IncomeCategoryQuery.CheckIncomeCategoryDuplicateQuery();
            List<SqlParameter> duplicateParams = new()
            {
                new SqlParameter("@IncomeCategoryName", requestModel.IncomeCategoryName),
                new SqlParameter("@IsActive", true),
                new SqlParameter("@IncomeCategoryId", id)
            };
            DataTable dt = _adoDotNetService.QueryFirstOrDefault(duplicateQuery, duplicateParams.ToArray());
            if (dt.Rows.Count > 0)
                return Conflict("Income Category Name already exists.");

            //updateIncomeCategoryQuery
            string query = IncomeCategoryQuery.UpdateIncomeCategoryQuery();
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@IncomeCategoryName", requestModel.IncomeCategoryName),
                new SqlParameter("@IncomeCategoryId", id)
            };
            int result = _adoDotNetService.Execute(query, parameters.ToArray());

            return result > 0 ? StatusCode(202, "Updating Successful!") : BadRequest("Updating Fail!");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [HttpDelete]
    [Route("/api/income-category/{id}")]
    public IActionResult DeleteIncomeCategory(long id)
    {
        try
        {
            //checkGetCheckIncomeExistsQuery
            if (id == 0)
                return BadRequest();

            string validateQuery = IncomeQuery.GetCheckIncomeExistsQuery();
            SqlParameter[] validateParams = { new("@IncomeCategoryId", id) };
            DataTable dt = _adoDotNetService.QueryFirstOrDefault(validateQuery, validateParams);

            if (dt.Rows.Count > 0)
                return Conflict("Income with this category already exists! Cannot delete.");

            //deleteIncomeCategoryQuery
            string query = IncomeCategoryQuery.DeleteIncomeCategoryQuery();
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@IsActive", false),
                new SqlParameter("@IncomeCategoryId", id)
            };
            int result = _adoDotNetService.Execute(query, parameters.ToArray());

            return result > 0 ? StatusCode(202, "Deleting Successful!") : BadRequest("Deleting Fail!");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
