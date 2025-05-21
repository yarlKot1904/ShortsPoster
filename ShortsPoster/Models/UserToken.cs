using System.ComponentModel.DataAnnotations.Schema;

namespace ShortsPoster.Models
{
    [Table("usertokens")]
    public class UserToken
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("telegramuserid")]
        public long TelegramUserId { get; set; }
        [Column("refreshtoken")]
        public string RefreshToken { get; set; }
        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
