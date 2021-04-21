using IncTrak.Data;
using IncTrak.FeedbackModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace IncTrak.Controllers
{
    [ApiController]
    public class FeedbackController : IncTrakController
    {
        public FeedbackController(IOptions<AppSettings> config) : base(config)
        {
        }

        [Route("api/feedback/message_types/")]
        public object GetMessageTypes()
        {
            try
            { 
                using (inctrak_feedbackContext context = new inctrak_feedbackContext(_options.Value))
                {
                    return (from m in context.MessageType
                            select new { Key = m.MessageTypePk, Name = m.MessageType1 }).ToArray();
                }
            }
            catch (Exception excp)
            {
                IncTrakErrors.LogError(_options.Value, GetLoginUser(), excp, "feedback types");
                return null;
            }
        }

        [Route("api/feedback/get_message/")]
        public object GetMessage()
        {
            try
            { 
                return new Feedback() { MessageTypeFk = 7 };
            }
            catch (Exception excp)
            {
                IncTrakErrors.LogError(_options.Value, GetLoginUser(), excp, "feedback message");
                return null;
            }
        }

        [Route("api/feedback/save_message/")]
        public ActionResult Post(Feedback feedBack)
        {
            try
            {
                StringValues header;
                Uri referrer = null;
                if (Request.Headers.TryGetValue("Referer", out header))
                    referrer = new Uri(header);

                if (referrer == null ||
                    (referrer.DnsSafeHost.EndsWith("inctrak.com") == false
#if DEBUG 
                    && referrer.DnsSafeHost != "localhost"
#endif
                    ))
                {
                    return Ok(new { success = false, message = "Invalid request" });
                }
                var incTrak = referrer.GetLeftPart(UriPartial.Authority);

                using (inctrak_feedbackContext context = new inctrak_feedbackContext(_options.Value))
                {
                    if (feedBack == null || (string.IsNullOrWhiteSpace(feedBack.EmailAddress) && string.IsNullOrWhiteSpace(feedBack.Name))
                         || (string.IsNullOrWhiteSpace(feedBack.Message) && string.IsNullOrWhiteSpace(feedBack.Subject)))
                    {
                        return Ok(new { success = false, message = "Please enter email or name and subject or message" });
                    }
                    else
                    {
                        string who = "";
                        if (string.IsNullOrWhiteSpace(feedBack.EmailAddress))
                        {
                            feedBack.EmailAddress = "none";
                        }
                        else
                        {
                            who = string.Format(", {0}", feedBack.EmailAddress);
                        }
                        if (string.IsNullOrWhiteSpace(feedBack.Name))
                        {
                            feedBack.Name = "none";
                        }
                        else
                        {
                            who = string.Format(", {0}", feedBack.Name);
                        }

                        if (string.IsNullOrWhiteSpace(feedBack.Message))
                            feedBack.Message = "none";
                        else if (string.IsNullOrWhiteSpace(feedBack.Subject))
                            feedBack.Subject = "none";
                        feedBack.Created = DateTime.Now;
                        feedBack.ClientData = GetClientInfo();
                        context.Feedback.Add(feedBack);
                        context.SaveChanges();
                        string messageType = context.MessageType.Single(s => s.MessageTypePk == feedBack.MessageTypeFk).MessageType1;
                        var response = Ok(new { success = true, message = string.Format("Thanks for the {0}{1}", messageType, who) });

                        // TODO response.Headers.Add("Access-Control-Allow-Origin", incTrak);

                        return response;
                    }
                }
            }
            catch(Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, GetLoginUser(), excp, "feedback save");
                return Ok(new { success = false, message = message });
            }
        }
    }
}