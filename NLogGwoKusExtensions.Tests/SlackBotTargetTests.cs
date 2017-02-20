using System;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using NLog;
using Xunit;
using NSubstitute;
using RestSharp;

namespace NLogGwoKusExtensions.Tests
{

	public class SlackBotTargetTests
	{
		private readonly string apiKey;

		private readonly IRestResponse goodResponse;
		private readonly IRestResponse badResponse;
		private readonly IRestResponse notFoundResponse;

		public SlackBotTargetTests()
		{
			apiKey = "botApiKey";

			var goodResponseJObject = new JObject
			{
				["ok"] = true
			};
			var goodResponseJson = goodResponseJObject.ToString();
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
			var badResponseJson = badResponseJObject.ToString();
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
			var slackTarget = new SlackBotTarget
			{
				ThrowExceptions = true
			};

			Assert.Throws<ArgumentNullException>(() => slackTarget.Write(new LogEventInfo(), null));
		}

		[Fact]
		public void DoesNotThrowOnNullLogEventInfo()
		{
			var slackTarget = new SlackBotTarget
			{
				ThrowExceptions = true
			};

			slackTarget.Write(null, null);
		}

		[Fact]
		public void RequiredParametersPresentRequest()
		{
			var slackTarget = new SlackBotTarget
			{
				ApiKey = "testKey",
				Channel = "testChannel"
			};

			var restClient = Substitute.For<IRestClient>();
			restClient.Execute(Arg.Any<IRestRequest>()).Returns(r =>
			{
				IRestRequest request = (IRestRequest)r[0];
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
			var slackTarget = new SlackBotTarget
			{
				ApiKey = "testKey",
				Channel = "testChannel",
				UserName = "testUserName",
				Emoji = ":testEmoji:"
			};

			var restClient = Substitute.For<IRestClient>();
			restClient.Execute(Arg.Any<IRestRequest>()).Returns(r =>
			{
				var request = (IRestRequest)r[0];
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
			var slackTarget = new SlackBotTarget
			{
				ApiKey = "testKey",
				Channel = "testChannel",
				Emoji = ":testEmoji:",
				IconUrl = "testIcon"
			};

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
			var slackTarget = new SlackBotTarget
			{
				ApiKey = "testKey",
				Channel = "testChannel",
				IconUrl = "testIcon"
			};

			var restClient = Substitute.For<IRestClient>();
			restClient.Execute(Arg.Any<IRestRequest>()).Returns(r =>
			{
				var request = (IRestRequest)r[0];

				Assert.Equal(0, request.Parameters.Count(p => p.Name == "icon_url"));

				return goodResponse;
			});

			slackTarget.Write(new LogEventInfo(LogLevel.Debug, "Test", "testMessage"), restClient);
		}

		[Fact]
		public void ThrowsExceptionWitErrorFromApi()
		{
			var slackTarget = new SlackBotTarget
			{
				ApiKey = "testKey",
				Channel = "testChannel",
				IconUrl = "testIcon",
				ThrowExceptions = true
			};

			var restClient = Substitute.For<IRestClient>();
			restClient.Execute(Arg.Any<IRestRequest>()).Returns(r =>
			{
				var request = (IRestRequest)r[0];
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
