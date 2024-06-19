namespace DotNet7_ExpenseTrackerApi.Queries;
public class BalanceQuery
{
    #region GetBalanceByUserId
    public static string GetBalanceByUserId()
    {
        return @"SELECT [BalanceId]
      ,[UserId]
      ,[Amount]
      ,[CreateDate]
      ,[UpdateDate]
  FROM [dbo].[Balance] WHERE UserId = @UserId";
    }
    #endregion

    #region CreateBalanceQuery
    public static string CreateBalanceQuery()
    {
        return @"INSERT INTO Balance (UserId, Amount, CreateDate, UpdateDate) VALUES (@UserId, @Amount, @CreateDate, @UpdateDate)";
    }
    #endregion

    #region UpdateBalanceQuery
    public static string UpdateBalanceQuery()
    {
        return @"UPDATE Balance SET Amount = @Amount, UpdateDate = @UpdateDate
WHERE UserId = @UserId";
    }
    #endregion

    #region GetBalanceList
    public static string GetBalanceList()
    {
        return @"SELECT [BalanceId]
      ,[UserId]
      ,[Amount]
      ,[CreateDate]
      ,[UpdateDate]
  FROM [dbo].[Balance]";
    }
    #endregion
}
