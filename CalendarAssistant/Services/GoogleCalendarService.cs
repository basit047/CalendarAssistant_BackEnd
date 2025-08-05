using CalendarAssistant.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CalendarAssistant.Services
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly IGoogleCalendarSettings _settings;
        private readonly IConfiguration _configuration;

        public GoogleCalendarService(IGoogleCalendarSettings settings,
            IConfiguration configuration
            )
        {
            _settings = settings;
            _configuration = configuration;
        }


        public async Task<List<EventListView>> GetAllEvents(string accessToken)
        {
             List<EventListView> lstEvents = new List<EventListView>();

            var credentialg = GoogleCredential.FromAccessToken(accessToken);
            var services = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentialg,
                ApplicationName = _configuration["ProjectName"]
            });

            

            EventsResource.ListRequest requestList = services.Events.List("primary");
            requestList.TimeMinDateTimeOffset = DateTime.Now;
            requestList.TimeMaxDateTimeOffset = DateTime.Now.AddDays(7);
            requestList.MaxResults = 1000;
            requestList.SingleEvents = true;
            requestList.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            Events events = requestList.Execute();
            if (events != null && events.Items != null)
            {
                var itemSummary = events.Items.Select(x => x.Summary);
                var json = JsonConvert.SerializeObject(events.Items);

                foreach (var item in events.Items.OrderByDescending(x => x.Start.DateTimeDateTimeOffset!.Value.DateTime))
                {
                    var eventObj = new EventListView();
                    DateTimeOffset startDateTimeOffset = item.Start.DateTimeDateTimeOffset!.Value;
                    DateTimeOffset endDateTimeOffset = item.End.DateTimeDateTimeOffset!.Value;

                    eventObj.Attendees = item.Attendees != null ? string.Join(",", item.Attendees.Select(x => x.Email)) : "";
                    eventObj.DateTimeOffsetInHours = startDateTimeOffset.Offset.Hours;
                    eventObj.Description = item.Description ?? item.Summary;
                    eventObj.StartDay = startDateTimeOffset.DayOfWeek.ToString();
                    eventObj.EndDay = endDateTimeOffset.DayOfWeek.ToString();
                    eventObj.StartTime = startDateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss zzz");
                    eventObj.EndTime = endDateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss zzz");
                    eventObj.Status = item.Status;
                    eventObj.TotalDurationInMinutes = GetDateDifferenceInMinutes(startDateTimeOffset.DateTime, endDateTimeOffset.DateTime);
                    eventObj.EventId = item.Id;
                    eventObj.EventType = item.EventType;

                    lstEvents.Add(eventObj);
                }

                var filteredData = lstEvents.Where(x => x.StartDay == "Monday").Select(x => new DayEvent
                {
                    StartTime = x.StartTime,
                    EndTime = x.EndTime
                });
                string prompt = "I have a working day from 08:00 am to 05:00 pm, and I have meeting(s) from: ";
                foreach (var item in filteredData)
                {
                    prompt += $"{item.StartTime} - {item.EndTime} and ";
                }
            }

            return lstEvents;
        }


        public async Task<bool> Schedule(CalendarEvent calendarEvent, CancellationToken cancellationToken)
        {
            bool isEventScheduled = true;
            try
            {
                var calendarService = await GetCalendarService(cancellationToken);
                if (calendarService != null)
                {
                    var newEvent = CreateEvent(calendarEvent);

                    var eventRequest = calendarService.Events.Insert(newEvent, _settings.CalendarId);
                    var requestCreate = await eventRequest.ExecuteAsync(cancellationToken);
                    bool isSaveEnable = Convert.ToBoolean(_configuration["isSaveEnabled"]);
                }

            }
            catch (Exception)
            {
                isEventScheduled = false;
            }

            return isEventScheduled;
        }

        public async Task<bool> Cancel(string eventId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(eventId))
                return false;

            bool isEventCancelled = true;
            try
            {
                var calendarService = await GetCalendarService(cancellationToken);
                if (calendarService != null)
                {
                    var cancelEvent = calendarService.Events.Delete("primary", eventId);
                    var requestCancelled = await cancelEvent.ExecuteAsync(cancellationToken);
                    bool isSaveEnable = Convert.ToBoolean(_configuration["isSaveEnabled"]);
                }
            }
            catch (Exception)
            {
                isEventCancelled = false;
            }
            return isEventCancelled;
        }

        public async Task<bool> Reschedule(UpdateEvent updateEvent, string accessToken)
        {

            if (string.IsNullOrEmpty(updateEvent.EventId))
                return false;

            bool isEventRescheduled = true;
            try
            {

                var credentialg = GoogleCredential.FromAccessToken(accessToken);
                var services = new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credentialg,
                    ApplicationName = _configuration["ProjectName"]
                });
                
                // Add Attendees
                var attendeeList = new EventAttendee[updateEvent.Attendees!.Count];

                for (int i = 0; i < updateEvent.Attendees.Count; i++)
                {
                    attendeeList[i] = new EventAttendee() { Email = updateEvent.Attendees[i] };
                }

                var updateObj = services.Events.Update(updateEvent.EventObj, "primary", updateEvent.EventId);
                var request = await updateObj.ExecuteAsync();

            }
            catch (Exception)
            {
                isEventRescheduled = false;
            }

            return isEventRescheduled;
        }

        public async Task<Event> GetEventById(string eventId, string accessToken)
        {
            if (string.IsNullOrEmpty(eventId))
                return new Event();


            var credentialg = GoogleCredential.FromAccessToken(accessToken);
            var services = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentialg,
                ApplicationName = _configuration["ProjectName"]
            });

            var requestObj = services.Events.Get("primary", eventId);
            var eventObj = await requestObj.ExecuteAsync();

            return eventObj;

        }

        public async Task<List<Event>> GetEventByDay(DateTime startDate, DateTime endDate, string accessToken, string eventIdToExclude = "")
        {

            var credentialg = GoogleCredential.FromAccessToken(accessToken);
            var services = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentialg,
                ApplicationName = _configuration["ProjectName"]
            });

            EventsResource.ListRequest requestList = services.Events.List("primary");
            requestList.TimeMin = startDate;
            requestList.TimeMax = endDate;

            var events = await requestList.ExecuteAsync();

            var items = events.Items.Where(x => x.Id != eventIdToExclude).ToList();

            return (List<Event>)items;
        }


        public async Task<List<Event>> GetConflictingEvents(DateTime startDate, DateTime endDate, string accessToken, string eventIdToExclude = "")
        {
            try
            {
                var credentialg = GoogleCredential.FromAccessToken(accessToken);

                var services = new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credentialg,
                    ApplicationName = _configuration["ProjectName"]
                });

                EventsResource.ListRequest requestList = services.Events.List("primary");
                requestList.TimeMin = startDate;
                requestList.TimeMax = endDate;
                requestList.SingleEvents = true;

                var events = await requestList.ExecuteAsync();

                var items = events.Items.Where(x => x.Id != eventIdToExclude && startDate >= x.Start.DateTime && endDate <= x.End.DateTime).ToList();
                return (List<Event>)items;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<DateTime?> FindAvailableSlot(DateTime searchStart, DateTime searchEnd, TimeSpan requiredDuration, string accessToken, string timeZone = "UTC")
        {
            var credentialg = GoogleCredential.FromAccessToken(accessToken);
            var services = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentialg,
                ApplicationName = _configuration["ProjectName"]
            });
           
            var requestBody = new FreeBusyRequest
            {
                TimeMin = searchStart,
                TimeMax = searchEnd,
                TimeZone = timeZone,
                Items = new List<FreeBusyRequestItem>
        {
            new FreeBusyRequestItem { Id = "primary" }
        }
            };

            var freeBusyRequest = services.Freebusy.Query(requestBody);
            var response = await freeBusyRequest.ExecuteAsync();

            if (!response.Calendars.TryGetValue("primary", out var calendarBusy))
                throw new Exception("Calendar data not returned or not found.");

            var busyTimes = calendarBusy.Busy.OrderBy(b => b.Start).ToList();
            DateTime currentTime = searchStart;

            foreach (var busy in busyTimes)
            {
                var busyStart = busy.Start.Value;
                var busyEnd = busy.End.Value;

                // If there's a gap between current time and the next busy slot
                if (currentTime + requiredDuration <= busyStart)
                {
                    return currentTime;
                }

                // Advance current time if inside a busy period
                if (currentTime < busyEnd)
                {
                    currentTime = busyEnd;
                }
            }

            // After the last busy period
            if (currentTime + requiredDuration <= searchEnd)
            {
                return currentTime;
            }

            return null; // No free slot found
        }



        #region "Helper Methods"
        private Event CreateEvent(CalendarEvent calendarEvent)
        {
            if (calendarEvent == null)
                return new Event();

            var newEvent = new Event()
            {
                Summary = calendarEvent.Summary,
                Location = calendarEvent.Location,
                Description = calendarEvent.Description,

                Start = new EventDateTime()
                {
                    DateTimeDateTimeOffset = calendarEvent.StartTime,
                    TimeZone = TimeZoneInfo.Local.DisplayName
                },
                End = new EventDateTime()
                {
                    DateTimeDateTimeOffset = calendarEvent.EndTime,
                    TimeZone = TimeZoneInfo.Local.DisplayName
                },
                Recurrence = new String[] { "RRULE:FREQ=DAILY;COUNT=1" },
                Reminders = new Event.RemindersData()
                {
                    UseDefault = false,
                    Overrides = new EventReminder[]
                        {
                        new EventReminder() { Method = "email", Minutes = 24 * 60 },
                        }
                }
            };


            // Add Attendees
            var attendeeList = new EventAttendee[calendarEvent.Attendees!.Length];

            for (int i = 0; i < calendarEvent.Attendees.Length; i++)
            {
                attendeeList[i] = new EventAttendee() { Email = calendarEvent.Attendees[i] };
            }

            newEvent.Attendees = attendeeList;
            return newEvent;
        }

        private async Task<CalendarService> GetCalendarService(CancellationToken cancellationToken)
        {
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                       new ClientSecrets()
                       {
                           ClientId = _settings.ClientId,
                           ClientSecret = _settings.ClientSecret
                       },
                       _settings.Scope,
            _settings.User,
                       cancellationToken);

            var services = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _settings.ApplicationName,
            });

            return services;
        }

        private short GetEventStatus(string status)
        {
            return status switch
            {
                "confirmed" => 0,
                "tentative" => 1,
                "cancelled" => 2,
                "rescheduled" => 3,
                _ => throw new NotImplementedException(),
            };
        }
        private async Task<UserCredential> GetUserCredentials(CancellationToken cancellationToken)
        {
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets()
                    {
                        ClientId = _settings.ClientId,
                        ClientSecret = _settings.ClientSecret
                    },
                    _settings.Scope,
            _settings.User,
                    cancellationToken);

            return credential;
        }

        private double GetDateDifferenceInMinutes(DateTime startDate, DateTime endDate) => (endDate - startDate).TotalMinutes;

        public async Task<UserAuthentication> AuthenticateUser(TokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token))
                    return new UserAuthentication();

                var token = await ExchangeCodeForToken(request.Token);

                var userInfo = await GetUserInfo(token.AccessToken);
                userInfo.IdToken = token.IdToken;
                userInfo.AccessToken = token.AccessToken;

                return userInfo;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Message: {ex.Message}");
                throw ex;
            }
        }

        private async Task<UserAuthentication> GetUserInfo(string accessToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetStringAsync("https://www.googleapis.com/oauth2/v3/userinfo");
            var json = JsonDocument.Parse(response).RootElement;

            return new UserAuthentication
            {
                Email = json.GetProperty("email").GetString(),
                Name = json.GetProperty("name").GetString(),
                Picture = json.GetProperty("picture").GetString()
            };
        }

        private async Task<TokenResponse> ExchangeCodeForToken(string code)
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _configuration["GoogleCalendarSettings:ClientId"],
                    ClientSecret = _configuration["GoogleCalendarSettings:ClientSecret"]
                },
                Scopes = new[]
                {
                GmailService.Scope.GmailReadonly,
                GmailService.Scope.GmailSend,
                CalendarService.Scope.Calendar,
                //CalendarService.Scope.CalendarReadonly,
                "openid", "email", "profile"
            }
            });

            var token = await flow.ExchangeCodeForTokenAsync("", code, "https://calendar-assistant-front-end-q85t.vercel.app", CancellationToken.None);
            return token;
        }


        #endregion
    }
}
