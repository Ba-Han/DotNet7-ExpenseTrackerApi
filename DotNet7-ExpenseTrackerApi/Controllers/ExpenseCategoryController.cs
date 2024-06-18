using DotNet7_ExpenseTrackerApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotNet7_ExpenseTrackerApi.Controllers;

public class ExpenseCategoryController : ControllerBase
{
    private readonly AdoDotNetService _adoDotNetService;

    public ExpenseCategoryController(AdoDotNetService adoDotNetService)
    {
        _adoDotNetService = adoDotNetService;
    }

    [Authorize]
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

            return Ok(lst);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
