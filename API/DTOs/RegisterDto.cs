using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDto
    {
        [Required]public string UserName { get; set; }
        [Required] public string KnownAs {get;set;}
        [Required] public string Gender {get;set;}
        [Required] public DateTime DateofBirth {get;set;}
        [Required] public string city {get;set;}
        [Required] public string country {get;set;}
        [Required]
        [StringLength(8,MinimumLength =4)]
        public string Password { get; set; }
    }
}