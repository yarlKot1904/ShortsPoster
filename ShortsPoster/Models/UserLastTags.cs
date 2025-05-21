using System.ComponentModel.DataAnnotations.Schema;

namespace ShortsPoster.Models
{
    [Table("userlasttags")]
    public class UserLastTags
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("telegramuserid")]
        public long TelegramUserId { get; set; }
        [Column("tagscsv")]
        public string TagsCsv { get; set; } = "";
    }
}
