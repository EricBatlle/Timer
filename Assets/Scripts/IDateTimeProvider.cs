using System;

public interface IDateTimeProvider
{
	DateTime UtcNow { get; }
}