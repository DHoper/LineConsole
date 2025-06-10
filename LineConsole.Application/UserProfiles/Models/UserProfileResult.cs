namespace LineConsole.Application.UserProfiles.Models
{
    /// <summary>建立使用者後的回傳結果</summary>
    public record class CreateUserProfileResult
    {
        /// <summary>新建立的使用者 ID</summary>
        public Guid Id { get; init; }
    }
}