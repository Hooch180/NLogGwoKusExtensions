using System;
using System.Net;
using Newtonsoft.Json.Linq;
using NLog;
using RestSharp;
using Xunit;

namespace NLogGwoKusExtensions.Tests
{
	public class PushbulletTargetTests
	{
		private readonly string apiUrl;
		private readonly string apiToken;
		private readonly string channelTag;

		private readonly IRestResponse goodResponse;
		private readonly IRestResponse badResponse;
		private readonly IRestResponse notFoundResponse;

		public PushbulletTargetTests()
		{
			apiUrl = @"https://api.pushbullet.com/v2/";
			apiToken = "";
			channelTag = "";

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
		public void PushToChannel_RestClientIsOkAndChanellTagIsNull_DoesThrowException()
		{
			var pushbulletTarget = new PushbulletTarget { ApiToken = apiToken, MessagesTitle = "Title logger"};
			var logEvent = new LogEventInfo {Message = "error message :("};
			IRestClient restClient = new RestClient(apiUrl);

			pushbulletTarget.PushToChannel(logEvent, restClient);
		}

		[Fact]
		public void PushToChannel_RestClientIsOkAndChanellTagIsNotNull_DoesThrowException()
		{
			var pushbulletTarget = new PushbulletTarget { ApiToken = apiToken, ChannelTag = channelTag, MessagesTitle = "Title logger" };
			var logEvent = new LogEventInfo { Message = "error message :(" };
			IRestClient restClient = new RestClient(apiUrl);

			pushbulletTarget.PushToChannel(logEvent, restClient);
		}

		[Fact]
		public void PushToChannel_RestClientIsNull_hrowException()
		{
			var pushbulletTarget = new PushbulletTarget { ApiToken = apiToken, ChannelTag = channelTag, MessagesTitle = "Title logger", ThrowExceptions = true };
			var logEvent = new LogEventInfo { Message = "error message :(" };

			Assert.Throws<ArgumentNullException>(() => pushbulletTarget.PushToChannel(logEvent, null));
		}

		[Fact]
		public void PushToChannel_ThrowExceptionsIsFalseAndBedParameters_DoesThrowException()
		{
			var pushbulletTarget = new PushbulletTarget { ThrowExceptions = false};
			var logEvent = new LogEventInfo { Message = "error message :(" };
			IRestClient restClient = new RestClient(apiUrl);

			pushbulletTarget.PushToChannel(logEvent, restClient);
		}

		[Fact]
		public void PushToChannel_ThrowExceptionsIsTrueAndBedParameters_ThrowException()
		{
			var pushbulletTarget = new PushbulletTarget { ThrowExceptions = true };
			var logEvent = new LogEventInfo { Message = "error message :(" };
			IRestClient restClient = new RestClient(apiUrl);
			
			Assert.Throws<NLogRuntimeException>(() => pushbulletTarget.PushToChannel(logEvent, restClient));
		}
	}
}
