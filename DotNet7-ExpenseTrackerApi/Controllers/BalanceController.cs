using DotNet7_ExpenseTrackerApi.Models.Entities;
using DotNet7_ExpenseTrackerApi.Queries;
using DotNet7_ExpenseTrackerApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace DotNet7_ExpenseTrackerApi.Controllers;
public class BalanceController : ControllerBase
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
            List<BalanceModel> lst = _adoDotNetService.Query<BalanceModel>(query, parameters.ToArray());

            return Ok(lst);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
