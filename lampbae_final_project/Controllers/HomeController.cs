using lampbae_final_project.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace lampbae_final_project.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [Authorize]
        public ActionResult NewLamp()
        {
            ViewBag.Title = "Upload A New Lamp";

            Listing u1 = new Listing();

            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult NewLamp(HttpPostedFileBase file, Listing model)
        {
            ViewBag.Title = "Upload A New Lamp";

            var db = new LampBaeEntities1();
            var path = "";
            if (file != null)
            {
                if (file.ContentLength > 0)
                {
                    //for checking uploaded file is valid or not
                    if (Path.GetExtension(file.FileName).ToLower() == ".png"
                        || Path.GetExtension(file.FileName).ToLower() == ".jpg"
                        || Path.GetExtension(file.FileName).ToLower() == ".jpeg"
                        || Path.GetExtension(file.FileName).ToLower() == ".gif")
                    {
                        //saves to server
                        path = Path.Combine(Server.MapPath("~/Content/Images"), file.FileName);
                        file.SaveAs(path);

                        //formats path for simplified SQL database storage & HTML retrieval
                        path = path.Substring(path.IndexOf("Images"));
                        path = path.Replace(@"\\", @"/");
                        path = path.Replace(@"\", @"/");

                        //assign formatted path to image column of this record.
                        model.Image = path;

                    }
                }
            }

            db.Listings.Add(model);
            db.SaveChanges();

            Listing u1 = new Listing();

            return RedirectToAction("Index", "Home");
        }

        // ************ Just testing out the ratings table *********** //
        // icnrement the rating if a valid lampid is deliberately typed.
        [Authorize]
        public ActionResult Lamps(int? lampid)
        {
            ViewBag.Title = "Lamps";

            //instantiate DB from model
            LampBaeEntities1 db = new LampBaeEntities1();

            //instantiate new list for lamp id's
            List<Listing> LampDBList = new List<Listing>();

            //populate list from model/db
            LampDBList = (from p in db.Listings
                          where p.ID != 0
                          select p).ToList();

            //instantiate new list for ratings
            List<ViewCount> ViewCountList = (from p in db.ViewCounts
                                          where p.ViewID != 0
                                          select p).ToList();

            //declaring our listing object
            Listing listing = null;

            // if no lampid is provided, a random id will be generated and assigned to x
            if (lampid == null || lampid > LampDBList.Count || lampid < LampDBList.Count)
            {
                //instantiate new random object & create a new random int based on range of list count as max value
                Random r = new Random();
                lampid = r.Next(1, LampDBList.Count());
                listing = (from p in db.Listings
                           where p.ID == lampid
                           select p).Single();
            }

            else
            {

                //grabs a listing from list
                listing = (from p in db.Listings
                           where p.ID == lampid
                           select p).Single();
            }

            //handling for ebay listings vs user listings (the image url structure is different)
            if (listing.EbayItemNumber == null)
            {
                ViewData["ImageURL"] = Url.Content("~/Content/" + listing.Image);
            }
            else
            {
                ViewData["ImageURL"] = listing.Image;
            }

            //test if view record exists already
            ViewCount existingViewCount = null;
            try
            {
                existingViewCount = (from m in db.ViewCounts
                                  where m.ItemID == lampid
                                  && m.UserID == User.Identity.Name
                                  select m).Single();


            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }



            // if a view record already exists, simply increment
            if (existingViewCount != null)
            {
                existingViewCount.ViewCount1 = existingViewCount.ViewCount1 + 1;
                db.SaveChanges();
            }

            //if a view record doesn't already exist, create a new record and assign it a value of one
            else
            {
                ViewCount newviewcount = new ViewCount();
                newviewcount.UserID = User.Identity.Name;
                newviewcount.ItemID = (int)lampid;
                newviewcount.ViewCount1 = 1;
                newviewcount.ViewDate = DateTime.Now;
                new ViewCount() { UserID = newviewcount.UserID, ItemID = (int)lampid, ViewCount1 = 1, ViewDate = DateTime.Now};
                db.ViewCounts.Add(newviewcount);
                db.SaveChanges();

            }
            return View();
        }
    
        [Authorize]
        public ActionResult Search()
        {
            ViewBag.Title = "Home Page";
            JArray items;
            string keyword = "lamp";
            string postalcode = "48206";
            int page;

            for (page = 1; page < 5; page++)
            {
                HttpWebRequest WR = WebRequest.CreateHttp($"http://svcs.ebay.com/services/search/FindingService/v1?OPERATION-NAME=findItemsByKeywords" +
                    $"&SERVICE-VERSION=1.0.0&GLOBAL-ID=EBAY-US&SECURITY-APPNAME=shaunitt-studenta-PRD-b7141958a-1d5c6f0c&RESPONSE-DATA-FORMAT=JSON&REST-PAYLOAD=TRUE" +
                    $"&keywords={keyword}&buyerPostalCode={postalcode}&itemFilter.name=MaxDistance&itemFilter.value=40" +
                    $"&paginationInput.entriesPerPage=100&paginationInput.pageNumber={page}");

                HttpWebResponse Response;
                try
                {
                    Response = (HttpWebResponse)WR.GetResponse();
                }
                catch (WebException e)
                {
                    ViewBag.Error = "Exception";
                    ViewBag.ErrorDescription = e.Message;
                    return View();
                }

                //reads response
                StreamReader reader = new StreamReader(Response.GetResponseStream());
                string searchData = reader.ReadToEnd();
                //parses JSON
                JObject JsonData = JObject.Parse(searchData);

                //J array for items returned
                items = (JArray)JsonData["findItemsByKeywordsResponse"][0]["searchResult"][0]["item"];


                //instantiate new ebaylisting object
                Listing listing = new Listing();

                //instantiate new lamp entities database
                LampBaeEntities1 db = new LampBaeEntities1();

                List<Listing> LampDBList = new List<Listing>();

                LampDBList = (from p in db.Listings
                              where p.ID != 0
                              select p).ToList();

                for (int i = 0; i < items.Count; i++)
                {
                    if (LampDBList.Exists(x => x.EbayItemNumber.ToString() == (string)items[i]["itemId"][0]))
                    {
                        //dupes detected
                    }
                    else
                    {
                        listing.Title = (string)items[i]["title"][0];
                        listing.Image = (string)items[i]["galleryURL"][0];
                        listing.PostalCode = (string)items[i]["postalCode"][0];
                        listing.EbayItemNumber = (string)items[i]["itemId"][0];
                        listing.EndDate = (DateTime)items[i]["listingInfo"][0]["endTime"][0];
                        listing.Price = (decimal)items[i]["sellingStatus"][0]["currentPrice"][0]["__value__"];
                        new Listing() { EbayItemNumber = listing.EbayItemNumber, Title = listing.Title, PostalCode = listing.PostalCode, Image = listing.Image };
                        db.Listings.Add(listing);
                        db.SaveChanges();
                    }
                }
            }
            return View();
        }
    }
}