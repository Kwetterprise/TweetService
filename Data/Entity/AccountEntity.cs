using System;
using System.Collections.Generic;
using System.Text;

namespace Kwetterprise.TweetService.Data.Entity
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Kwetterprise.EventSourcing.Client.Models.DataTransfer;

    public class AccountEntity
    {
        public AccountEntity()
        {
            
        }

        public AccountEntity(Guid id, string username, AccountRole role, byte[]? profilePicture)
        {
            this.Id = id;
            this.Username = username;
            this.Role = role;
            this.ProfilePicture = profilePicture;
        }

        [Key]
        [Column(Order = 0)]
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Column(Order = 1)]
        public string Username { get; set; } = null!;

        [Column(Order = 2)]
        public AccountRole Role { get; set; }

        [Column(Order = 3)]
        public byte[]? ProfilePicture { get; set; }
    }
}
