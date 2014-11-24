﻿using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System;
using System.IO;
using System.Globalization;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MVC_DATABASE.Models;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Web.Security;
using Microsoft.AspNet.Identity.EntityFramework;
using MVC_DATABASE.Models.ViewModels;

namespace MVC_DATABASE.Controllers
{
    public class CONTRACTsController : Controller
    {
        private EVICEntities db = new EVICEntities();
        private ContractIndex contractindex = new ContractIndex();

        // GET: CONTRACTs
        [Authorize(Roles = "Administrator,Employee")]
        public ActionResult Index()
        {
            var indexview = from x in db.CONTRACTs
                            join y in db.RFPs
                            on x.RFPID equals y.RFPID
                            join z in db.VENDORs
                            on x.Id equals z.Id
                            select new ContractIndex { contractID = x.CONTRACTID, rfpID = y.RFPID, category = y.CATEGORY, contractPath = x.CONTRACT_PATH, organization = z.ORGANIZATION, CREATED = x.CREATED, EXPIRES = x.EXPIRES};
                        
            return View(indexview.ToList<ContractIndex>());
        }

        // GET: CONTRACTs/Details/5
        [Authorize(Roles = "Administrator,Employee")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CONTRACT cONTRACT = await db.CONTRACTs.FindAsync(id);
            if (cONTRACT == null)
            {
                return HttpNotFound();
            }
            RFP rfp = await db.RFPs.FindAsync(cONTRACT.RFPID);
            ViewBag.Category = rfp.CATEGORY;

            return View(cONTRACT);
        }

        public JsonResult GetAcceptedVendors(int RFPID)
        {
            EVICEntities dbo = new EVICEntities();

            var vendorProductsQuery = from v in dbo.VENDORs
                                      join c in dbo.RFPINVITEs
                                      on v.Id equals c.Id
                                      join r in dbo.RFPs
                                      on c.RFPID equals r.RFPID
                                      where c.OFFER_PATH != string.Empty
                                      where r.RFPID == RFPID
                                      select new {v.Id, v.ORGANIZATION };

            ViewBag.AcceptedVendors = vendorProductsQuery;

            return Json(vendorProductsQuery, JsonRequestBehavior.AllowGet);
        }         

        // GET: CONTRACTs/Create
        [Authorize(Roles = "Administrator,Employee")]
        public ActionResult Create()
        {
            var rfpidquery = from x in db.RFPs
                             where x.EXPIRES <= DateTime.Now
                             select x.RFPID;

            ViewBag.rfpid = rfpidquery;

            var templates = from x in db.TEMPLATEs
                            where x.TYPE == "CONTRACT"
                            select x;

            ViewBag.templates = new SelectList(templates, "TEMPLATEID", "TEMPLATEID");

            var vendorProductsQuery = from v in db.VENDORs
                                      join c in db.RFPINVITEs
                                      on v.Id equals c.Id
                                      join r in db.RFPs
                                      on c.RFPID equals r.RFPID
                                      where c.OFFER_PATH != string.Empty
                                      where r.RFPID == rfpidquery.FirstOrDefault()
                                      select new { v.Id, v.ORGANIZATION };

            ViewBag.AcceptedVendors = vendorProductsQuery;

            return View();
        }

        private CreateContract negmodel = new CreateContract();

        public ActionResult NegCreate(string Id, int negid, int rfpid)
        {
            if (Id == null || negid == null || rfpid == null )
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            negmodel.contract = new CONTRACT();

            negmodel.contract.Id = Id;

            negmodel.vendor = db.VENDORs.Find(negmodel.contract.Id);

            var templateQuery = from x in db.TEMPLATEs
                           where x.NEGID == negid
                           select x;        

            negmodel.contract.TEMPLATEID = templateQuery.FirstOrDefault().TEMPLATEID;
            
            negmodel.template = db.TEMPLATEs.Find(negmodel.contract.TEMPLATEID);

            RFP rfp = new RFP();
            rfp = db.RFPs.Find(rfpid);

            if (negmodel.vendor == null || negmodel.template == null || rfp == null)
            {
                return HttpNotFound();
            }

            negmodel.contract.RFPID = rfpid;

            negmodel.contract.EXPIRES = DateTime.Now;

            return View(negmodel);

        }


        // POST: CONTRACTs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Employee")]
        public async Task<ActionResult> Create(CreateContract model)
        {
            if (ModelState.IsValid)
            {
                
                if (model.file != null)
                {
                
                    //Extract the file name.
                    var fileName =  Path.GetFileName(model.file.FileName);
                    string documentpath = "~/Content/File_Uploads/" + fileName;
                    //Establishes where to save the path using the extracted name.
                    var path =  Path.Combine(Server.MapPath("~/Content/File_Uploads/"), fileName);
                    //Saves file.
                    model.file.SaveAs(path);
                    CONTRACT con = new CONTRACT { Id = model.contract.Id, TEMPLATEID = model.contract.TEMPLATEID, RFPID = model.contract.RFPID, CREATED = DateTime.Now, EXPIRES = model.contract.EXPIRES, DOCUMENTPATH = documentpath, CONTRACT_PATH = "" };
                    db.CONTRACTs.Add(con);
                }
                else
                {
                    CONTRACT con = new CONTRACT { Id = model.contract.Id, TEMPLATEID = model.contract.TEMPLATEID, RFPID = model.contract.RFPID, CREATED = DateTime.Now, EXPIRES = model.contract.EXPIRES, CONTRACT_PATH = "" };
                    db.CONTRACTs.Add(con);
                }
               
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var rfpidquery = from x in db.RFPs
                             where x.EXPIRES <= DateTime.Now
                             select x.RFPID;

            ViewBag.rfpid = rfpidquery;

            var templates = from x in db.TEMPLATEs
                            where x.TYPE == "CONTRACT"
                            select x;

            ViewBag.templates = new SelectList(templates, "TEMPLATEID", "TEMPLATEID");

            var vendorProductsQuery = from v in db.VENDORs
                                      join c in db.RFPINVITEs
                                      on v.Id equals c.Id
                                      join r in db.RFPs
                                      on c.RFPID equals r.RFPID
                                      where c.OFFER_PATH != string.Empty
                                      where r.RFPID == rfpidquery.FirstOrDefault()
                                      select new { v.Id, v.ORGANIZATION };

            ViewBag.AcceptedVendors = vendorProductsQuery;
            return View(model);
        }

        [Authorize(Roles = "Administrator,Employee,Vendor")]
        public FileResult DownloadTemplate(string path)
        {

            //select contract id
            var templateId = from y in db.TEMPLATEs
                             where y.PATH == path
                             select y.TEMPLATEID;

            string fileName = ("Contract Template - " + templateId.FirstOrDefault().ToString());

            return File(path, GetMimeType(path), fileName);
        }

        [Authorize(Roles = "Administrator,Employee,Vendor")]
        public FileResult DownloadContract(string path) 
        {

            //select vendors Id from contract
            var InviteId = from x in db.CONTRACTs
                           where x.CONTRACT_PATH == path
                           select x.Id;
            //Get vendor items from Id
            VENDOR vendor = db.VENDORs.Find(InviteId.FirstOrDefault());
            //select contract id
            var contractId = from y in db.CONTRACTs
                        where y.CONTRACT_PATH == path
                        where y.Id == vendor.Id
                        select y.CONTRACTID;

            string fileName = (vendor.ORGANIZATION.ToString() + " - " + contractId.FirstOrDefault());

            return File(path, GetMimeType(path), fileName);
        }

        //// GET: CONTRACTs/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    CONTRACT cONTRACT = await db.CONTRACTs.FindAsync(id);
        //    if (cONTRACT == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.Id = new SelectList(db.AspNetUsers, "Id", "Email", cONTRACT.Id);
        //    ViewBag.TEMPLATEID = new SelectList(db.TEMPLATEs, "TEMPLATEID", "TYPE", cONTRACT.TEMPLATEID);
        //    return View(cONTRACT);
        //}

        //// POST: CONTRACTs/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "CONTRACTID,Id,TEMPLATEID,RFPID,CONTRACT_PATH")] CONTRACT cONTRACT)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(cONTRACT).State = EntityState.Modified;
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.Id = new SelectList(db.AspNetUsers, "Id", "Email", cONTRACT.Id);
        //    ViewBag.TEMPLATEID = new SelectList(db.TEMPLATEs, "TEMPLATEID", "TYPE", cONTRACT.TEMPLATEID);
        //    return View(cONTRACT);
        //}


        //Beginning of vendor
        VendorContract responsemodel = new VendorContract();

        //public async Task<ActionResult> VendorResponse(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    responsemodel.contract = await db.CONTRACTs.FindAsync(id);

        //    if (responsemodel.contract == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    responsemodel.contractlist = new List<CONTRACT>();
        //    responsemodel.vendorlist = new List<VENDOR>();

        //    foreach (var x in db.CONTRACTs.ToList())
        //    {

        //        if (x.CONTRACTID == id)
        //        {
        //            VENDOR vendor = await db.VENDORs.FindAsync(x.Id);
        //            responsemodel.contractlist.Add(x);
        //            responsemodel.vendorlist.Add(vendor);
        //        }
        //    }

        //    return View(responsemodel);
        //}

        CreateContract vendorrespond = new CreateContract();

        [Authorize(Roles = "Vendor")]
        public ActionResult Respond(int Id)
        {
            
            vendorrespond.contract = db.CONTRACTs.Find(Id);
            if (!string.IsNullOrWhiteSpace(vendorrespond.contract.CONTRACT_PATH))
            {
                return RedirectToAction("VendorIndex");
            }
            
            return View(vendorrespond);
        }

        //
        //Stores the uploaded form from View VendorRFI/Respond

        [HttpPost]
        [Authorize(Roles = "Vendor")]
        public ActionResult Respond(CreateContract model)
        {
            if (ModelState.IsValid)
            {
                //Verify a file is selected.
                if (model.file != null)
                {
                    CONTRACT con = new CONTRACT();
                    con = db.CONTRACTs.Find(model.contract.CONTRACTID);
                    //Extract the file name.
                    var fileName = Path.GetFileName(model.file.FileName);
                    //Establishes where to save the path using the extracted name.
                    var path = Path.Combine(Server.MapPath("~/Content/ContractStore/"+model.contract.CONTRACTID+"/"), fileName);
                    //Saves file.
                    string contractpath = "~/Content/ContractStore/" + model.contract.CONTRACTID + "/" + fileName;

                    //checks to see if file path exists, if it doesn't it creates
                    var folderpath = Server.MapPath("~/Content/ContractStore/" + model.contract.CONTRACTID + "/");
                    if (!System.IO.Directory.Exists(folderpath))
                        System.IO.Directory.CreateDirectory(folderpath);

                    model.file.SaveAs(path);
                    
                    con.CONTRACT_PATH = contractpath;
                    db.Entry(con).State = EntityState.Modified;
                    db.SaveChanges();

                }

                //Sends the user back to their respective RFI Index page.
                return RedirectToAction("VendorIndex");
            }
            return View(model);
        }

        
        private string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        public FileResult VendorDocDownload(string fileName)
        {
            return File(fileName, GetMimeType(fileName), "Additional Document");
        }

        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> VendorDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CONTRACT cONTRACT = await db.CONTRACTs.FindAsync(id);
            if (cONTRACT == null)
            {
                return HttpNotFound();
            }
            RFP rfp = await db.RFPs.FindAsync(cONTRACT.RFPID);
            ViewBag.Category = rfp.CATEGORY;

            return View(cONTRACT);
        }


        VendorContractIndex vendorIndex = new VendorContractIndex();

        [Authorize(Roles="Vendor")]
        public ActionResult VendorIndex()
        {
            EVICEntities db = new EVICEntities();
            var user = User.Identity.GetUserId();

            var indexview = from x in db.CONTRACTs
                            join y in db.RFPs
                            on x.RFPID equals y.RFPID
                            join z in db.VENDORs
                            on x.Id equals z.Id
                            where z.Id == user
                            select new VendorContractIndex { contractID = x.CONTRACTID, rfpID = y.RFPID, category = y.CATEGORY, contractPath = x.CONTRACT_PATH};

            return View(indexview.ToList<VendorContractIndex>());
        } 	
    }
}
