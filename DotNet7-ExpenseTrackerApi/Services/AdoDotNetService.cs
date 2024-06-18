using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DotNet7_ExpenseTrackerApi.Services;
public class AdoDotNetService
{
    private readonly IConfiguration _configuration;

    public AdoDotNetService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    #region Query
    public List<T> Query<T>(string query, SqlParameter[]? parameters = null)
    {
        SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        conn.Open();
        SqlCommand cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddRange(parameters);
        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
        DataTable dt = new DataTable();
        adapter.Fill(dt);
        conn.Close();

        string jsonStr = JsonConvert.SerializeObject(dt);
        List<T> lst = JsonConvert.DeserializeObject<List<T>>(jsonStr)!;

        return lst;
    }
    #endregion

    #region Query With Transaction
    public List<T> Query<T>(SqlConnection conn, SqlTransaction transaction, string query, SqlParameter[]? parameters = null)
    {
        SqlCommand cmd = new SqlCommand(query, conn, transaction);
        cmd.Parameters.AddRange(parameters);
        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
        DataTable dt = new DataTable();
        adapter.Fill(dt);

        string jsonStr = JsonConvert.SerializeObject(dt);
        List<T> lst = JsonConvert.DeserializeObject<List<T>>(jsonStr)!;

        return lst;
    }
    #endregion

    #region QueryFirstOrDefault
    public DataTable QueryFirstOrDefault(string query, SqlParameter[]? parameters = null)
    {
        SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        conn.Open();
        SqlCommand cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddRange(parameters);
        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
        DataTable dt = new DataTable();
        adapter.Fill(dt);
        conn.Close();

        return dt;
    }
    #endregion

    #region QueryFirstOrDefault With Transaction
    public DataTable QueryFirstOrDefault(SqlConnection conn, SqlTransaction transaction, string query, SqlParameter[]? parameters = null)
    {
        SqlCommand cmd = new SqlCommand(query, conn, transaction);
        cmd.Parameters.AddRange(parameters);
        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
        DataTable dt = new DataTable();
        adapter.Fill(dt);

        return dt;
    }
    #endregion

    #region Excute
    public int Excute(string query, SqlParameter[]? parameters = null)
    {
        SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        conn.Open();
        SqlCommand cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddRange(parameters);
        int resExcute = cmd.ExecuteNonQuery();
        conn.Close();

        return resExcute;
    }
    #endregion

    #region Excute With Transaction
    public int Excute(SqlConnection conn, SqlTransaction transaction, string query, SqlParameter[]? parameters = null)
    {
        SqlCommand cmd = new SqlCommand(query, conn, transaction);
        cmd.Parameters.AddRange(parameters);
        int resExcute = cmd.ExecuteNonQuery();

        return resExcute;
    }
    #endregion
}
