using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CovidApts.Data;
using Microsoft.AspNetCore.Authorization;
using CovidApts.Models;
using Microsoft.AspNetCore.Identity;

namespace CovidApts.Controllers
{
    public class UsersController : Controller
    {
        private readonly CovidAptsDbContext _context;
        private readonly UserManager<User> _userManager;

        public UsersController(CovidAptsDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<IActionResult> SearchUserDetails(string Name, string Email, string Address)
        {
            //_context.User.Where(m => m.Email.Contains(Email) && m.FullName.Contains(Name) && m.Address.Contains(Address));
            var q = from users in _context.User
                    where (Name == null || users.FullName.ToLower().Contains(Name.ToLower())) &&
                          (Email == null || users.Email.ToLower().Contains(Email.ToLower())) &&
                          (Address == null || users.Address.ToLower().Contains(Address.ToLower()))
                    select users;

            return View(await q.ToListAsync());
        }

        [Authorize(Roles = "Admin,Janitor,Guide,SocialWorker")]
        public async Task<IActionResult> Index(string Name, string Email, string Address)
        {
            if (String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Email) && String.IsNullOrEmpty(Address))
            {
                return View(await _context.User.ToListAsync());
            }

            return (await SearchUserDetails(Name, Email, Address));
        }

        public async Task<IActionResult> Details(String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return RedirectToAction("NotFoundPage");
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id.ToString());

            if (user == null)
            {
                return RedirectToAction("NotFoundPage");
            }

            return View(user);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return RedirectToAction("NotFoundPage");
            }
            if (_userManager.GetUserId(HttpContext.User).Equals(id))
            {
                return Redirect("/Identity/Account/Manage");
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return RedirectToAction("NotFoundPage");
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(String id, [Bind("Id,FullName,Email,PhoneNumber,Address")] User user)
        {
            if (!id.Equals(user.Id))
            {
                return RedirectToAction("NotFoundPage");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    User dbUser = (User)_context.Users.First(u => u.Id == id);
                    dbUser.PhoneNumber = user.PhoneNumber;
                    dbUser.FullName = user.FullName;
                    dbUser.Address = user.Address;
                    _context.Update(dbUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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

            return View(user);
        }

        private bool UserExists(String id)
        {
            return _context.User.Any(e => e.Id == id);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return RedirectToAction("NotFoundPage");
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return RedirectToAction("NotFoundPage");
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(String id)
        {
            var user = await _context.User.FindAsync(id);
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult NotFoundPage()
        {
            return View();
        }
    }

}