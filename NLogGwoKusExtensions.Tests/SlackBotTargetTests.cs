using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLogGwoKusExtensions.Tests
{

	public class SlackBotTargetTests
	{
		private string apiKey;

		public SlackBotTargetTests()
		{
			apiKey = "botApiKey";
		}

		[Fact]
		public void HelloWorld()
		{
			Assert.StartsWith("Hello", "Hello World!");
		}
	}
}
