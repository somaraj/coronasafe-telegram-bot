using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CoreHtmlToImage;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using TelegramWebHook.Models;
using TelegramWebHook.Services;

namespace TelegramWebHook.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Route("api/[controller]")]
    public class UpdateController : ControllerBase
    {
        private readonly IUpdateService _updateService;

        public UpdateController(IUpdateService updateService)
        {
            _updateService = updateService;
        }

        public string Get()
        {
            return "Hello World!";
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            await _updateService.EchoAsync(update);
            return Ok();
        }
    }
}
