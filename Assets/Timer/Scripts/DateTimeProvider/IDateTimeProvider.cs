using System;

namespace TimerModule
{
	public interface IDateTimeProvider
	{
		DateTime UtcNow { get; }
	}
}