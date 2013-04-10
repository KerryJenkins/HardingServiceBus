using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ColorSender.Utilities;

namespace ColorSender.Controllers
{
    public class PublishController : ApiController
    {
        // GET api/publish/who/color
        public HttpResponseMessage Get(string who, string color)
        {
                var httpSb = new HttpServiceBus(serviceNamespace: "harding",
                                                issuerName: "owner",
                                                issuerSecret: "Q8Kf3NiSIzVxSAjzBpLZhYkRusN19lgbxToEi1B5toI=");

                var headers = new Dictionary<string, string>();
                headers.Add("who", who);

                httpSb.SendMessage("color", color, headers);

                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    new {status = string.Format("{0} sent to {1}!", color, who)});
        }
    }
}
