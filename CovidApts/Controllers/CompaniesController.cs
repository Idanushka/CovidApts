using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using CovidApts.Data;
using CovidApts.Models;
using CovidApts.Services;
using CovidApts.ViewModels;

namespace CovidApts.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly CovidAptsDbContext _context;
        private readonly IHostingEnvironment he;

        public CompaniesController(CovidAptsDbContext context, IHostingEnvironment e)
        {
            _context = context;
            he = e;
        }

        // POST: Get Companies parameters from the user and search in server
        //       it shows the Companies details and also user name, appartment password
        public async Task<IActionResult> ShowMalfExtraDetails(DateTime createDate, Status status,
                                                              string address, string userName)
        {
            if ((createDate == null) && (String.IsNullOrEmpty(status.ToString())) &&
                  String.IsNullOrEmpty(address) && String.IsNullOrEmpty(userName))
            {
                return View(await _context.Company.ToListAsync());
            }

            return (await SearchMalfExtraDetails(createDate, status, address, userName));
        }

        private async Task<IActionResult> SearchMalfExtraDetails(DateTime createDate, Status status,
                                                              string address, string userName)
        {
            Status enumStatus = (Status)status;

            if (address == null)
                address = "";

            var q = from Company in _context.Company
                    join apartments in _context.Apartment on Company.CurrentApartment equals apartments
                    where Company.CreationDate >= createDate &&
                          Company.Status.Equals(enumStatus) &&
                          apartments.Address.Contains(address)
                    select new ExtraDetailsCompaniesVM()
                    {
                        Title = Company.Title,
                        Status = Company.Status.ToString(),
                        Content = Company.Content,
                        CreationDate = Company.CreationDate,
                        ModifiedDate = Company.ModifiedDate,
                        AppartmentAddress = apartments.Address,
                    };

             return View(await q.ToListAsync());
    }

        // GET: Companies
        [Authorize(Roles = "Admin,Janitor,Guide,SocialWorker")]
        public async Task<IActionResult> Index(String searchString)
        {
            var databaseContext = _context.Company.Include(p => p.CurrentApartment);

            if (String.IsNullOrEmpty(searchString))
            {
                return View(await databaseContext.ToListAsync());
            }

            return View(await databaseContext.Where(m => m.Content.Contains(searchString)).ToListAsync());
        }

        // GET: Companies/Details/5
        [Authorize(Roles = "Admin,Janitor,Guide,SocialWorker")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFoundPage");
            }

            var Company = await _context.Company
                .FirstOrDefaultAsync(m => m.CompanyId == id);
            if (Company == null)
            {
                return RedirectToAction("NotFoundPage");
            }

            return View(Company);
        }

        // GET: Companies/Create
        [Authorize(Roles = "Admin,Janitor,Guide,SocialWorker")]
        public async Task<IActionResult> Create()
        {
            var apartments = from apt in _context.Apartment.Include(s => s.Companies)
                             select new { Value = apt.ApartmentId, Text = apt.Address };

            var users = from usr in _context.User.Include(s => s.Companies)
                        select new { Value = usr.Id, Text = usr.Email };

            var statuses = from Status stat in Enum.GetValues(typeof(Status))
                           select new { Value = (int)stat, Text = stat.ToString() };

            ViewData["all_appartments"] = new SelectList(await apartments.ToListAsync(), "Value", "Text");
            ViewData["all_users"] = new SelectList(await users.ToListAsync(), "Value", "Text");
            ViewData["statuses"] = new SelectList(statuses, "Value", "Text");

            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Janitor,Guide,SocialWorker")]
        public async Task<IActionResult> Create([Bind("CurrentApartment")]int CurrentApartment, [Bind("CompanyId,Status,Title,Content,Resources,CurrentApartmentId")] Company Company, IFormFile mFiles)
        {            
            if (ModelState.IsValid)
            {
                // Creating the query of the apartment
                var queryApt = from apt in _context.Apartment
                               where apt.ApartmentId == CurrentApartment
                               select apt;

                // If the id of the apartment/user does not exist in DB
                if (!queryApt.Any() || !queryApt.Any())
                {
                    return View(Company);
                }

                // Adding the apartment to the Company to save
                var curApartment = queryApt.First();
                Company.CurrentApartment = curApartment;
                
                // Adding the creation date and modification date
                Company.CreationDate = DateTime.Now;
                Company.ModifiedDate = DateTime.Now;

                // Uploading photo describing the Company
                SavePhoto(Company, mFiles);

                _context.Add(Company);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(Company);
        }

        private void SavePhoto(Company Company, IFormFile mFiles)
        {
            // Sanity check apartment cant be null
            if (Company == null)
            {
                return;
            }

            // If there is no defined photo, set the default photo
            if (mFiles == null)
            {
                Company.Resources = "/images/Companies/default_Company_photo.jpg";
                return;
            }

            var fileName = Path.Combine(he.WebRootPath + "/images/Companies", Path.GetFileName(mFiles.FileName));
            Company.Resources = "/images/Companies/" + mFiles.FileName;

            // If the file does not exist already creating it
            if (!System.IO.File.Exists(fileName))
            {
                mFiles.CopyTo(new FileStream(fileName, FileMode.Create));
            }
        }

        // GET: Companies/Edit/5
        [Authorize(Roles = "Admin,Janitor,Guide,SocialWorker")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFoundPage");
            }

            var Company = await _context.Company.FindAsync(id);
            if (Company == null)
            {
                return RedirectToAction("NotFoundPage");
            }

            // Adding all the statuses to the viewData
            var statuses = from Status stat in Enum.GetValues(typeof(Status))
                select new { Value = (int)stat, Text = stat.ToString() };
            ViewData["statuses"] = new SelectList(statuses, "Value", "Text");

            return View(Company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Janitor,Guide,SocialWorker")]
        public async Task<IActionResult> Edit(string id, [Bind("CompanyId,Status,Title,Content,Resources")] Company Company, IFormFile files)
        {
            if (id != Company.CompanyId)
            {
                return RedirectToAction("NotFoundPage");
            }

            if (ModelState.IsValid)
            {
                if (files != null)
                {
                    SavePhoto(Company, files);
                }

                if (!CompanyExists(Company.CompanyId))
                    return RedirectToAction("NotFoundPage");

                // Getting the Company from db
                var CompanyToSave = _context.Company.First(mal => mal.CompanyId == id);

                // Setting the new values
                CompanyToSave.ModifiedDate = DateTime.Now;
                CompanyToSave.Status = Company.Status;
                CompanyToSave.Title = Company.Title;
                CompanyToSave.Content = Company.Content;
                CompanyToSave.Resources = Company.Resources;

                // Updating the new Company
                _context.Update(CompanyToSave);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(Company);
        }

        // GET: Companies/Delete/5
        [Authorize(Roles = "Admin,Janitor,Guide,SocialWorker")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFoundPage");
            }

            var Company = await _context.Company
                .FirstOrDefaultAsync(m => m.CompanyId == id);
            if (Company == null)
            {
                return RedirectToAction("NotFoundPage");
            }

            return View(Company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var Company = await _context.Company.FindAsync(id);
            _context.Company.Remove(Company);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(string id)
        {
            return _context.Company.Any(e => e.CompanyId == id);
        }

        public IActionResult NotFoundPage()
        {
            return View();
        }
    }
}
