using System;
using System.Net;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using RestSharp;

namespace NLogGwoKusExtensions
{
	[Target("Pushbullet")]
	public sealed class PushbulletTarget : TargetWithLayout
	{
		/// <summary>
		/// User token found in user profile setting.
		/// </summary>
		[RequiredParameter]
		public string ApiToken { get; set; }

		/// <summary>
		/// Title of pushed messages 
		/// </summary>
		[RequiredParameter]
		public string MessagesTitle { get; set; }

		/// <summary>
		/// Optional channel token without "@" at the begining.
		/// If no channel is set, notification is send to all users devices.
		/// </summary>
		public string ChannelTag { get; set; }

		/// <summary>
		/// Gets or sets if exceptions will be rethrown.
		/// Set it to true if ElasticSearchTarget target is used within FallbackGroup target
		/// (https://github.com/NLog/NLog/wiki/FallbackGroup-target).
		/// </summary>
		public bool ThrowExceptions { get; set; }

		private IRestClient restClient;

		/// <summary>
		/// Creates instance of <see cref="PushbulletTarget"/> class.
		/// </summary>
		public PushbulletTarget()
		{
			ApiToken = null;
			ChannelTag = null;
		}

		/// <summary>
		/// Method invoked by NLog to write log message.
		/// </summary>
		/// <param name="logEvent">Info to log.</param>
		protected override void Write(LogEventInfo logEvent)
		{
			try
			{
				if (restClient == null)
				{
					var pushbulletApiUrl = new Uri($@"https://api.pushbullet.com/v2/");
					restClient = new RestClient(pushbulletApiUrl);
				}
			}
			catch (Exception)
			{
				if (ThrowExceptions)
					throw;
			}

			PushToChannel(logEvent, restClient);
		}

		/// <summary>
		/// Pushesh log messagee to Pushbulleet channel or user.
		/// </summary>
		/// <param name="logEventInfo"></param>
		public void PushToChannel(LogEventInfo logEvent, IRestClient customRestClient)
		{
			try
			{
				if (logEvent == null)
					return;

				if (customRestClient == null)
					throw new ArgumentNullException(nameof(customRestClient));

				var request = new RestRequest(@"pushes", Method.POST);
				request.AddHeader("Access-Token", ApiToken);
				request.AddParameter("type", "note", ParameterType.GetOrPost);
				request.AddParameter("title", MessagesTitle, ParameterType.GetOrPost);
				request.AddParameter("body", logEvent.Message, ParameterType.GetOrPost);

				if (!string.IsNullOrEmpty(ChannelTag))
					request.AddParameter("channel_tag", ChannelTag, ParameterType.GetOrPost);

				var response = customRestClient.Execute(request);
				if (response.StatusCode != HttpStatusCode.OK)
					throw new NLogRuntimeException($"Error response code from Pushbullet. Code: {response.StatusCode}.");

				var responseParsed = JObject.Parse(response.Content);
				if ((bool)responseParsed["ok"] != true)
				{
					string message = $"Error returned from Pushbullet Api: {responseParsed["error"]}";
					throw new NLogRuntimeException(message);
				}
			}
			catch (Exception)
			{
				if (ThrowExceptions)
					throw;
			}
		}
	}
}