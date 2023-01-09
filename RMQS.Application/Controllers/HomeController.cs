using Microsoft.AspNetCore.Mvc;
using RMQS.Application.Models;
using RMQS.Application.Queue;
using System.Diagnostics;

namespace RMQS.Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly IQueuePublisher _publisher;

        public HomeController(IQueuePublisher publisher)
        {
            _publisher = publisher;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public JsonResult SendMessage([FromBody] MessageModel model)
        {
            switch (model.MessageType)
            {
                case "A":
                    {
                        var message = new MessageA { Comment = model.Comment };
                        _publisher.Publish(message, model.WithException);
                    }
                    break;
                case "B":
                    {
                        var message = new MessageB { Comment = model.Comment };
                        _publisher.Publish(message, model.WithException);
                    }
                    break;
                case "C":
                    {
                        var message = new MessageC { Comment = model.Comment };
                        _publisher.Publish(message, model.WithException);
                    }
                    break;
            }

            return new JsonResult(new { message = "ok" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}