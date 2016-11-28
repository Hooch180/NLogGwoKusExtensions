using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;
using Xunit;
using NSubstitute;
using NLogGwoKusExtensions;
using RestSharp;

namespace NLogGwoKusExtensions.Tests
{

	public class SlackBotTargetTests
	{
		private string apiKey;
		private string goodResponseJson;
		private string badResponseJson;
		private IRestResponse goodResponse;
		private IRestResponse badResponse;
		private IRestResponse notFoundResponse;

		public SlackBotTargetTests()
		{
			apiKey = "botApiKey";

			JObject goodResponseJObject = new JObject
			{
				["ok"] = true
			};
			goodResponseJson = goodResponseJObject.ToString();
			goodResponse = new RestResponse
			{
				StatusCode = HttpStatusCode.OK,
				Content = goodResponseJson
			};

			JObject badResponseJObject = new JObject
			{
				["ok"] = false,
				["error"] = "test_error"
			};
			badResponseJson = badResponseJObject.ToString();
			badResponse = new RestResponse
			{
				StatusCode = HttpStatusCode.OK,
				Content = badResponseJson
			};

			notFoundResponse = new RestResponse
			{
				StatusCode = HttpStatusCode.NotFound
			};
		}

		[Fact]
		public void ThrowsNullOnNullRestClient()
		{
			SlackBotTarget slackTarget = new SlackBotTarget();
			slackTarget.ThrowExceptions = true;
			Assert.Throws<ArgumentNullException>(() => slackTarget.Write(new LogEventInfo(), null));
		}

		[Fact]
		public void DoesNotThrowOnNullLogEventInfo()
		{
			SlackBotTarget slackTarget = new SlackBotTarget();
			slackTarget.ThrowExceptions = true;
			slackTarget.Write(null, null);
		}

		[Fact]
		public void RequiredParametersPresentRequest()
		{
			SlackBotTarget slackTarget = new SlackBotTarget();
			slackTarget.ApiKey = "testKey";
			slackTarget.Channel = "testChannel";
			
			var restClient = Substitute.For<IRestClient>();
			restClient.Execute(Arg.Any<IRestRequest>()).Returns(r =>
			{
				IRestRequest request = (IRestRequest) r[0];
				Assert.Equal(Method.GET, request.Method);
				Assert.Equal(1, request.Parameters.Count(p => p.Name == "token" && (string)p.Value == "testKey"));
				Assert.Equal(1, request.Parameters.Count(p => p.Name == "channel" && (string)p.Value == "testChannel"));
				Assert.Equal(1, request.Parameters.Count(p => p.Name == "text" && (string)p.Value == "testMessage"));
				
				return goodResponse;
			});

			slackTarget.Write(new LogEventInfo(LogLevel.Debug, "Test", "testMessage"), restClient);
		}

		[Fact]
		public void AllParametersPresentRequest()
		{
			SlackBotTarget slackTarget = new SlackBotTarget();
			slackTarget.ApiKey = "testKey";
			slackTarget.Channel = "testChannel";
			slackTarget.UserName = "testUserName";
			slackTarget.Emoji = ":testEmoji:";

			var restClient = Substitute.For<IRestClient>();
			restClient.Execute(Arg.Any<IRestRequest>()).Returns(r =>
			{
				IRestRequest request = (IRestRequest)r[0];
				Assert.Equal(Method.GET, request.Method);
				Assert.Equal(1, request.Parameters.Count(p => p.Name == "token" && (string)p.Value == "testKey"));
				Assert.Equal(1, request.Parameters.Count(p => p.Name == "channel" && (string)p.Value == "testChannel"));
				Assert.Equal(1, request.Parameters.Count(p => p.Name == "text" && (string)p.Value == "testMessage"));
				Assert.Equal(1, request.Parameters.Count(p => p.Name == "username" && (string)p.Value == "testUserName"));
				Assert.Equal(1, request.Parameters.Count(p => p.Name == "icon_emoji" && (string)p.Value == ":testEmoji:"));

				return goodResponse;
			});

			slackTarget.Write(new LogEventInfo(LogLevel.Debug, "Test", "testMessage"), restClient);
		}

		[Fact]
		public void IconUrlNotSetWhenEmojiSet()
		{
			SlackBotTarget slackTarget = new SlackBotTarget();
			slackTarget.ApiKey = "testKey";
			slackTarget.Channel = "testChannel";
			slackTarget.Emoji = ":testEmoji:";
			slackTarget.IconUrl = "testIcon";

			var restClient = Substitute.For<IRestClient>();
			restClient.Execute(Arg.Any<IRestRequest>()).Returns(r =>
			{
				IRestRequest request = (IRestRequest)r[0];

				Assert.Equal(0, request.Parameters.Count(p => p.Name == "icon_url"));
				Assert.Equal(1, request.Parameters.Count(p => p.Name == "icon_emoji" && (string)p.Value == ":testEmoji:"));

				return goodResponse;
			});

			slackTarget.Write(new LogEventInfo(LogLevel.Debug, "Test", "testMessage"), restClient);
		}

		[Fact]
		public void IconUrlSetWhenEmojiNotSet()
		{
			SlackBotTarget slackTarget = new SlackBotTarget();
			slackTarget.ApiKey = "testKey";
			slackTarget.Channel = "testChannel";
			slackTarget.IconUrl = "testIcon";

			var restClient = Substitute.For<IRestClient>();
			restClient.Execute(Arg.Any<IRestRequest>()).Returns(r =>
			{
				IRestRequest request = (IRestRequest)r[0];

				Assert.Equal(0, request.Parameters.Count(p => p.Name == "icon_url"));

				return goodResponse;
			});

			slackTarget.Write(new LogEventInfo(LogLevel.Debug, "Test", "testMessage"), restClient);
		}

		[Fact]
		public void ThrowsExceptionWitErrorFromApi()
		{
			SlackBotTarget slackTarget = new SlackBotTarget();
			slackTarget.ApiKey = "testKey";
			slackTarget.Channel = "testChannel";
			slackTarget.IconUrl = "testIcon";
			slackTarget.ThrowExceptions = true;

			var restClient = Substitute.For<IRestClient>();
			restClient.Execute(Arg.Any<IRestRequest>()).Returns(r =>
			{
				IRestRequest request = (IRestRequest)r[0];
				return badResponse;
			});

			Assert.Throws<NLogRuntimeException>(() => slackTarget.Write(new LogEventInfo(LogLevel.Debug, "Test", "testMessage"), restClient));

			try
			{
				slackTarget.Write(new LogEventInfo(LogLevel.Debug, "Test", "testMessage"), restClient);
			}
			catch (Exception ex)
			{
				Assert.Contains("test_error", ex.Message);
			}
			

		}
	}
}
