using NCrontab;

namespace CalendarAssistant.Services
{
    public class HostedService : BackgroundService
    {

        private CrontabSchedule _schedule;
        private DateTime _nextRun;

        private string Schedule => "*/60 * * * * *"; //Runs every 5 minutes


        public HostedService()
        {
            _schedule = CrontabSchedule.Parse(Schedule, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
            _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                var now = DateTime.Now;
                var nextrun = _schedule.GetNextOccurrence(now);
                if (now > _nextRun)
                {
                    Process();
                    _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                }
                await Task.Delay(5000, stoppingToken); //5 seconds delay
            }
            while (!stoppingToken.IsCancellationRequested);
        }

        private void Process()
        {
            Console.WriteLine("hello world" + DateTime.Now.ToString("F"));
        }
    }
    
}
