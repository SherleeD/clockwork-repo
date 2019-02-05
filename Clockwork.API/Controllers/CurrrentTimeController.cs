using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clockwork.API.Models;


namespace Clockwork.API.Controllers
{
    [Route("api/[controller]")]
    public class CurrentTimeController : Controller
    {       
        // GET api/currenttime/1
        [HttpGet("{iNewRequest}/{strTimezone?}")]
        public IEnumerable<CurrentTimeQuery> Get(int iNewRequest, string strTimezone)
        {            
            var db = new ClockworkContext();

            if (iNewRequest == 1)
            {                
                var ip = this.HttpContext.Connection.RemoteIpAddress.ToString();
              
                if (strTimezone == null )
                {
                    var utcTime = DateTime.UtcNow;
                    var serverTime = DateTime.Now;

                    var newRequestVal = new CurrentTimeQuery
                    {
                        UTCTime = utcTime,
                        ClientIp = ip,
                        Time = serverTime
                    };

                    db.CurrentTimeQueries.Add(newRequestVal);
                }
                else
                {
                    //get and store requested time zone server time and utc time
                    DateTime serverDateTime = DateTime.Now;
                    DateTime dbDateTime = serverDateTime.ToUniversalTime();

                    //get date time offset for UTC date stored in the database
                    DateTimeOffset dbDateTimeOffset = new DateTimeOffset(dbDateTime, TimeSpan.Zero);

                    //get user's time zone from profile stored in the database
                    TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(strTimezone);

                    //convert  db offset to user offset
                    DateTimeOffset userDateTimeOffset = TimeZoneInfo.ConvertTime(dbDateTimeOffset, userTimeZone);
                    
                    var newTimezoneRequestVal = new CurrentTimeQuery
                    {
                        UTCTime = dbDateTimeOffset.DateTime,
                        ClientIp = ip,
                        Time = userDateTimeOffset.DateTime
                    };

                    db.CurrentTimeQueries.Add(newTimezoneRequestVal);
                }
                
                var count = db.SaveChanges();

                //display new record in console
                Console.WriteLine("{0} records saved to database", count);

                //Console.WriteLine();
                //foreach (var CurrentTimeQuery in db.CurrentTimeQueries)
                //{
                //    Console.WriteLine(" UTC Time - {0} | Server Time - {1} | IP - {2}", CurrentTimeQuery.UTCTime, CurrentTimeQuery.Time, CurrentTimeQuery.ClientIp);
                //}
            }

            //display previous time queries
            var returnVal = new List<CurrentTimeQuery>();
            var currentTimeQuery = new CurrentTimeQuery();
            
            foreach (CurrentTimeQuery clockwork in db.CurrentTimeQueries)
            {
                currentTimeQuery = new CurrentTimeQuery();

                currentTimeQuery.CurrentTimeQueryId = clockwork.CurrentTimeQueryId;
                currentTimeQuery.ClientIp = clockwork.ClientIp;
                currentTimeQuery.Time = clockwork.Time;
                currentTimeQuery.UTCTime = clockwork.UTCTime;             
                returnVal.Add(currentTimeQuery);

                Console.WriteLine("Query ID- {0} | UTC Time - {1} | Server Time - {2} | IP - {3}", clockwork.CurrentTimeQueryId, clockwork.UTCTime, clockwork.Time, clockwork.ClientIp);
            }
          
            return returnVal;
        }
    }
}
