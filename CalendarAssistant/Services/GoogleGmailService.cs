using CalendarAssistant.Helpers;
using CalendarAssistant.Models;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Calendar.v3;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

using MimeKit;
using System.Globalization;
using System.Text;
using Azure.Core;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.WebUtilities;
using OllamaSharp.Models.Chat;

namespace CalendarAssistant.Services
{
    public class GoogleGmailService : IGoogleGmailService
    {
        private readonly IConfiguration _configuration;
        private readonly CalendarAssistantContext _calendarAssistantContext;
        private readonly IGoogleCalendarService _googleCalendarService;
        private readonly IHttpService _httpService;
        public GoogleGmailService(IConfiguration configuration,
            CalendarAssistantContext calendarAssistantContext,
            IGoogleCalendarService googleCalendarService,
            IHttpService httpService)
        {
            _configuration = configuration;
            _calendarAssistantContext = calendarAssistantContext;
            _googleCalendarService = googleCalendarService;
            _httpService = httpService;
        }


        public async Task<List<LabelView>> GetGmailLabels(string? accessToken)
        {
            List<LabelView> labelsView = new List<LabelView>();
            try
            {
                string[] scopes = { GmailService.Scope.GmailReadonly };
                UserCredential credential = await GetGmailCredentials(scopes);

                var service = new GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = _configuration["ProjectName"]
                });

                UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");

                IList<Label> labels = request.Execute().Labels;
                Console.WriteLine("Labels:");
                if (labels == null || labels.Count == 0)
                {
                    return labelsView;
                }


                foreach (var labelItem in labels)
                {
                    var labelView = new LabelView()
                    {
                        Color = labelItem.Color?.BackgroundColor ?? "No Color",
                        Name = labelItem.Name,
                        Id = labelItem.Id
                    };

                    labelsView.Add(labelView);
                }

            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            return labelsView;
        }

        public async Task<List<MailView>> GetMails(MailFilter filters, string? accessToken)
        {

            List<MailView> messagesView = new List<MailView>();
            try
            {
                string[] scopes = { GmailService.Scope.GmailReadonly };
                UserCredential credential = await GetGmailCredentials(scopes);

                var service = new GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = _configuration["ProjectName"]
                });

                string mailFilter = GenerateFilterStringFromModel(filters);
                var request = service.Users.Messages.List("me");
                request.MaxResults = filters.NumberOfMail;
                request.LabelIds = "INBOX";
                request.Q = mailFilter;
                ListMessagesResponse requestResponse = await request.ExecuteAsync();

                if (requestResponse != null && requestResponse.Messages != null)
                {
                    foreach (var message in requestResponse.Messages)
                    {
                        var messageRequest = service.Users.Messages.Get("me", message.Id);
                        var messageObj = await messageRequest.ExecuteAsync();


                        var messageViewObj = new MailView()
                        {
                            Snippet = messageObj.Snippet
                        };

                        foreach (var header in messageObj.Payload.Headers)
                        {

                            switch (header.Name?.ToLower())
                            {
                                case "subject":
                                    messageViewObj.Subject = header.Value;
                                    break;
                                case "from":
                                    messageViewObj.From = header.Value;
                                    break;
                                case "date":
                                    messageViewObj.ReceivedAt = header.Value;
                                    break;
                                default:
                                    break;
                            }
                        }
                        messagesView.Add(messageViewObj);
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            return messagesView;

        }

        public async Task<bool> SendEmail(SendEmailModel sendEmailModel, string? accessToken)
        {
            string[] scopes = { GmailService.Scope.GmailSend };
            //UserCredential credential = await GetGmailCredentials(scopes);
            var credential = GoogleCredential.FromAccessToken(accessToken);
            var service = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = _configuration["ProjectName"]
            });

            var email = CreateMessage(sendEmailModel.To!, sendEmailModel.Subject!, sendEmailModel.Message!, sendEmailModel.MessageId!);
            var request = service.Users.Messages.Send(email, "me");
            var data = await request.ExecuteAsync();

            if (data != null)
                return true;
            else
                return false;
        }

        public async Task<List<EmailModelSync>> CheckForNewEmails(EmailSyncModel emailSyncModel, string? accessToken, string? idToken)
        {
            List<EmailModelSync> lstEmailModelSync = new List<EmailModelSync>();

            string[] scopes = { GmailService.Scope.GmailReadonly, GmailService.Scope.GmailModify };
            var credential = GoogleCredential.FromAccessToken(accessToken);
            string classifierUrl = _configuration["ClassifierURL"]!;
            string ollamaUrl = _configuration["OllamaURL"]!;
            string userEmail = await ValidateIdTokenReturnEmail(idToken);


            List<LabelView>? filteredLabels = new List<LabelView>();
            string filters = "";

            if (emailSyncModel?.LabelsToExclude?.Any() ?? false)
            {
                var allLabels = await GetGmailLabels(accessToken);
                filteredLabels = allLabels.Where(x => !emailSyncModel.LabelsToExclude.Contains(x.Id!)).ToList();
                filters = GetAllLabelsInFilter(filteredLabels, filters);
            }

            var service = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = _configuration["ProjectName"]
            });

            //Get last sync date from file with email
            var lastSyncDateTime = UserSyncFileWriter.GetUserByEmail(userEmail).SyncDateTime;

            var request = service.Users.Messages.List("me");
            filters += $" after:{lastSyncDateTime.ToString("yyyy/MM/dd")}";
            request.Q = filters;
            var response = request.Execute();

            if (response?.Messages != null)
            {
                foreach (var message in response.Messages)
                {
                    var msg = service.Users.Messages.Get("me", message.Id).Execute();

                    // To extract recipient FROM
                    var fromHeader = msg.Payload.Headers.FirstOrDefault(h => h.Name.Equals("From", StringComparison.OrdinalIgnoreCase)).Value;
                    string messageId = msg.Payload.Headers.FirstOrDefault(h => h.Name == "Message-ID")?.Value;
                    //Meaning its an event
                    if (msg?.Payload?.Parts != null
                        &&
                        msg.Payload.Parts.Any(x => x.MimeType == "application/ics"))
                    {
                        var result = await GetStartEndDateTimeForEvents(service, message, msg, accessToken, userEmail);

                        var emailSync = new EmailModelSync()
                        {
                            ReceivedAt = ConvertLongDateTimeToDateTime(msg.InternalDate),
                            From = fromHeader,
                            IsMeetingInvite = true,
                            Snippet = msg.Snippet,
                            HasConflict = true,
                            EventModel = result.First(),
                            MessageId = messageId
                        };

                        lstEmailModelSync.Add(emailSync);
                    }
                    else
                    {
                        // Check with the email classifier
                        var classifierResponse = await _httpService.PostAsyncToClassifier(classifierUrl, msg?.Snippet!);

                        if (classifierResponse.IsMeetingInvite)
                        {
                            float thresholdConfidenceValue = float.Parse(_configuration["ClassifierConfidenceThreshold"]!);
                            if (classifierResponse.Confidence >= thresholdConfidenceValue)
                            {
                                var model = await GetEmailModel(msg.Snippet, fromHeader, msg.InternalDate, messageId, accessToken);
                                lstEmailModelSync.Add(model);
                            }
                            else
                            {
                                // Check with the email classifier
                                var llmResponse = await _httpService.GetEmailClassificationOllamaPostAsync(classifierUrl, msg?.Snippet!, "phi3");
                                if (llmResponse.IsEmailMeetingInvite)
                                {
                                    var model = await GetEmailModel(msg.Snippet, fromHeader, msg.InternalDate, messageId, accessToken);
                                    lstEmailModelSync.Add(model);
                                }
                                else
                                {
                                    var response_ = await _httpService.GetEmailResponseSummaryOllamaPostAsync(ollamaUrl, msg?.Snippet!, "phi3");
                                    var emailSync = new EmailModelSync()
                                    {
                                        ReceivedAt = ConvertLongDateTimeToDateTime(msg.InternalDate),
                                        From = fromHeader,
                                        IsMeetingInvite = false,
                                        Snippet = msg.Snippet,
                                        MessageId = messageId,
                                        NonMeetingInviteResponse = new NonMeetingInviteResponse()
                                        {
                                            ResponseSuggestedByLLM = response_.Reply,
                                            Summary = response_.Summary ?? ""
                                        }
                                    };

                                    lstEmailModelSync.Add(emailSync);
                                }
                            }
                        }

                        else
                        {

                            var response_ = await _httpService.GetEmailResponseSummaryOllamaPostAsync(ollamaUrl, msg?.Snippet!, "phi3");
                            var emailSync = new EmailModelSync()
                            {
                                ReceivedAt = ConvertLongDateTimeToDateTime(msg.InternalDate),
                                From = fromHeader,
                                IsMeetingInvite = false,
                                Snippet = msg.Snippet,
                                MessageId = messageId,
                                NonMeetingInviteResponse = new NonMeetingInviteResponse()
                                {
                                    ResponseSuggestedByLLM = response_.Reply,
                                    Summary = response_.Summary ?? ""
                                }
                            };

                            lstEmailModelSync.Add(emailSync);
                        }
                    }
                }
            }
            //Update Last sync date
            UserSyncFileWriter.UpdateUserSync(userEmail);
            return lstEmailModelSync;
        }

        public DateTime GetLastSyncDate(string email) => UserSyncFileWriter.GetUserByEmail(email).SyncDateTime;


        #region "Private Methods"

        private string GetAllLabelsInFilter(List<LabelView> filteredLabels, string filters)
        {
            foreach (var filter in filteredLabels)
            {
                var model = new MailFilter()
                {
                    Label = filter.Id
                };

                string value = GenerateFilterStringFromModel(model);

                if (filters.Contains("label"))
                {
                    filters += " OR ";
                    filters += value;
                }
                else
                {
                    filters += value;
                }
            }

            return filters;
        }

        private DateTime? GetDateTimeFromStringUsingRecognizers(string text)
        {
            DateTime? parsedDateTime = null;
            var results = Microsoft.Recognizers.Text.DateTime.DateTimeRecognizer.RecognizeDateTime(text, Microsoft.Recognizers.Text.Culture.English);

            foreach (var result in results)
            {
                var resolutionValues = (IList<Dictionary<string, string>>)result.Resolution["values"];
                foreach (var val in resolutionValues)
                {
                    if (val.TryGetValue("value", out string datetimeString))
                        parsedDateTime = DateTime.Parse(datetimeString, null, DateTimeStyles.RoundtripKind);
                }
            }
            return parsedDateTime;
        }

        private async Task<List<EventModel>> GetStartEndDateTimeForEvents(GmailService service, Google.Apis.Gmail.v1.Data.Message? message, Google.Apis.Gmail.v1.Data.Message? msg, string accessToken, string userEmail)
        {
            DateTime? eventStartDateTime = null;
            DateTime? eventEndDateTime = null;
            DateTime? nextAvailableStartTime = null;
            DateTime? nextAvailableEndTime = null;
            UpdateEvent updateEventModel = new UpdateEvent();
            SendEmailModel sendEmailModel = new SendEmailModel();
            List<EventModel> lstEventModel = new List<EventModel>();

            var icsPart = msg?.Payload.Parts.FirstOrDefault(p => p.Filename?.EndsWith(".ics") == true && p.Body?.AttachmentId != null);

            var attachment = service.Users.Messages.Attachments
                            .Get("me", message?.Id, icsPart?.Body.AttachmentId)
                            .Execute();

            var icsData = Encoding.UTF8.GetString(Convert.FromBase64String(
                attachment.Data.Replace("-", "+").Replace("_", "/")));

            //To get Start Date & Time
            var startDateTimeLine = icsData.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).FirstOrDefault(line => line.StartsWith("DTSTART;"));
            if (!string.IsNullOrEmpty(startDateTimeLine))
            {
                var startDateTime = startDateTimeLine.Split(':').Last().Trim();
                eventStartDateTime = DateTime.ParseExact(startDateTime, "yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
            }

            //For End date & time
            var endDateTimeLine = icsData.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).FirstOrDefault(line => line.StartsWith("DTEND;"));

            if (!string.IsNullOrEmpty(endDateTimeLine))
            {
                var endDateTime = endDateTimeLine.Split(':').Last().Trim();
                eventEndDateTime = DateTime.ParseExact(endDateTime, "yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
            }

            var uidCalendarId = icsData.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).FirstOrDefault(line => line.StartsWith("UID:"));

            if (!string.IsNullOrEmpty(uidCalendarId))
            {
                uidCalendarId = uidCalendarId.Split(':').Last().Trim();
                uidCalendarId = uidCalendarId.Substring(0, 26);
            }

            if (eventStartDateTime.HasValue && eventEndDateTime.HasValue)
            {
                var list = await _googleCalendarService.GetConflictingEvents(eventStartDateTime.Value, eventEndDateTime.Value, accessToken, uidCalendarId);

                if (list.Any()) //Means conflict occured
                {
                    int dayId = (short)eventEndDateTime.Value.DayOfWeek;
                    var userDetails = UserSyncFileWriter.GetUserByEmail(userEmail);

                    var modelt = await FindNextAvailableSlotAndRescheduleAsync(userEmail, eventStartDateTime.Value, eventEndDateTime.Value, accessToken, uidCalendarId);

                    foreach (var item in list.OrderBy(x => x.Created.Value))
                    {
                        var model = new EventModel()
                        {
                            //Conflicting
                            ConflictingEndDateTime = item.End.DateTime.Value,
                            ConflictingStartDateTime = item.Start.DateTime.Value,
                            ConflictingTitle = item.Summary,

                            //Actual
                            Title = msg.Snippet,
                            EndDateTime = eventEndDateTime.Value,
                            StartDateTime = eventStartDateTime.Value,

                            //Suggested
                            SuggestedEndDateTime = modelt.Item4,
                            SuggestedStartDateTime = modelt.Item3,

                            UpdateEvent = modelt.Item1,
                            SendEmailModel = modelt.Item2,

                        };

                        lstEventModel.Add(model);
                    }
                }
            }

            return lstEventModel;
        }

        private async Task<UserCredential> GetGmailCredentials(string[] scopes)
        {
            using (var stream = new FileStream(_configuration["ClientSecretGoogle"]!, FileMode.Open, FileAccess.Read))
            {
               
                var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None);

                return credential;
            }
        }

        private string GenerateFilterStringFromModel(MailFilter filter)
        {
            StringBuilder filterValue = new StringBuilder();
            if (filter.HasAttachment)
                filterValue.Append("has:attachment ");

            if (!string.IsNullOrEmpty(filter.From))
                filterValue.Append($"from:{filter.From} ");

            if (!string.IsNullOrEmpty(filter.To))
                filterValue.Append($"to:{filter.To} ");

            if (!string.IsNullOrEmpty(filter.Label))
                filterValue.Append($"label:{filter.Label} ");

            if (!string.IsNullOrEmpty(filter.Subject))
                filterValue.Append($"subject:{filter.Subject} ");

            if (filter.After.HasValue)
                filterValue.Append($"after:{filter.After.Value.ToString("yyyy/MM/dd")} ");

            if (filter.Before.HasValue)
                filterValue.Append($"before:{filter.Before.Value.ToString("yyyy/MM/dd")} ");


            return filterValue.ToString().TrimEnd();
        }

        private Google.Apis.Gmail.v1.Data.Message CreateMessage(string to, string subject, string body, string messageId)
        {
            var emailMessage = new MimeMessage();
            emailMessage.To.Add(MailboxAddress.Parse(to));
            emailMessage.Subject = subject ?? "Reply";
            if (!string.IsNullOrEmpty(messageId))
                emailMessage.InReplyTo = messageId;
            emailMessage.Body = new TextPart("plain") { Text = body };

            using (var stream = new MemoryStream())
            {
                emailMessage.WriteTo(stream);
                var bytes = stream.ToArray();
                var base64Url = Convert.ToBase64String(bytes)
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .Replace("=", "");

                return new Google.Apis.Gmail.v1.Data.Message { Raw = base64Url };
            }
        }




        private DateTime ConvertLongDateTimeToDateTime(long? timeStamp)
        {
            if (timeStamp == null)
                return DateTime.UtcNow;

            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(timeStamp.Value);

            DateTime dateTime = dateTimeOffset.LocalDateTime;
            DateTime utcDateTime = dateTimeOffset.UtcDateTime;

            return utcDateTime;
        }

        private async Task<string> ValidateIdTokenReturnEmail(string idToken)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string>() { "814206436687-df03frm3q9ml8ue57cnliu2q3m90e3ov.apps.googleusercontent.com" } // your OAuth client id
            };

            string token = idToken.Replace("Bearer ", "");

            var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);

            return payload.Email;
        }

        public async Task<bool> IsWorkingDayAsync(string userEmail, DateTime date, string accessToken)
        {

            var credentialg = GoogleCredential.FromAccessToken(accessToken);

            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentialg,
                ApplicationName = _configuration["ProjectName"]
            });



            var request = new FreeBusyRequest
            {
                TimeMin = date.Date,
                TimeMax = date.Date.AddDays(1),
                TimeZone = "Europe/Berlin", // Or your relevant time zone
                Items = new List<FreeBusyRequestItem>
            {
                new FreeBusyRequestItem { Id = userEmail }
            }
            };

            var query = service.Freebusy.Query(request);
            var response = await query.ExecuteAsync();

            // Check if the user has any busy events on that date
            if (response.Calendars.ContainsKey(userEmail))
            {
                var busyTimes = response.Calendars[userEmail].Busy;
                return busyTimes != null && busyTimes.Count > 0;
            }

            return false;
        }

        private async Task<DateTime> GetNextWorkingDayAsync(string userEmail, DateTime date, string accessToken)
        {
            // Skip Saturday and Sunday
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return await GetNextWorkingDayAsync(userEmail, date.AddDays(1), accessToken);

            var isWorkingDay = await IsWorkingDayAsync(userEmail, date, accessToken);
            if (isWorkingDay)
                return date;

            return await GetNextWorkingDayAsync(userEmail, date.AddDays(1), accessToken);
        }

        private async Task<(UpdateEvent, SendEmailModel, DateTime, DateTime)> FindNextAvailableSlotAndRescheduleAsync(string userEmail, DateTime eventStartDateTime, DateTime eventEndDateTime, string accessToken, string uidCalendarId)
        {
            var userDetails = UserSyncFileWriter.GetUserByEmail(userEmail);
            var nextWorkingDate = await GetNextWorkingDayAsync(userEmail, eventEndDateTime, accessToken);

            TimeSpan durationOfMeeting = eventEndDateTime - eventStartDateTime;

            var slotStart = nextWorkingDate.Date + userDetails.StartTime.Value;
            var slotEnd = nextWorkingDate.Date + userDetails.EndTime.Value;

            var nextAvailableTimeSlot = await _googleCalendarService.FindAvailableSlot(slotStart, slotEnd, durationOfMeeting, accessToken);

            if (nextAvailableTimeSlot.HasValue)
            {
                var eventObj = await _googleCalendarService.GetEventById(uidCalendarId, accessToken);
                if (eventObj != null)
                {
                    var newStart = nextAvailableTimeSlot.Value;
                    var newEnd = newStart.AddMinutes(durationOfMeeting.TotalMinutes);

                    eventObj.Start.DateTime = newStart;
                    eventObj.End.DateTime = newEnd;

                    var updateEventModel = new UpdateEvent()
                    {
                        EventId = uidCalendarId,
                        EventObj = eventObj,
                        Attendees = eventObj.Attendees.Select(x => x.Email).ToList()
                    };

                    var sendEmailModel = new SendEmailModel()
                    {
                        To = eventObj.Organizer.Email,
                        Subject = $"Reschedule: {eventObj.Summary}",
                        Message = $"Dear {eventObj.Organizer.DisplayName ?? "Organizer"},\n" +
                                  $"I wanted to propose rescheduling the meeting currently set for: {eventObj.Summary} " +
                                  $"on {newStart:MMMM d 'at' h:mm tt} - {newEnd:MMMM d 'at' h:mm tt}.\nThanks!"
                    };

                    // Send email or reschedule logic
                    // await SendEmail(sendEmailModel);
                    // await _googleCalendarService.Reschedule(updateEventModel, new CancellationToken());

                    return (updateEventModel, sendEmailModel, nextAvailableTimeSlot.Value, nextAvailableTimeSlot.Value.AddMinutes(durationOfMeeting.TotalMinutes)); // Successfully rescheduled
                }
            }

            // Try the next day
            return await FindNextAvailableSlotAndRescheduleAsync(userEmail, eventStartDateTime.AddDays(1), eventEndDateTime.AddDays(1), accessToken, uidCalendarId);
        }

        private async Task<EmailModelSync> GetEmailModel(string? mailSnippet, string fromHeader, long? internalDate, string messageId, string accessToken)
        {
            if (string.IsNullOrEmpty(mailSnippet))
                return new EmailModelSync();

            var emailSync = new EmailModelSync();
            var dateTime = GetDateTimeFromStringUsingRecognizers(mailSnippet);
            if (dateTime.HasValue)
            {
                var list = await _googleCalendarService.GetConflictingEvents(dateTime.Value, dateTime.Value.AddMinutes(30), accessToken, "");
                if (list == null || !list.Any())
                {
                    var calendarEvent = new CalendarEvent()
                    {
                        Attendees = new string[] { fromHeader },
                        Description = "Meeting Call set up by LLM Scheduler",
                        StartTime = dateTime.Value,
                        EndTime = dateTime.Value.AddMinutes(30),
                        Location = "Berlin",
                        Summary = "Meeting Call set up by LLM Scheduler"
                    };

                    //  bool isEventScheduled = await _googleCalendarService.Schedule(calendarEvent, new CancellationToken());

                    emailSync.ReceivedAt = ConvertLongDateTimeToDateTime(internalDate);
                    emailSync.From = fromHeader;
                    emailSync.IsMeetingInvite = true;
                    emailSync.Snippet = mailSnippet;
                    emailSync.CalendarEvent = calendarEvent;
                    emailSync.MessageId = messageId;

                    //var emailSync = new EmailModelSync()
                    //{
                    //    ReceivedAt = ConvertLongDateTimeToDateTime(internalDate),
                    //    From = fromHeader,
                    //    IsMeetingInvite = true,
                    //    Snippet = mailSnippet,
                    //    CalendarEvent = calendarEvent,
                    //    MessageId = messageId
                    //};
                }
            }

            return emailSync;
        }

        #endregion
    }
}
