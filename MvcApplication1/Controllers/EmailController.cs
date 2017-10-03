using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using MvcApplication1.Models;
using MvcApplication1.Services;

namespace MvcApplication1.Controllers
{
    public class EmailController : ApiController
    {
        private EmailRepository emailListRepository;

        public EmailController()
        {
            emailListRepository = new EmailRepository();
        } 

        public Email[] Get()
        {
            Log.Append("GET Email requested. Access denied");
            return new Email[] { };
            //return emailListRepository.GetAllEmails();
        }

        [Route("Email/{paramTwo}/{paramOne}/{APIKey}/{useLINQ}")]
        public Email[] Get(string paramTwo, string paramOne, string APIKey, string useLINQ)
        {
            if (!Permissions.ValidAPIKey(APIKey)) return new Email[] { };
            

            Log.Append(String.Format("GET Email ({1}) - param: '{0}'", paramOne, paramTwo));
            return emailListRepository.GetAllEmails(paramTwo, paramOne, useLINQ == "1").ToArray();
        }

        [Route("Email/{paramTwo}/{paramOne}/{APIKey}")]
        public Email[] Get(string paramTwo, string paramOne, string APIKey)
        {
            if (!Permissions.ValidAPIKey(APIKey)) return new Email[] { };
            

            Log.Append(String.Format("GET Email ({1}) - param: '{0}'", paramOne, paramTwo));

            if (paramOne.Contains("="))
            {
                return emailListRepository.GetAllEmails(paramTwo, paramOne).ToArray();
            }

            return SearchAlgorithm.SearchDieNumber(paramOne).ToArray();
        }

        /// <summary>
        /// Update GETter
        /// </summary>
        /// <param name="paramOne"></param>
        /// <param name="APIKey"></param>
        /// <returns></returns>
        //[Route("Email/{paramOne}/{APIKey}")]
        [Route("Email/{paramOne}")]
        public void Get(string paramOne)
        {
            //if (!Permissions.ValidAPIKey(APIKey)) return new Email[] {};
            paramOne = AESGCM.SimpleDecryptWithPassword(paramOne, AESGCM.AES256Key);

            if (paramOne.ToLower() == "reset")
            {
                Log.Append("GET - Reset Email Sync Parameters");
                Global.isSyncing = false;
                Readiness.DeleteBlockerFile();
            }
            if (paramOne.ToLower() == "sync")
            {
                Log.Append("GET - Sync PST Files");
                PSTImporter.SyncPSTFiles();
            }
            if (paramOne.ToLower() == "validate")
            {
                Log.Append("GET - Validating email and file integrity");
                Task.Run(() => Global.ValidateMessages());
            }
            if (paramOne.ToLower() == "refresh")
            {
                Log.Append("GET - Refresh Email List requested");
                // Reload settings before getting emails
                Global.LoadSettings();
                EmailRepository.CacheInfo(Global.GetAllEmails().ToArray());
            }
            //return new Email[] { };
        }

        public HttpResponseMessage Post(Email email)
        {
            emailListRepository.SaveEmail(email);

            var response = Request.CreateResponse<Email>(HttpStatusCode.Created, email);

            return response;
        }
    }
}
