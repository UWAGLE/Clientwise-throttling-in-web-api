using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Service.WebAPI.Filter
{
    public class AuthorizeCustom : AuthorizeAttribute
    {
        const string key = "EF56137B67F832F07832F07175AB3EEF1A5BF3C65DD293C075AB3EEF1A";
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            try
            {
                if (actionContext.Request.Headers.Contains("x"))
                    return actionContext.Request.Headers.GetValues("x").FirstOrDefault() == key ? true : false;
                else
                    return false;

            }
            catch (Exception e)
            {
                //Log error
                return false;
            }
        }
    }
}