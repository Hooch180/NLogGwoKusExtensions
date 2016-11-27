using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace NLogGwoKusExtensions
{
	[Target("SlackBotTarget")]
	public sealed class SlackBotTarget : TargetWithLayout
	{
		[RequiredParameter]
		public string ApiKey { get; set; }

		[RequiredParameter]
		public string Channel { get; set; }

		public SlackBotTarget()
		{
			
		}

		protected override void Write(LogEventInfo logEvent)
		{
			base.Write(logEvent);
		}
	}
}
