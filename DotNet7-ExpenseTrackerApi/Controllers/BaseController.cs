﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DotNet7_ExpenseTrackerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected IActionResult Content(object obj)
    {
        return Ok(JsonConvert.SerializeObject(obj));
    }

    protected IActionResult InternalServerError(Exception ex)
    {
        return StatusCode(500, ex.Message);
    }
}
