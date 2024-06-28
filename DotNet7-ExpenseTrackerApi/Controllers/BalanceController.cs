using System.Data.SqlClient;
using DotNet7_ExpenseTrackerApi.Models.Entities;
using DotNet7_ExpenseTrackerApi.Queries;
using DotNet7_ExpenseTrackerApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNet7_ExpenseTrackerApi.Controllers;

public class BalanceController : BaseController
{
    private readonly IConfiguration _configuration;
    private readonly AdoDotNetService _adoDotNetService;

    public BalanceController(IConfiguration configuration, AdoDotNetService adoDotNetService)
    {
        _configuration = configuration;
        _adoDotNetService = adoDotNetService;
    }

    [HttpGet]
    [Route("/api/balance")]
    public IActionResult GetList()
    {
        try
        {
            string query = BalanceQuery.GetBalanceList();
            List<SqlParameter> parameters = new();
            List<BalanceModel> lst = _adoDotNetService.Query<BalanceModel>(
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
}
