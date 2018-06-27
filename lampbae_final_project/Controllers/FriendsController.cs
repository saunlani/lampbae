using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using lampbae_final_project.Models;

namespace lampbae_final_project.Controllers
{
    [Authorize]
    public class FriendsController : Controller
    {
        private LampBaeEntities1 db = new LampBaeEntities1();

        // GET: Friends
        public ActionResult Index()
        {
            string currentUserId = User.Identity.Name;
            var thisUsersFriends = db.Friends.Where(e => e.UserID1 == currentUserId || e.UserID2 == currentUserId);
            return View(thisUsersFriends.ToList());
        }

        // GET: Friends/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Friend friend = db.Friends.Find(id);
            if (friend == null)
            {
                return HttpNotFound();
            }
            return View(friend);
        }

        // GET: Friends/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Friends/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ConnectionID,UserID1,UserID2")] Friend friend)
        {
            if (ModelState.IsValid)
            {
                db.Friends.Add(friend);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(friend);
        }

        // GET: Friends/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Friend friend = db.Friends.Find(id);
            if (friend == null)
            {
                return HttpNotFound();
            }
            return View(friend);
        }

        // POST: Friends/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ConnectionID,UserID1,UserID2")] Friend friend)
        {
            if (ModelState.IsValid)
            {
                db.Entry(friend).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(friend);
        }

        // GET: Friends/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Friend friend = db.Friends.Find(id);
            if (friend == null)
            {
                return HttpNotFound();
            }
            return View(friend);
        }

        // POST: Friends/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Friend friend = db.Friends.Find(id);
            db.Friends.Remove(friend);
            db.SaveChanges();
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

        public ActionResult GetFriendsByEmail(string email)
        {
            //instantiate DB
            LampBaeEntities1 db = new LampBaeEntities1();

            AspNetUser friend = new AspNetUser();


            List<AspNetUser> SearchResults = new List<AspNetUser>();

            if (email != "")
            {
                try
                {
                    SearchResults = (from u in db.AspNetUsers
                                     where u.Email.Contains(email)
                                     select u).ToList();
                }
                catch
                {
                    Exception e;
                }
            }


            return Json(SearchResults);


        }

        public ActionResult FriendLampList (string friendID)
        {
            LampBaeEntities1 db = new LampBaeEntities1();

            List<Rating> FriendLampRList = new List<Rating>();
            FriendLampRList = (from p in db.Ratings
                             where p.UserID == friendID
                             select p).ToList();

            //randomize the llist
            var rand = new Random();
            FriendLampRList = FriendLampRList.OrderBy(x => rand.Next()).ToList();

            ViewBag.FriendLampRList = FriendLampRList;

            Listing listing = new Listing();

            List<Listing> FriendLampListings = new List<Listing>();

            if (FriendLampRList.Count != 0)
            {
                if (FriendLampRList.Count < 5)
                {
                    for (int i = 0; i < FriendLampRList.Count; i++)
                    {
                        var LampId = FriendLampRList[i].ItemID;
                        listing = db.Listings.Find(LampId);
                        FriendLampListings.Add(listing);
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        var LampId = FriendLampRList[i].ItemID;
                        listing = db.Listings.Find(LampId);
                        FriendLampListings.Add(listing);
                    }
                }
            }
            else
            { }

            ViewBag.FriendLampListings = FriendLampListings;
            return View();

        }
    }
}
