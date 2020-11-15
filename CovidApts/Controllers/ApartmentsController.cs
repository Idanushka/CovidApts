using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GoogleMaps.LocationServices;
using GuigleAPI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CovidApts.Data;
using CovidApts.Models;
using System.Net;
using ProtoBuf;
using transit_realtime;
using Geocoding.Microsoft;
using Geocoding;

namespace CovidApts.Controllers
{
    public class ApartmentsController : Controller
    {
        private readonly CovidAptsDbContext _context;
        private readonly IHostingEnvironment he;


        public ApartmentsController(CovidAptsDbContext context, IHostingEnvironment e)
        {
            _context = context;
            he = e;
        }

        // GET: Apartments
        public async Task<IActionResult> Index(String searchString)
        {
            if (String.IsNullOrEmpty(searchString))
            {
                return View(await _context.Apartment.ToListAsync());
            }

            return View(await _context.Apartment.Where(a => a.Address.Contains(searchString)).ToListAsync());
        }

        // GET: Apartments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFoundPage");
            }

            var apartment = await _context.Apartment.Include(x=> x.Companies)
                .FirstOrDefaultAsync(m => m.ApartmentId == id);

            if (apartment == null)
            {
                return RedirectToAction("NotFoundPage");
            }

            return View(apartment);
        }

        // GET: Apartments/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Apartments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ApartmentId,Address,Photo,RoomsNumber")] Apartment apartment, IFormFile files)
        {
            if (ModelState.IsValid)
            {
                // Getting the long lat
                var location = AddLongLatAsync(apartment.Address);
                apartment.Latitude = location.Result.Latitude;
                apartment.Longitude = location.Result.Longitude;
                SavePhoto(apartment, files);
                _context.Add(apartment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(apartment);
        }

        private async Task<MapPoint> AddLongLatAsync(string apartmentAddress)
        {
            MapPoint point;

            // Getting the location of the address
            try
            {
                IGeocoder geocoder = new BingMapsGeocoder("AgnX4fQVaxUmGPZMrIAWH2sVbJ34rY2GR-qjkOWUCJprHGbHX5iMfjlq-xOIYelB");
                IEnumerable <Address> addresses = await geocoder.GeocodeAsync(apartmentAddress);

                point = new MapPoint
                {
                    Latitude = addresses.First().Coordinates.Latitude,
                    Longitude = addresses.First().Coordinates.Longitude
                };


            }
            catch (Exception ex)
            {
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine(ex);
                Console.WriteLine("----------------------------------------------");
                point = new MapPoint
                {
                    Latitude = 32.1637206,
                    Longitude = 34.8647352
                };
            }

            return point;
        }

        private void SavePhoto(Apartment apartment, IFormFile files)
        {
            // Sanity check apartment cant be null
            if (apartment == null)
            {
                return;
            }

            // If there is no defined photo, set the default photo
            if (files == null)
            {
                apartment.Photo = "/images/apartments/default_photo.jpg";
                return;
            }

            var fileName = Path.Combine(he.WebRootPath + "/images/apartments", Path.GetFileName(files.FileName));
            apartment.Photo = "/images/apartments/" + files.FileName;

            // If the file does not exist already creating it
            if (!System.IO.File.Exists(fileName))
            {
                files.CopyTo(new FileStream(fileName, FileMode.Create));
            }
        }

        // GET: Apartments/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFoundPage");
            }

            var apartment = await _context.Apartment.FindAsync(id);
            if (apartment == null)
            {
                return RedirectToAction("NotFoundPage");
            }
            return View(apartment);
        }

        // POST: Apartments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ApartmentId,Address,Photo,RoomsNumber")] Apartment apartment, IFormFile files)
        {
            if (id != apartment.ApartmentId)
            {
                return RedirectToAction("NotFoundPage");
            }

            if (ModelState.IsValid)
            {
                if (files != null)
                {
                    SavePhoto(apartment, files);
                }

                try
                {
                    _context.Update(apartment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApartmentExists(apartment.ApartmentId))
                    {
                        return RedirectToAction("NotFoundPage");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(apartment);
        }

        // GET: Apartments/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFoundPage");
            }

            var apartment = await _context.Apartment
                .FirstOrDefaultAsync(m => m.ApartmentId == id);
            if (apartment == null)
            {
                return RedirectToAction("NotFoundPage");
            }

            return View(apartment);
        }

        // POST: Apartments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var apartment = await _context.Apartment.FindAsync(id);
            _context.Apartment.Remove(apartment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApartmentExists(int id)
        {
            return _context.Apartment.Any(e => e.ApartmentId == id);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult NotFoundPage()
        {
            return View();
        }
    }
}
