using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CovidApts.Models
{
    public class Apartment
    {
        #region Properties

        public int ApartmentId { get; set; }

        [Required]
        [MinLength(4)]
        [MaxLength(50)]
        public string Address { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public string Photo { get; set; }

        [Range(1, 30)]
        [DisplayName("Rooms Number")]
        public int RoomsNumber { get; set; }

        public ICollection<Company> Companies { get; set; }

        #endregion
    }
}
