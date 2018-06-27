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
    public class UtilityClass
    {
        // gets IP address of user
        public static string GetIP()
        {
            //string userIP1 = new WebClient().DownloadString("http://icanhazip.com");
            string userIP = HttpContext.Current.Request.UserHostAddress;
            return userIP;
        }

        //determine the zip code based on the IP address
        public static string GetZip(string userIP)
        {
            HttpWebRequest ipstackrequest =
                WebRequest.CreateHttp($"http://api.ipstack.com/{userIP}?access_key=d73350465665ed422161f5a8724f5bd2");
            ipstackrequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
            HttpWebResponse IPStackResponse;
            IPStackResponse = (HttpWebResponse)ipstackrequest.GetResponse();

            StreamReader IPReader = new StreamReader(IPStackResponse.GetResponseStream());
            string IPData = IPReader.ReadToEnd();

            JObject IPJsonData = JObject.Parse(IPData);
            string userZipCode = (string)IPJsonData["zip"];

            return userZipCode;
        }

        //determine the distance two zip codes
        public static string GetZipCodeDistance(string userZip, string itemZip)
        {
            HttpWebRequest request =
                WebRequest.CreateHttp($"https://redline-redline-zipcode.p.mashape.com/rest/distance.json/{userZip}/{itemZip}/mile");
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";

            // Adding keys to the header 
            request.Headers.Add("X-Mashape-Key", "bsytHiIt3FmshiXhyAlMdv4D7Vsxp1OeNuxjsnNL5l6EVqH1u9");
            HttpWebResponse Response;
            Response = (HttpWebResponse)request.GetResponse();
            
            StreamReader reader = new StreamReader(Response.GetResponseStream());
            string distanceData = reader.ReadToEnd();

            JObject JsonData = JObject.Parse(distanceData);
            string distance = (string)JsonData["distance"];
            string[] distanceSplit = distance.Split('.');
            return distanceSplit[0];


        }
    }
}