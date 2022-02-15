using Data.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Data.Models
{
   public class BlackList
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public BlackListWhiteListType Type { get; set; }
        public string Value { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
