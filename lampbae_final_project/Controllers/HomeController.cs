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

        public ActionResult NewLamp()
        {
            ViewBag.Title = "Upload A New Lamp";

            UserListing u1 = new UserListing();

            return View();
        }

        [HttpPost]
        public ActionResult NewLamp(UserListing model,HttpPostedFileBase image1)
        {
            var db = new LampBaeEntities();

            if (image1 != null)
            {
                model.Image = new byte[image1.ContentLength];
                image1.InputStream.Read(model.Image, 0, image1.ContentLength);
            }
            db.UserListings.Add(model);
            db.SaveChanges();

            ViewBag.Title = "Upload A New Lamp";

            UserListing u1 = new UserListing();

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Lamps()
        {

            LampBaeEntities db = new LampBaeEntities();
            List<EbayListing> LampDBList = new List<EbayListing>();


            EbayListing e = (from p in db.EbayListings
                          where p.ID == 394
                          select p).Single();

            string url = e.GalleryURL;

            ViewBag.urlforone = url;

            //List<EbayListing> IDAndNames = Lam.Select(p => new ProductShort { ProductID = p.ProductID, ProductName = p.ProductName }).ToList();
            //LampDBList() 

            //ViewBag.URL = LampBaeEntities
            ViewBag.Title = "Lamps";

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
                EbayListing eItem = new EbayListing();

                //instantiate new lamp entities database
                LampBaeEntities db = new LampBaeEntities();

                List<EbayListing> LampDBList = new List<EbayListing>();

                LampDBList = (from p in db.EbayListings
                              where p.ItemID != ""
                              select p).ToList();

                for (int i = 0; i < items.Count; i++)
                {
                    if (LampDBList.Exists(x => x.ItemID == (string)items[i]["itemId"][0]))
                    {
                        //dupes detected
                    }
                    else
                    {
                        eItem.Title = (string)items[i]["title"][0];
                        eItem.GalleryURL = (string)items[i]["galleryURL"][0];
                        eItem.PostalCode = (string)items[i]["postalCode"][0];
                        eItem.ItemID = (string)items[i]["itemId"][0];
                        eItem.EndDate = (DateTime)items[i]["listingInfo"][0]["endTime"][0];
                        eItem.Price = (decimal)items[i]["sellingStatus"][0]["currentPrice"][0]["__value__"];
                        new EbayListing() { ItemID = eItem.ItemID, Title = eItem.Title, PostalCode = eItem.PostalCode, GalleryURL = eItem.GalleryURL };
                        db.EbayListings.Add(eItem);
                        db.SaveChanges();
                    }
                }
            }
            return View();
        }
    }
}