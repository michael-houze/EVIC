﻿using MVC_DATABASE.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;

namespace MVC_DATABASE.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class RolesAdminController : Controller
    {
        public ApplicationDbContext db = new ApplicationDbContext();


        //public RolesAdminController()
        //{
        //}

        //public RolesAdminController(ApplicationUserManager userManager,
        //    ApplicationRoleManager roleManager)
        //{
        //    UserManager = userManager;
        //    RoleManager = roleManager;
        //}

        //private ApplicationUserManager _userManager;
        //public ApplicationUserManager UserManager
        //{
        //    get
        //    {
        //        return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        //    }
        //    set
        //    {
        //        _userManager = value;
        //    }
        //}

        //private ApplicationRoleManager _roleManager;

        //public ApplicationRoleManager RoleManager
        //{
        //    get
        //    {
        //        return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
        //    }
        //    private set
        //    {
        //        _roleManager = value;
        //    }
        //}

        //
        // GET: /Roles/
        public ActionResult Index()
        {
            var roleStore = new ApplicationRoleStore(db);
            var roleManager = new ApplicationRoleManager(roleStore);
            return View(roleManager.Roles);
        }

        //
        // GET: /Roles/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var roleStore = new ApplicationRoleStore(db);
            var roleManager = new ApplicationRoleManager(roleStore);
            var userStore = new ApplicationUserStore(db);
            var userManager = new ApplicationUserManager(userStore);

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = await roleManager.FindByIdAsync(id);
            // Get the list of Users in this Role
            var users = new List<ApplicationUser>();

            // Get the list of Users in this Role
            foreach (var user in userManager.Users.ToList())
            {
                if (await userManager.IsInRoleAsync(user.Id, role.Name))
                {
                    users.Add(user);
                }
            }
            
            ViewBag.Users = users;
            ViewBag.UserCount = users.Count();
            return View(role);
        }

        //
        // GET: /Roles/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Roles/Create
        [HttpPost]
        public async Task<ActionResult> Create(RoleViewModel roleViewModel)
        {
            if (ModelState.IsValid)
            {
                var roleStore = new ApplicationRoleStore(db);
                var roleManager = new ApplicationRoleManager(roleStore);
                var role = new ApplicationRole(roleViewModel.Name);
                var roleresult = await roleManager.CreateAsync(role);
                if (!roleresult.Succeeded)
                {
                    ModelState.AddModelError("", roleresult.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }

        //
        // GET: /Roles/Edit/Admin
        public async Task<ActionResult> Edit(string id)
        {
            var roleStore = new ApplicationRoleStore(db);
            var roleManager = new ApplicationRoleManager(roleStore);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            RoleViewModel roleModel = new RoleViewModel { Id = role.Id, Name = role.Name };
            return View(roleModel);
        }

        //
        // POST: /Roles/Edit/5
        [HttpPost]

        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Name,Id")] RoleViewModel roleModel)
        {
            if (ModelState.IsValid)
            {
                var roleStore = new ApplicationRoleStore(db);
                var roleManager = new ApplicationRoleManager(roleStore);
                var role = await roleManager.FindByIdAsync(roleModel.Id);
                role.Name = roleModel.Name;
                await roleManager.UpdateAsync(role);
                return RedirectToAction("Index");
            }
            return View();
        }

        //
        // GET: /Roles/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            var roleStore = new ApplicationRoleStore(db);
            var roleManager = new ApplicationRoleManager(roleStore);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        //
        // POST: /Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id, string deleteUser)
        {
            if (ModelState.IsValid)
            {
                var roleStore = new ApplicationRoleStore(db);
                var roleManager = new ApplicationRoleManager(roleStore);
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var role = await roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return HttpNotFound();
                }
                IdentityResult result;
                if (deleteUser != null)
                {
                    result = await roleManager.DeleteAsync(role);
                }
                else
                {
                    result = await roleManager.DeleteAsync(role);
                }
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}