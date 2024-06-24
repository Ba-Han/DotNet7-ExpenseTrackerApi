using System.Data;
using System.Data.SqlClient;
using DotNet7_ExpenseTrackerApi.Enums;
using DotNet7_ExpenseTrackerApi.Models.Entities;
using DotNet7_ExpenseTrackerApi.Models.RequestModels.User;
using DotNet7_ExpenseTrackerApi.Queries;
using DotNet7_ExpenseTrackerApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNet7_ExpenseTrackerApi.Controllers;

public class UserController : BaseController
{
    private readonly AdoDotNetService _adoDotNetService;
    private readonly IConfiguration _configuration;
    private readonly JwtService _jwtService;

    public UserController(
        AdoDotNetService service,
        IConfiguration configuration,
        JwtService jwtService
    )
    {
        _adoDotNetService = service;
        _configuration = configuration;
        _jwtService = jwtService;
    }

    [HttpPost]
    [Route("/api/account/register")]
    public IActionResult Register([FromBody] RegisterRequestModel requestModel)
    {
        SqlConnection conn = new(_configuration.GetConnectionString("DefaultConnection"));
        conn.Open();
        SqlTransaction transaction = conn.BeginTransaction();
        try
        {
            if (string.IsNullOrEmpty(requestModel.UserName))
                return BadRequest("User Name cannot be empty.");

            if (string.IsNullOrEmpty(requestModel.Email))
                return BadRequest("Email cannot be empty.");

            if (string.IsNullOrEmpty(requestModel.Password))
                return BadRequest("Password cannot be empty.");

            if (string.IsNullOrEmpty(requestModel.DOB))
                return BadRequest("DOB cannot be empty.");

            if (string.IsNullOrEmpty(requestModel.Gender))
                return BadRequest("Gender cannot be empty.");

            if (requestModel.Gender == EnumGender.Male.ToString())
                requestModel.Gender = EnumGender.Male.ToString();
            else if (requestModel.Gender == EnumGender.Female.ToString())
                requestModel.Gender = EnumGender.Female.ToString();
            else if (requestModel.Gender == EnumGender.Other.ToString())
                requestModel.Gender = EnumGender.Other.ToString();
            else
                return BadRequest("Invalid Gender");

            string duplicateQuery = UserQuery.GetDuplicateEmailQuery();
            List<SqlParameter> duplicateParams =
                new()
                {
                    new SqlParameter("@Email", requestModel.Email),
                    new SqlParameter("@IsActive", true)
                };
            DataTable user = _adoDotNetService.QueryFirstOrDefault(
                duplicateQuery,
                duplicateParams.ToArray()
            );
            if (user.Rows.Count > 0)
            {
                return Conflict("User with this email already exists. Please login.");
            }
            string query = UserQuery.CreateRegisterQuery();
            SqlCommand cmd = new(query, conn) { Transaction = transaction };
            List<SqlParameter> parameters =
                new()
                {
                    new SqlParameter("@UserName", requestModel.UserName),
                    new SqlParameter("@Email", requestModel.Email),
                    new SqlParameter("@Password", requestModel.Password),
                    new SqlParameter("@UserRole", requestModel.UserRole),
                    new SqlParameter("@DOB", requestModel.DOB),
                    new SqlParameter("@Gender", requestModel.Gender),
                    new SqlParameter("@IsActive", true)
                };
            cmd.Parameters.AddRange(parameters.ToArray());
            long userID = Convert.ToInt64(cmd.ExecuteScalar());

            int result = 0;
            if (userID != 0)
            {
                string balanceQuery = BalanceQuery.CreateBalanceQuery();
                List<SqlParameter> balanceParams =
                    new()
                    {
                        new SqlParameter("@UserId", userID),
                        new SqlParameter("@Amount", 0),
                        new SqlParameter("@CreateDate", DateTime.Now)
                    };
                SqlCommand balanceCmd = new(balanceQuery, conn) { Transaction = transaction };

                balanceCmd.Parameters.AddWithValue("@UserId", userID);
                balanceCmd.Parameters.AddWithValue("@Amount", 0);
                balanceCmd.Parameters.AddWithValue("@CreateDate", DateTime.Now);
                balanceCmd.Parameters.AddWithValue("@UpdateDate", DateTime.Now);
                //balanceCmd.Parameters.AddRange(balanceParams.ToArray());
                result = balanceCmd.ExecuteNonQuery();
            }

            if (userID == 0 || result == 0 || result < 0)
            {
                transaction.Rollback();
                return BadRequest("Registration Fail.");
            }

            transaction.Commit();
            conn.Close();
            return StatusCode(201, "Registration Successful.");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return InternalServerError(ex);
        }
    }

    [HttpPatch]
    [Route("/api/account/register/{id}")]
    public IActionResult UpdateRegister([FromBody] RegisterRequestModel requestModel, long id)
    {
        try
        {
            //checkUpdateRegisterDuplicateQuery
            if (string.IsNullOrEmpty(requestModel.UserName))
                return BadRequest("User Name cannot be empty.");

            if (string.IsNullOrEmpty(requestModel.Email))
                return BadRequest("Email cannot be empty.");

            if (string.IsNullOrEmpty(requestModel.Password))
                return BadRequest("Password cannot be empty.");

            if (string.IsNullOrEmpty(requestModel.DOB))
                return BadRequest("DOB cannot be empty.");

            if (string.IsNullOrEmpty(requestModel.Gender))
                return BadRequest("Gender cannot be empty.");

            if (requestModel.Gender == EnumGender.Male.ToString())
                requestModel.Gender = EnumGender.Male.ToString();
            else if (requestModel.Gender == EnumGender.Female.ToString())
                requestModel.Gender = EnumGender.Female.ToString();
            else if (requestModel.Gender == EnumGender.Other.ToString())
                requestModel.Gender = EnumGender.Other.ToString();
            else
                return BadRequest("Invalid Gender");

            string duplicateQuery = UserQuery.CheckUserEixstsQuery();
            List<SqlParameter> duplicateParams =
                new()
                {
                    new SqlParameter("@UserName", requestModel.UserName),
                    new SqlParameter("@Email", requestModel.Email),
                    new SqlParameter("@UserRole", requestModel.UserRole),
                    new SqlParameter("@DOB", requestModel.DOB),
                    new SqlParameter("@Gender", requestModel.Gender),
                    new SqlParameter("@IsActive", true),
                    new SqlParameter("@UserId", id)
                };
            DataTable dt = _adoDotNetService.QueryFirstOrDefault(
                duplicateQuery,
                duplicateParams.ToArray()
            );
            if (dt.Rows.Count > 0)
                return Conflict("User Name already exists.");

            //updateRegisterQuery
            string query = UserQuery.UpdateRegisterQuery();
            List<SqlParameter> parameters =
                new()
                {
                    new SqlParameter("@UserName", requestModel.UserName),
                    new SqlParameter("@Email", requestModel.Email),
                    new SqlParameter("@UserRole", requestModel.UserRole),
                    new SqlParameter("@DOB", requestModel.DOB),
                    new SqlParameter("@Gender", requestModel.Gender),
                    new SqlParameter("@IsActive", true),
                    new SqlParameter("@UserId", id)
                };
            int result = _adoDotNetService.Execute(query, parameters.ToArray());

            return result > 0
                ? StatusCode(202, "Updating Successful!")
                : BadRequest("Updating Fail!");
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }

    [HttpPost]
    [Route("/api/account/login")]
    public IActionResult Login([FromBody] LoginRequestModel requestModel)
    {
        try
        {
            if (
                requestModel is null
                || string.IsNullOrEmpty(requestModel.Email)
                || string.IsNullOrEmpty(requestModel.Password)
            )
                return BadRequest("Email or Password is empty.");

            string query = UserQuery.GetLoginQuery();
            List<SqlParameter> parameters =
                new()
                {
                    new SqlParameter("@Email", requestModel.Email),
                    new SqlParameter("@Password", requestModel.Password),
                    new SqlParameter("@IsActive", true),
                };

            List<UserModel> lst = _adoDotNetService.Query<UserModel>(query, parameters.ToArray());
            if (lst is null)
                return NotFound("User not found. Login Fail.");

            UserModel userDataModel = lst[0];

            return StatusCode(
                202,
                new { access_token = _jwtService.GenerateJWTToken(userDataModel) }
            );
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }

    [HttpGet]
    [Route("/api/account/login")]
    public IActionResult GetLoginList()
    {
        try
        {
            string query = UserQuery.GetLoginlistQuery();
            List<SqlParameter> parameters = new() { new SqlParameter("@IsActive", true) };
            List<UserModel> lst = _adoDotNetService.Query<UserModel>(query, parameters.ToArray());

            return Ok(lst);
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }
}
