using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Net.Http;
using Clockwork.Web.ViewModel;
using PagedList.Mvc;
using PagedList;

namespace Clockwork.Web.Controllers
{
    public class HomeController : Controller
    {
        [HandleError]
        public ActionResult Index(int? page)
        {
            //To display project info
            GetProjectInfo();

            //To populate list of time zones
            ListTimeZones();

            //To populate table of time queries on page load
            //var queryResults = GetTimeQueriesFromAPI(strTimezone);
            var queryResults = GetTimeQueriesFromAPI();

            return View(queryResults.ToList().ToPagedList(page ?? 1, 10));
        }

        [HttpPost]
        public ActionResult Index(string timezone, int? page)
        {
            GetProjectInfo();
            ListTimeZones();

            if (timezone != "")
            {
                DateTime serverDateTime = DateTime.Now;
                DateTime dbDateTime = serverDateTime.ToUniversalTime();

                //get date time offset for UTC date stored in the database
                DateTimeOffset dbDateTimeOffset = new DateTimeOffset(dbDateTime, TimeSpan.Zero);

                //get user's time zone from profile stored in the database
                TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);

                //convert  db offset to user offset
                DateTimeOffset userDateTimeOffset = TimeZoneInfo.ConvertTime(dbDateTimeOffset, userTimeZone);

                //format user offset for display purpose
                string dbDateTimeString = dbDateTimeOffset.ToString("dd MMM yyyy - HH:mm:ss (zzz)");
                string userDateTimeString = userDateTimeOffset.ToString("dd MMM yyyy - HH:mm:ss (zzz)");

                ViewBag.UtcDateTime = dbDateTimeString;
                ViewBag.UserDateTime = userDateTimeString;
                ViewBag.TimezoneSelected = timezone;
            }
            else
            {
                ViewBag.UtcDateTime = "";
                ViewBag.UserDateTime = "";
                ViewBag.TimezoneSelected = "";
            }

            var queryResults = GetTimeQueriesFromAPI(timezone);

            return View(queryResults.ToList().ToPagedList(page ?? 1, 10));
        }

        private List<TimeInquiryVM> GetTimeQueriesFromAPI()
        {
            try
            {
                var resultList = new List<TimeInquiryVM>();
                var client = new HttpClient();

                var getDataTask = client.GetAsync("http://localhost:51946/api/currenttime/0")
                .ContinueWith(response =>
                {
                    var result = response.Result;
                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var readResult = result.Content.ReadAsAsync<List<TimeInquiryVM>>();
                        readResult.Wait();
                        //get the result
                        resultList = readResult.Result;
                    }
                });
                getDataTask.Wait();
                return resultList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<TimeInquiryVM> GetTimeQueriesFromAPI(string strTimezone)
        {
            try
            {
                var resultList = new List<TimeInquiryVM>();
                var client = new HttpClient();


                if (strTimezone == "" && strTimezone == null)
                {
                    var getDataTask = client.GetAsync("http://localhost:51946/api/currenttime/1")
                    .ContinueWith(response =>
                    {
                        var result = response.Result;
                        if (result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var readResult = result.Content.ReadAsAsync<List<TimeInquiryVM>>();
                            readResult.Wait();
                            //get the result
                            resultList = readResult.Result;
                        }
                    });
                    getDataTask.Wait();
                }
                else
                {
                    var getTimezoneDataTask = client.GetAsync("http://localhost:51946/api/currenttime/1/" + strTimezone)
                    .ContinueWith(response =>
                    {
                        var result = response.Result;
                        if (result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var readResult = result.Content.ReadAsAsync<List<TimeInquiryVM>>();
                            readResult.Wait();
                            //get the result
                            resultList = readResult.Result;
                        }
                    });
                    getTimezoneDataTask.Wait();
                }

                return resultList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ListTimeZones()
        {
            var timeZones = TimeZoneInfo.GetSystemTimeZones();
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var timeZone in timeZones)
            {
                items.Add(new SelectListItem() { Text = timeZone.Id });
            }
            ViewBag.TimeZones = items;
        }

        private void GetProjectInfo()
        {
            var mvcName = typeof(Controller).Assembly.GetName();
            var isMono = Type.GetType("Mono.Runtime") != null;

            ViewData["Version"] = mvcName.Version.Major + "." + mvcName.Version.Minor;
            ViewData["Runtime"] = isMono ? "Mono" : ".NET";
        }
    }
}
