using System;
public enum TimerState
{
	Default,
	Running,
	Paused,
	Freeze,
}
public class Timer
{
	private TimerState state;
	private DateTime pauseEndTime;

	public TimerState State
	{
		get => state;
		set
		{
			PreviousState = State;
			state = value;
		}
	}
	public TimerState PreviousState { get; set; } = TimerState.Default;
	public TimeSpan Duration { get; set; }
	public DateTime StartTime { get; set; }
	public DateTime PauseStartTime { get; set; }
	public DateTime PauseEndTime
	{
		get => pauseEndTime;
		set
		{
			pauseEndTime = value;
			TotalPausedTime += value - PauseStartTime;
		}
	}
	
	public TimeSpan FreezeDuration { get; set; }
	public DateTime FreezeStartTime { get; set; }

	public TimeSpan TotalPausedTime = TimeSpan.Zero;
	public TimeSpan TotalFreezedTime = TimeSpan.Zero;
	public TimeSpan TotalPausedDuringFreezedTime = TimeSpan.Zero;

	public Timer(TimeSpan duration)
	{
		this.Duration = duration;
	}

	public TimeSpan GetLastPauseTime => PauseEndTime - PauseStartTime;
}
