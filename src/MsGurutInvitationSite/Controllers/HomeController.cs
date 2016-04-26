using System;
using System.Net.Mail;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace MsGurutInvitationSite.Controllers {
	public class HomeController : Controller {
		
		[HttpGet]
		public IActionResult Index() {
			return View(new FrontPageViewModel());
		}

		[HttpPost, ActionName("Index")]
		public IActionResult DoInvite(string email) {
			MailAddress m;

			try {
				m = new MailAddress(email);
			}
			catch {
				return View(new FrontPageViewModel() {Message = "Invalid email address", Success = false});
			}

			string error = InviteToSlack(m.ToString());
			if (error != null) {
				return View(new FrontPageViewModel() {Message = error, Success = false});
			}

			return View(new FrontPageViewModel() {Message = "Invitation sent to " + m.ToString() + "!", Success = true });
		}

		private IRestResponse res;
		// Returns an error message or null if success
		private string InviteToSlack(string email) {
			RestClient rc = new RestClient("https://msgurut.slack.com/api/");
			var rr = new RestRequest("users.admin.invite", Method.POST);
			rr.AddParameter("email", email, ParameterType.GetOrPost);
			rr.AddParameter("token", GetSlackToken(), ParameterType.GetOrPost);
			rr.AddParameter("set_active", "true", ParameterType.GetOrPost);

			this.res = rc.Execute(rr);
			try {
				JObject reply = JObject.Parse(res.Content);
				if (reply["ok"].Value<string>() == "True") {
					return null;
				}
				return "Slack API error: " + reply["error"].Value<string>();
			}
			catch {
				return "Unknown Slack error.";
			}
		}

		private object GetSlackToken() {
			return Environment.GetEnvironmentVariable("SlackApiKey");
		}

		public IActionResult Error() {
			return View();
		}
	}

	public class FrontPageViewModel {
		public bool Success { get; set; }
		public string Message { get; set; }
	}
}
