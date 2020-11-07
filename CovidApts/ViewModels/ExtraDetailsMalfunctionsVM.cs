using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CovidApts.Models;

namespace CovidApts.ViewModels
{
    public class ExtraDetailsCompaniesVM
    {
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Content")]
        public string Content { get; set; }

        [Display(Name = "Creation date")]
        public DateTime CreationDate { get; set; }

        [Display(Name = "Modified date")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Appartment address")]
        public String AppartmentAddress { get; set; }

        [Display(Name = "User name")]
        public String UserName { get; set; }
    }
}
