using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RoleMgtMVC.Data;
using RoleMgtMVC.DTOs;
using RoleMgtMVC.Models;

namespace RoleMgtMVC.Controllers
{
    public class UsersController : Controller
    {
        private readonly RoleMgtMVCContext _context;
        public IConfiguration Configuration { get; }

        public UsersController(RoleMgtMVCContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
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
        public async Task<IActionResult> Index()
        {
            var session = Guid.Empty;
            var userId = 0;
            if(HttpContext.Session.GetString("SessionKey") != "")
            {
                session = Guid.Parse(HttpContext.Session.GetString("SessionKey"));
                userId = int.Parse(HttpContext.Session.GetString("UserId"));
            }

            if (session == null || session == Guid.Empty || !await ValidateSession(userId, session))
            {
                return RedirectToAction("Index", "Home");
            }

            var loggedUser = await GetUserBySession(session);

            if (loggedUser == null)
            {
                return RedirectToAction("Index", "Home");
            }
            HttpContext.Session.SetString("SessionKey", loggedUser.SessionKey.ToString());
            HttpContext.Session.SetString("UserId", loggedUser.Id.ToString());

            var users = await _context.User.Include(u => u.UserRole).ToListAsync();


            users = users.OrderByDescending(s => s.UserRoleId).ToList();

            if (loggedUser.UserRoleId < 4)
            {
                users = users.Where(u => u.UserRoleId < loggedUser.UserRoleId).ToList();
            }

            var userVM = new UserViewModel()
            {
                Id = loggedUser.Id,
                Email = loggedUser.Email,
                VisibleUsers = users,
                UserRoleId = loggedUser.UserRoleId.ToString(),
                FirstName = loggedUser.FirstName,
                LastName = loggedUser.LastName,
                UserRole = loggedUser.UserRole.Name
            };

            return View(userVM);
        }


        public async Task<User> GetUserByEmail(string email)
        {
            // validate parameter is filled
            if (email == null || email.Contains("\'") || email.Contains(";"))
            {
                return null;
            }

            // validate the email
            SqlDataReader rdr = null;
            SqlConnection con = new SqlConnection(Configuration.GetConnectionString("RoleMgtMVCContext"));
            object t1, t2 = null;

            try
            {
                string sql = "select [Email],[SessionKey]  from [User] where [Email]= @EmailID";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add(new SqlParameter("@EmailID", email));
                con.Open();
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    t1 = rdr[0];
                    t2 = rdr[1];
                }
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }

                if (con != null)
                {
                    con.Close();
                }
            }



            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Email == email);
            if (user == null)
            {
                return null;
            }

            return user;
        }


        public async Task<User> GetUserBySession(Guid sessionKey)
        {
            // validate parameter is filled
            if (sessionKey == null || sessionKey == Guid.Empty)
            {
                return null;
            }

            // validate the session key
            var user = await _context.User.Include(u => u.UserRole).FirstOrDefaultAsync(m => m.SessionKey.Equals(sessionKey));
            if (user == null)
            {
                return null;
            }

            return user;
        }

        public async Task<User> GetUserById(int id)
        {
            // validate parameter is filled
            if (id == 0)
            {
                return null;
            }

            // validate the email
            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var loggedUser = await GetUserBySession(Guid.Parse(HttpContext.Session.GetString("SessionKey")));
            var roles = await _context.UserRole.Where(r => r.Level < loggedUser.UserRole.Level).ToListAsync();
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
                };

                if (user.UserRoleId == 1)
                {
                    user.ExpireDate = DateTime.Now.AddMonths(3);
                }
                else
                {
                    user.ExpireDate = DateTime.Now.AddYears(1);
                    user.IsActive = true;
                }
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

            var user = await _context.User.Include(u => u.UserRole).FirstOrDefaultAsync(t => t.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var userVM = new UserViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                UserRoleId = user.UserRoleId.ToString(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserRole = user.UserRole.Name,
                IsActive = user.IsActive
            };
            var loggedUser = await GetUserBySession(Guid.Parse(HttpContext.Session.GetString("SessionKey")));
            var roles = await _context.UserRole.Where(r => r.Level < loggedUser.UserRole.Level).ToListAsync();
            userVM.UserRoles = roles.Select(s => new SelectListItem()
            {
                Value = s.Id.ToString(),
                Text = s.Name
            }).ToList();
            return View(userVM);
        }

        // POST: Users/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id,UserViewModel userVM)
        {
            if (id != userVM.Id)
            {
                return NotFound();
            }

            User user = null;
            if (ModelState.IsValid)
            {
                try
                {
                    user = await _context.User.Include(u => u.UserRole).FirstOrDefaultAsync(t => t.Id == id);

                    user.CreatedDate = DateTime.Now;
                    user.Email = userVM.Email;
                    user.UserRoleId = int.Parse(userVM.UserRoleId);
                    user.FirstName = userVM.FirstName;
                    user.LastName = userVM.LastName;
                    user.IsActive = userVM.IsActive;
                    
                    if(user.UserRoleId == 1)
                    {
                        user.ExpireDate = DateTime.Now.AddMonths(3);
                    }
                    else
                    {
                        user.ExpireDate = DateTime.Now.AddYears(1);
                    }
                    

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

            var user = await _context.User.Include(u => u.UserRole).FirstOrDefaultAsync(t => t.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var userVM = new UserViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                UserRoleId = user.UserRoleId.ToString(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserRole = user.UserRole.Name,
                IsActive = user.IsActive
            };
            var roles = await _context.UserRole.ToListAsync();
            userVM.UserRoles = roles.Select(s => new SelectListItem()
            {
                Value = s.Id.ToString(),
                Text = s.Name
            }).ToList();
            return View(userVM);
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
            if (salt == null)
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
