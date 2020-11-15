using System.Data;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using CovidApts.Data;
using CovidApts.Models;
using ProbabilityFunctions;
using System;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CovidApts.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly CovidAptsDbContext _context;
        private const int TRESHOLD = 5;
        private DataTable table = new DataTable();
        private DataSet dataSet = new DataSet();
        private int[] malfCount = new int[12];
        
        public StatisticsController(CovidAptsDbContext context)
        {
            _context = context;
            table.Columns.Add("Method");
            table.Columns.Add("RoomNum", typeof(double));
            table.Columns.Add("Month", typeof(double));
            table.Columns.Add("Location", typeof(double));
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Index(string address)
        {
            // ---- Bar Graph ----
            // Query with Join and Group By- using address parameter
           var qBarGraph = from Companies in _context.Company
                            join apartments in _context.Apartment on Companies.CurrentApartment equals apartments
                            where apartments.Address.Contains(address)
                            group Companies by Companies.CreationDate.Month into groupCompanies
                            select new
                            {
                                month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(groupCompanies.First().CreationDate.Month),
                                count = groupCompanies.Count()
                            };

            // Create JSON from list
            var CompaniesBarJson = JsonConvert.SerializeObject(qBarGraph.ToList());
            ViewBag.Count = CompaniesBarJson;


            // ---- Pie Graph ----
            var qPieGraph = from Company in _context.Company
                            group Company by Company.Status into groupStatus
                            select new
                            {
                                status = ((Company)groupStatus).Status.ToString(),
                                count = groupStatus.Count()
                            };

            // Create JSON from list
            var CompaniesPieJson = JsonConvert.SerializeObject(qPieGraph.ToList());

            ViewBag.Status = CompaniesPieJson;

            return View();
        }
        
        [HttpGet]
        public JsonResult Companies_in_apartment(string address)
        {
            // ---- Bar Graph ----
            // Query with Join and Group By- using address parameter
            var qBarGraph = from Companies in _context.Company
                join apartments in _context.Apartment on Companies.CurrentApartment equals apartments
                where apartments.Address.Contains(address)
                group Companies by Companies.CreationDate.Month into groupCompanies
                select new
                {
                    month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(groupCompanies.First().CreationDate.Month),
                    count = groupCompanies.Count()
                };
            return Json(JsonConvert.SerializeObject(qBarGraph.ToList()));
        }

        public JsonResult classify_extra_stuff(string address, int month)
        {
            if (month < 1 || month > 12)
                return Json($"{{\"message\":\"Month should be numerical and between 1 and 12\"}}");
            Apartment apartment = _context.Apartment.Include(m => m.Companies).FirstOrDefault(x => x.Address.Contains(address));
            InitiallizeTrainData(month);
            Classifier classifier = new Classifier();
            classifier.TrainClassifier(table);
            var name = classifier.Classify(new double[] { apartment.RoomsNumber, month, apartment.Longitude + apartment.Latitude });

            string message;

            message = name.ToLower() == "above" ? 
                "Consider putting more stuff at this area and this month, there should be a lot of Companies" : 
                "Your Stuff is quiet enough! :) No need to get extra.";

            return Json($"{{\"message\":\"{message}\"}}");
        }

        private void InitiallizeMalfPerMonth(Apartment apartment)
        {
            for (int i=0;i<12;i++)
            {
                malfCount[i] = 0;
            }
            foreach(Company malf in apartment.Companies)
            {
                malfCount[malf.CreationDate.Month - 1] ++;
            }
        }

        private void InitiallizeTrainData(int month)
        {
            var apartments = _context.Apartment.Include(m => m.Companies).ToList();
            foreach(Apartment apartment in apartments)
            {
                double location = apartment.Latitude + apartment.Longitude;
                //for (int i = 1; i <= 12; i++)
                //{
                    InitiallizeMalfPerMonth(apartment);

                    if (malfCount[month - 1] < TRESHOLD)
                    {
                        table.Rows.Add("under", apartment.RoomsNumber, GetApartmentMalfByMonth(apartment, month), location);
                    }
                    else
                    {
                        table.Rows.Add("above", apartment.RoomsNumber, GetApartmentMalfByMonth(apartment, month), location);
                    }
                    
                //}
            }
            
        }
        private int GetApartmentMalfByMonth(Apartment apartment,int month)
        {
            int count = 0;

            if (apartment.Companies != null)
            {
                foreach (Company malf in apartment.Companies)
                {
                    if (malf.CreationDate.Month == month)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}