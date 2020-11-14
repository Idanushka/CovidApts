using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CovidApts.Models
{
    public class User : IdentityUser
    {
        #region Properties

        public string UserId { get; set; }

        [Required]
        [MinLength(4)]
        [MaxLength(25)]
        [DisplayName("Full Name")]
        public string FullName { get; set; }
        
        public string FacebookId { get; set; }
        
        [Required]
        [MinLength(4)]
        [MaxLength(25)]
        public string Address { get; set; }

        public ICollection<Company> Companies { get; set; }

        #endregion
    }

    public enum eUserRoles
    {
        Guide,
        Janitor,
        SocialWorker,
        Admin
    }
}
