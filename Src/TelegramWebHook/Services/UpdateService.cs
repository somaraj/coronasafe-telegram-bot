using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CoreHtmlToImage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using TelegramWebHook.Helpers;
using TelegramWebHook.Models;

namespace TelegramWebHook.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly IBotService _botService;
        private readonly ILogger<UpdateService> _logger;

        public UpdateService(IBotService botService, ILogger<UpdateService> logger)
        {
            _botService = botService;
            _logger = logger;
        }

        public async Task EchoAsync(Update update)
        {
            if (update.Type != UpdateType.Message)
                return;

            var message = update.Message;
            var options = new StringBuilder();
            options.AppendLine("Reply with a number at any time to get the latest information on the topic:\n");
            options.AppendLine("<b>1: District wise report</b>");
            options.AppendLine("<b>2: Symptoms</b>");
            options.AppendLine("<b>3: Precautions and Preventive Measures</b>");
            options.AppendLine("<b>4: Helpline</b>");

            if (message.Type != MessageType.Text)
            {
                await _botService.Client.SendTextMessageAsync(message.Chat.Id, $"<i><b>Sorry i am unable to understand your query?</b></i>\n\n{options.ToString()}", ParseMode.Html);
                return;
            }

            var invalidInput = $"Sorry I am an automated system and didn't understand your reply.\n\n{options.ToString()}";

            switch (message.Text.ToLower().Trim())
            {
                case "/start":
                    var name = message.Chat.FirstName;
                    if (!string.IsNullOrEmpty(message.Chat.LastName))
                        name += $" {message.Chat.LastName}";

                    var welcomeMessage = new StringBuilder();
                    welcomeMessage.AppendLine($"Hello {name}\n");
                    welcomeMessage.AppendLine(options.ToString());
                    await _botService.Client.SendTextMessageAsync(message.Chat.Id, welcomeMessage.ToString(), ParseMode.Html);
                    break;
                case "1":
                    {
                        string json = (new WebClient()).DownloadString("https://volunteer.coronasafe.network/api/reports");
                        var jsonObj = JsonConvert.DeserializeObject<JsonReportModel>(json);
                        var sb = new StringBuilder();
                        sb.AppendLine("<!DOCTYPE html>");
                        sb.AppendLine("<html lang=\"en\">");
                        sb.AppendLine("<head>");
                        sb.AppendLine("<meta charset=\"UTF-8\">");
                        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width,initial-scale=1.0\">");
                        sb.AppendLine("<link rel=\"stylesheet\" href=\"https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css\">");
                        sb.AppendLine("<style>th{white-space:nowrap;}</style>");
                        sb.AppendLine("</head>");

                        sb.AppendLine("<table class=\"table table-bordered table-stripped\">");
                        sb.AppendLine("<thead>");
                        sb.AppendLine("<tr>");
                        sb.AppendLine("<th>District</th>");
                        sb.AppendLine("<th>Confirmed</th>");
                        sb.AppendLine("<th>Recovered</th>");
                        sb.AppendLine("<th>Deaths</th>");
                        sb.AppendLine("<th>Under Observation</th>");
                        sb.AppendLine("<th>Hospital Isolation</th>");
                        sb.AppendLine("<th>Home Isolation</th>");
                        sb.AppendLine("<th>Hospitalized Today</th>");
                        sb.AppendLine("</tr>");
                        sb.AppendLine("</thead>");
                        sb.AppendLine("<tbody>");
                        long totalCoronaPositive = 0;
                        long totalCuredDischarged = 0;
                        long totalDeaths = 0;
                        long totalUnderObservation = 0;
                        long totalTotalHospitalised = 0;
                        long totalUnderHomeIsolation = 0;
                        long totalHospitalisedToday = 0;

                        foreach (var item in jsonObj.Summary.OrderByDescending(x => x.Value.CoronaPositive))
                        {
                            totalCoronaPositive += item.Value.CoronaPositive.ToLong();
                            totalCuredDischarged += item.Value.CuredDischarged.ToLong();
                            totalDeaths += item.Value.Deaths.ToLong();
                            totalUnderObservation += item.Value.UnderObservation.ToLong();
                            totalTotalHospitalised += item.Value.TotalHospitalised.ToLong();
                            totalUnderHomeIsolation += item.Value.UnderHomeIsolation.ToLong();
                            totalHospitalisedToday += item.Value.HospitalisedToday.ToLong();

                            sb.AppendLine("<tr>");
                            sb.AppendLine($"<td>{item.Key}</td>");
                            sb.AppendLine($"<td>{item.Value.CoronaPositive}</td>");
                            sb.AppendLine($"<td>{item.Value.CuredDischarged}</td>");
                            sb.AppendLine($"<td>{item.Value.Deaths}</td>");
                            sb.AppendLine($"<td>{item.Value.UnderObservation}</td>");
                            sb.AppendLine($"<td>{item.Value.TotalHospitalised}</td>");
                            sb.AppendLine($"<td>{item.Value.UnderHomeIsolation}</td>");
                            sb.AppendLine($"<td>{item.Value.HospitalisedToday}</td>");
                            sb.AppendLine("</tr>");
                        }
                        //Set total
                        sb.AppendLine("<tfooter>");
                        sb.AppendLine("<tr>");
                        sb.AppendLine("<td>Total</td>");
                        sb.AppendLine($"<td>{totalCoronaPositive}</td>");
                        sb.AppendLine($"<td>{totalCuredDischarged}</td>");
                        sb.AppendLine($"<td>{totalDeaths}</td>");
                        sb.AppendLine($"<td>{totalUnderObservation}</td>");
                        sb.AppendLine($"<td>{totalTotalHospitalised}</td>");
                        sb.AppendLine($"<td>{totalUnderHomeIsolation}</td>");
                        sb.AppendLine($"<td>{totalHospitalisedToday}</td>");
                        sb.AppendLine("</tr>");
                        sb.AppendLine("</tfooter>");

                        sb.AppendLine("</tbody>");
                        sb.AppendLine("</table>");

                        var converter = new HtmlConverter();
                        var html = sb.ToString();
                        var bytes = converter.FromHtmlString(html);
                        System.IO.File.WriteAllBytes("district_wise_report.jpg", bytes);

                        using var stream = System.IO.File.Open("district_wise_report.jpg", FileMode.Open);
                        InputOnlineFile fts = new InputOnlineFile(stream);
                        await _botService.Client.SendPhotoAsync(message.Chat.Id, fts, "https://keralamap.coronasafe.network/");
                    }
                    break;
                case "2":
                    {
                        var symptoms = new StringBuilder();
                        symptoms.AppendLine("The most common symptoms of COVID-19 are fever, tiredness, and dry cough. They are usually mild and begin gradually. They appear around 2 - 14 days with average symptoms showing up after 5 days of being infected.\n");
                        symptoms.AppendLine("Some symptoms appear less common than others. The major symptoms of COVID-19 based on how common it appears in the reported cases are \n");
                        symptoms.AppendLine("<b>Fever</b>");
                        symptoms.AppendLine("<b>Dry Cough</b>");
                        symptoms.AppendLine("<b>Fatigue</b>");
                        symptoms.AppendLine("<b>Sputum Production</b>");
                        symptoms.AppendLine("<b>Shortness of Breath</b>");
                        symptoms.AppendLine("<b>Muscle Pain or Joint Pain</b>");
                        symptoms.AppendLine("<b>Sore Throat</b>\n");
                        symptoms.AppendLine("Most of the symptoms overlap with those of flu and common cold. COVID-19 rarely produces a runny nose.");
                        await _botService.Client.SendTextMessageAsync(message.Chat.Id, symptoms.ToString(), ParseMode.Html);
                    }
                    break;
                case "3":
                    await _botService.Client.SendTextMessageAsync(message.Chat.Id, "Please visit <b>https://www.coronasafe.in/precautions</b>", ParseMode.Html);
                    break;
                case "4":
                    {
                        using var stream = System.IO.File.Open("covid_helpline_kl.jpg", FileMode.Open);
                        InputOnlineFile fts = new InputOnlineFile(stream);
                        await _botService.Client.SendPhotoAsync(message.Chat.Id, fts, "https://kerala.gov.in/helpline");
                    }
                    break;
                default:
                    await _botService.Client.SendTextMessageAsync(message.Chat.Id, invalidInput, ParseMode.Html);
                    break;
            }
        }
    }
}
