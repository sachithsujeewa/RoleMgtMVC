using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RoleMgtMVC.Data;
using RoleMgtMVC.DTOs;
using RoleMgtMVC.Models;

namespace RoleMgtMVC.Controllers
{
    public class UsersController : Controller
    {
        private readonly RoleMgtMVCContext _context;

        public UsersController(RoleMgtMVCContext context)
        {
            _context = context;
        }

        public async Task<bool> ValidateSession(int id, Guid guid)
        {
            if (id == 0 || guid == Guid.Empty)
            {
                RedirectToAction("Index", "Home");
                return false;
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (user == null)
            {
                return false;
            }

            if (user.SessionKey.Equals(guid))
            {
                return true;
            }

            return false;
        }
        // GET: Users
        public async Task<IActionResult> Index(int userId, Guid session)
        {
            if(session == null || session == Guid.Empty || !await ValidateSession(userId,session))
            {
                return RedirectToAction("Index", "Home");
            }

            var users = await _context.User.Include(u => u.UserRole).ToListAsync();
            return View(users);
        }

        // GET: Users/Details/5
        public async Task<User> GetUser(string email)
        {
            if (email == null)
            {
                return null;
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Email == email);
            if (user == null)
            {
                return null;
            }

            return user;
        }

        // GET: Users/Create
        public async Task<IActionResult> CreateAsync()
        {
            var user = new UserViewModel();
            var roles = await _context.UserRole.ToListAsync();
            user.UserRoles = roles.Select(s => new SelectListItem()
            {
                Value = s.Id.ToString(),
                Text = s.Name
            }).ToList();
            return View(user);
        }

        // POST: Users/Create
        [HttpPost]
        public async Task<IActionResult> Create(UserViewModel userModel)
        {
            if (ModelState.IsValid)
            {
                var userRole = await _context.UserRole.FirstOrDefaultAsync(s => s.Id == int.Parse(userModel.UserRoleId));

                var hashData = GetPasswordHashValue(userModel.Password);
                var user = new User()
                {
                    Email = userModel.Email,
                    UserRole = userRole,
                    Salt = hashData.Salt,
                    PasswordHash = hashData.PasswordHash,
                    FirstName = userModel.FirstName,
                    LastName = userModel.LastName,
                    CreatedDate = DateTime.Now,
                    ExpireDate = DateTime.Now.AddMonths(3),
                };
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userModel);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
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

        public async Task<bool> UpdateUser(int id, User user)
        {
            if (id != user.Id)
            {
                return false;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return false;
                    }
                    else
                    {
                        throw;
                    }
                }
                return true;
            }
            return false;
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }

        public static HashData GetPasswordHashValue(string password, byte[] salt = null)
        {
            if(salt == null)
            {
                salt = new byte[128 / 8];

                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

            }


            var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            var hashData = new HashData()
            {
                PasswordHash = hash,
                Salt = salt
            };

            return hashData;
        }
    }
}
