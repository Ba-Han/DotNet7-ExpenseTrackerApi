namespace DotNet7_ExpenseTrackerApi.Queries;
public class UserQuery
{
    #region CreateRegisterQuery
    public static string CreateRegisterQuery()
    {
        return @"INSERT INTO Users (UserName, Email, Password, UserRole, DOB, Gender, IsActive)
        VALUES (@UserName, @Email, @Password, @UserRole, @DOB, @Gender, @IsActive);
        SELECT SCOPE_IDENTITY();";
    }
    #endregion

    #region GetLoginQuery
    public static string GetLoginQuery()
    {
        return @"SELECT [UserId]
      ,[UserName]
      ,[Email]
      ,[UserRole]
      ,[DOB]
      ,[Gender]
      ,[IsActive]
  FROM [dbo].[Users] WHERE Email = @Email AND Password = @Password AND IsActive = @IsActive";
    }
    #endregion

    #region GetLoginQuery
    public static string GetLoginlistQuery()
    {
        return @"SELECT [UserId]
      ,[UserName]
      ,[Email]
      ,[UserRole]
      ,[DOB]
      ,[Gender]
      ,[IsActive]
  FROM [dbo].[Users] WHERE IsActive = @IsActive ORDER BY UserId DESC";
    }
    #endregion

    #region GetDuplicateEmailQuery
    public static string GetDuplicateEmailQuery()
    {
        return @"SELECT [UserId]
      ,[UserName]
      ,[Email]
      ,[UserRole]
      ,[DOB]
      ,[Gender]
      ,[IsActive]
  FROM [dbo].[Users] WHERE Email = @Email AND IsActive = @IsActive";
    }
    #endregion

    #region CheckUserEixstsQuery
    public static string CheckUserEixstsQuery()
    {
        return @"SELECT [UserId]
      ,[UserName]
      ,[Email]
      ,[UserRole]
      ,[DOB]
      ,[Gender]
      ,[IsActive]
  FROM [dbo].[Users] WHERE UserId != @UserId AND UserName =@UserName AND IsActive = @IsActive";
    }
    #endregion

    #region UpdateExpenseQuery
    public static string UpdateRegisterQuery()
    {
        return @"UPDATE Users SET
        UserName = @UserName, 
        Email = @Email,
        UserRole = @UserRole,
        DOB = @DOB,
        Gender = @Gender
        WHERE UserId = @UserId";
    }
    #endregion
}
