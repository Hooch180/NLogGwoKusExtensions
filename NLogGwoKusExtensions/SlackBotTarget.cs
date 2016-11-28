using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using RestSharp;

namespace NLogGwoKusExtensions
{
	[Target("SlackBotTarget")]
	public sealed class SlackBotTarget : TargetWithLayout
	{
		/// <summary>
		/// Slack domain. Only part before ".slack.com".
		/// </summary>
		[RequiredParameter]
		public string SlackDomain
		{
			get { return slackDomain; }
			set
			{
				slackDomain = value;
				restClient = null;
			}
		}

		/// <summary>
		/// ApiKey of bot.
		/// </summary>
		[RequiredParameter]
		public string ApiKey { get; set; }

		/// <summary>
		/// Channel name without leadhing '#' or Channel Id.
		/// </summary>
		[RequiredParameter]
		public string Channel { get; set; }

		/// <summary>
		/// Bot username. "bot" if null;
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// Post emoji. Default if null.
		/// </summary>
		public string Emoji { get; set; }

		/// <summary>
		/// Icon url. Used only if Emoji is null.
		/// </summary>
		public string IconUrl { get; set; }

		/// <summary>
		/// Gets or sets if exceptions will be rethrown.
		/// Set it to true if ElasticSearchTarget target is used within FallbackGroup target (https://github.com/NLog/NLog/wiki/FallbackGroup-target).
		/// </summary>
		public bool ThrowExceptions { get; set; }

		private RestClient restClient;
		private string slackDomain;

		public SlackBotTarget()
		{
			UserName = null;
			Emoji = null;
			IconUrl = null;
			ThrowExceptions = false;
		}

		protected override void Write(LogEventInfo logEvent)
		{
			try
			{
				if (restClient == null)
				{
					var fullSlackApiUrl = new Uri($@"http://{slackDomain}.slack.com/api/");
					restClient = new RestClient(fullSlackApiUrl);
				}

				var request = new RestRequest(@"chat.postMessage", Method.GET);
				request.AddParameter("token", ApiKey);
				request.AddParameter("channel", Channel);
				request.AddParameter("text", logEvent.Message);

				if (UserName != null)
					request.AddParameter("username", UserName);

				if (Emoji != null)
					request.AddParameter("icon_emoji", Emoji);

				if (IconUrl != null && Emoji == null)
					request.AddParameter("icon_url", IconUrl);

				var response = restClient.Execute(request);
				if (response.StatusCode != HttpStatusCode.OK)
					throw new NLogRuntimeException($"Error response code from Slack. Code: {response.StatusCode}.");

				JObject responseParsed = JObject.Parse(response.Content);
				if ((bool)responseParsed["ok"] != true)
				{
					string message = $"Error returned from SlackApi: {responseParsed["error"]}";
					throw new NLogRuntimeException(message);
				}
			}
			catch (Exception)
			{
				if(ThrowExceptions)
					throw;
			}
		}
	}
}
