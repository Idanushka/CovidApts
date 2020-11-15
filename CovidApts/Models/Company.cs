using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CovidApts.Models
{
    public class Company
    {
        #region Properties

        public string CompanyId { get; set; }

        public Status Status { get; set; }

        [Required]
        [MinLength(4)]
        [MaxLength(25)]
        public string Title { get; set; }

        [Required]
        [MinLength(4)]
        [MaxLength(200)]
        public string Content { get; set; }

        [DisplayName("Photo")]
        public string Resources { get; set; }

        [DisplayName("Creation Date")]
        [DataType(DataType.Date)]
        public DateTime CreationDate { get; set; }

        [DisplayName("Modification Date")]
        [DataType(DataType.Date)]
        public DateTime ModifiedDate { get; set; }

        [DisplayName("Apartment")]
        public Apartment CurrentApartment { get; set; }

        #endregion
    }
}
