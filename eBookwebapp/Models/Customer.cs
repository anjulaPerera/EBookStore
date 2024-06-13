using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Permissions;

namespace eBookwebapp.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        public String Name { get; set; }
        public String Address { get; set; }
        public String PhoneNumber { get; set; }
        public DateOnly BirthDay { get; set; }

        [Required]
        [ForeignKey("User")]
        
        public string UserId { get; set; }

    }
}
