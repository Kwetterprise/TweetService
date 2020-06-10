using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Entity
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class TweetEntity
    {
        public const int MaxLength = 280;

        public TweetEntity()
        {
            
        }

        public TweetEntity(Guid id, Guid author, string content, DateTime postedOn)
        {
            this.Id = id;
            this.Author = author;
            this.Content = content;
            this.PostedOn = postedOn;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 0)]
        public Guid Id { get; set; }

        [Column(Order = 1)]
        public Guid Author { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(TweetEntity.MaxLength)]
        [Column(Order = 2)]
        public string Content { get; set; } = null!;

        [Required]
        [Column(Order = 3)]
        public DateTime PostedOn { get; set; }
    }
}
