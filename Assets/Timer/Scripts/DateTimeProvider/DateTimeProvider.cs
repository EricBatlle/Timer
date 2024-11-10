using System;

namespace TimerModule
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime UtcNow => DateTime.UtcNow;
	}
}
