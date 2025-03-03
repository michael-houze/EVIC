﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MVC_DATABASE.Models;
using MVC_DATABASE.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Web.Security;

namespace MVC_DATABASE.Controllers
{
    public class MESSAGEsController : Controller
    {
        private EVICEntities db = new EVICEntities();
        private MessageIndex messageindex = new MessageIndex();
        private EmployeeCreateMessage ecm = new EmployeeCreateMessage();
        private ApplicationUserManager _userManager;

        public MESSAGEsController()
        {
        }

        public MESSAGEsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private ApplicationSignInManager _signInManager;

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set { _signInManager = value; }
        }


        // GET: MESSAGEs
        [Authorize(Roles = "Administrator,Employee")]
        public ActionResult Index()
        {
            var messages = from x in db.MESSAGEs
                           where x.TO == null
                           orderby x.SENT descending
                           select x;

            messageindex.messagelist =  messages.ToList<MESSAGE>();

            return View(messageindex);
        }
        // GET: MESSAGEs/sent
        [Authorize(Roles = "Administrator,Employee")]
        public ActionResult Sent()
        {
            var messages = from x in db.MESSAGEs
                           where x.FROM == null
                           orderby x.SENT descending
                           select x;

            messageindex.messagelist = messages.ToList<MESSAGE>();

            return View(messageindex);
        }

        // GET: MESSAGEs/Details/5
        [Authorize(Roles = "Administrator,Employee")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            messageindex.message = await db.MESSAGEs.FindAsync(id);
            if (messageindex.message == null)
            {
                return HttpNotFound();
            }
            if (messageindex.message.READ == false)
            {
                messageindex.message.READ = true;
                db.Entry(messageindex.message).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            return View(messageindex);
        }

        // GET: MESSAGEs/Create
        [Authorize(Roles = "Administrator,Employee")]
        public ActionResult Create()
        {
            
            ViewBag.TO = new SelectList(db.VENDORs, "Id", "ORGANIZATION");
            return View();

        }

        // POST: MESSAGEs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Employee")]
        public async Task<ActionResult> Create( EmployeeCreateMessage model)
        {
            if (ModelState.IsValid)
            {
                MESSAGE newMessage = new MESSAGE { TO = model.TO, SUBJECT = model.SUBJECT, BODY = model.BODY, READ = false, SENT = DateTime.Now };
                db.MESSAGEs.Add(newMessage);
                string body = "Baptist Health has sent you a message. Please log into the Baptist Health Supply Chain Management system for further details.";
                string subject = "New Message from Baptist Health";                
                
                await UserManager.SendEmailAsync(model.TO, subject, body);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.TO = new SelectList(db.VENDORs, "Id", "ORGANIZATION");
            return View(model);
        }

        // GET: MESSAGEs/Reply/5
        [Authorize(Roles = "Administrator,Employee")]
        public async Task<ActionResult> Reply(int? pk, string id, string subject)
        {
            if (id == null || pk == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MESSAGE message = await db.MESSAGEs.FindAsync(pk);

            ecm.TO = id;
            ecm.SUBJECT = subject;

            ecm.vendor = new VENDOR();

            ecm.vendor = await db.VENDORs.FindAsync(id);

            if (ecm.vendor == null || message == null)
            {
                return HttpNotFound();
            }

            ecm.BODY = System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine 
                + System.Environment.NewLine + System.Environment.NewLine + "\tFrom: " + ecm.vendor.ORGANIZATION 
                + System.Environment.NewLine + "\tSent: " + message.SENT + System.Environment.NewLine 
                + "\tSubject: " + message.SUBJECT + System.Environment.NewLine + System.Environment.NewLine 
                + "\t" + message.BODY;

            return View(ecm);
        }

        // GET: MESSAGEs/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MESSAGE mESSAGE = await db.MESSAGEs.FindAsync(id);
            if (mESSAGE == null || mESSAGE.FROM == null)
            {
                return HttpNotFound();
            }
            
            return View(mESSAGE);
        }

        // POST: MESSAGEs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            MESSAGE mESSAGE = await db.MESSAGEs.FindAsync(id);
            db.MESSAGEs.Remove(mESSAGE);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        //Start Vendor
        //
        //
        //

        // GET: MESSAGEs
        [Authorize(Roles = "Vendor")]
        public ActionResult VendorInbox()
        {
            string userId = User.Identity.GetUserId();
            var messages = from x in db.MESSAGEs
                           where x.TO == userId
                           where x.READ == false
                           orderby x.SENT descending
                           select x;

            messageindex.messagelist = messages.ToList<MESSAGE>();

            return View(messageindex);
        }

        // GET: MESSAGEs/VendorDetails/5
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> VendorDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            messageindex.message = await db.MESSAGEs.FindAsync(id);
            if (messageindex.message == null || messageindex.message.READ == true)
            {
                return HttpNotFound();
            }

            string userId = User.Identity.GetUserId();
            if (messageindex.message.TO != userId)
            {
                return RedirectToAction("VendorInbox");
            }

            return View(messageindex);
        }


        // GET: MESSAGEs/Create
        [Authorize(Roles = "Vendor")]
        public ActionResult VendorCreate()
        {
            return View();
        }

        // POST: MESSAGEs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> VendorCreate(VendorCreateMessage model)
        {
            if (ModelState.IsValid)
            {
                string userId = User.Identity.GetUserId();
                MESSAGE newMessage = new MESSAGE { FROM = userId, SUBJECT = model.SUBJECT, BODY = model.BODY, READ = false, SENT = DateTime.Now };
                db.MESSAGEs.Add(newMessage);
                await db.SaveChangesAsync();
                return RedirectToAction("VendorInbox");
            }

            ViewBag.TO = new SelectList(db.VENDORs, "Id", "ORGANIZATION");
            return View(model);
        }

        VendorCreateMessage vcm = new VendorCreateMessage();

        // GET: MESSAGEs/VendorReply/5
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> VendorReply(int? pk, string subject)
        {
            if (subject == null || pk == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MESSAGE message = await db.MESSAGEs.FindAsync(pk);

            if (message == null)
            {
                return HttpNotFound();
            }


            vcm.SUBJECT = subject;

            vcm.BODY = System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine 
                + System.Environment.NewLine + System.Environment.NewLine + "\tFrom: " + "Baptist Health SCM" 
                + System.Environment.NewLine + "\tSent: " + message.SENT + System.Environment.NewLine 
                + "\tSubject: " + message.SUBJECT + System.Environment.NewLine + System.Environment.NewLine 
                + "\t" + message.BODY;


            return View(vcm);
        }


        // GET: MESSAGEs/Delete/5
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> VendorDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MESSAGE mESSAGE = await db.MESSAGEs.FindAsync(id);
            if (mESSAGE == null)
            {
                return HttpNotFound();
            }
            return View(mESSAGE);
        }

        // POST: MESSAGEs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> VendorDelete(int id)
        {
            MESSAGE mESSAGE = await db.MESSAGEs.FindAsync(id);
            mESSAGE.READ = true;
            await db.SaveChangesAsync();
            return RedirectToAction("VendorInbox");
        }
    }
}
