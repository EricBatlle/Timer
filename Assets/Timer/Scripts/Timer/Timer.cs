using System;

namespace TimerModule
{
	public class Timer
	{
		private const string DefaultId = "TimerDefaultId";
		private TimerState state;
		private DateTime pauseEndTime;

		public string Id { get; }
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
		public DateTime StopTime { get; set; }
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

		public Timer(string id, TimeSpan duration)
		{
			this.Id = id;
			this.Duration = duration;
		}

		public Timer(TimeSpan duration) : this(DefaultId, duration)
		{
		}

		public TimeSpan GetLastPauseTime => PauseEndTime - PauseStartTime;
	}
}
