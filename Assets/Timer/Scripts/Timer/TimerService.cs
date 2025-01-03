using System;

namespace TimerModule
{
	public class TimerService
	{
		public event Action<Timer> TimerStateChanged;

		private readonly IDateTimeProvider dateTimeProvider;

		public TimerService(IDateTimeProvider dateTimeProvider)
		{
			this.dateTimeProvider = dateTimeProvider;
		}

		public void ResetTimer(ref Timer timer)
		{
			timer.Reset();
			TimerStateChanged?.Invoke(timer);
		}

		public void StartTimer(Timer timer)
		{
			StartTimer(timer, timer.Duration);
		}

		public void StartTimer(Timer timer, TimeSpan duration)
		{
			if (timer.State == TimerState.Running)
			{
				return;
			}

			timer.Duration = duration;
			timer.FreezeDuration = TimeSpan.Zero;
			timer.State = TimerState.Running;
			timer.StartTime = dateTimeProvider.UtcNow;
			TimerStateChanged?.Invoke(timer);
		}

		public void StopTimer(Timer timer)
		{
			if (timer.State == TimerState.Stopped)
			{
				return;
			}

			timer.State = TimerState.Stopped;
			timer.StopTime = dateTimeProvider.UtcNow;
			TimerStateChanged?.Invoke(timer);
		}

		public void PauseTimer(Timer timer)
		{
			if (timer.State == TimerState.Paused || timer.State == TimerState.Stopped)
			{
				return;
			}

			timer.State = TimerState.Paused;
			timer.PauseStartTime = dateTimeProvider.UtcNow;
			TimerStateChanged?.Invoke(timer);
		}

		public void ResumeTimer(Timer timer)
		{
			if (timer.State != TimerState.Paused)
			{
				return;
			}

			timer.State = timer.PreviousState;
			timer.PauseEndTime = dateTimeProvider.UtcNow;
			if (timer.State == TimerState.Frozen)
			{
				timer.TotalPausedDuringFreezedTime += timer.GetLastPauseTime;
			}
			TimerStateChanged?.Invoke(timer);
		}

		public void FreezeTimer(Timer timer, TimeSpan freezeDuration)
		{
			if (timer.State == TimerState.Frozen || timer.State == TimerState.Paused || timer.State == TimerState.Stopped)
			{
				return;
			}

			timer.State = TimerState.Frozen;
			timer.FreezeDuration = freezeDuration;
			timer.FreezeStartTime = dateTimeProvider.UtcNow;
			timer.TotalPausedDuringFreezedTime = TimeSpan.Zero;
			TimerStateChanged?.Invoke(timer);
		}

		public void DefrostTimer(Timer timer)
		{
			if (timer.State != TimerState.Frozen)
			{
				return;
			}

			if (IsTimerDefrosted(timer))
			{
				timer.State = TimerState.Running;
				timer.TotalFreezedTime += timer.FreezeDuration;
				timer.FreezeDuration = TimeSpan.Zero;
				TimerStateChanged?.Invoke(timer);
			}
		}

		public TimeSpan GetTimerRemainingTime(Timer timer)
		{
			if (timer.State == TimerState.Default)
			{
				return timer.Duration;
			}

			if (timer.State == TimerState.Stopped)
			{
				var timerDurationUntilStopWithWaitings = timer.StopTime - timer.StartTime - GetTotalElapsedPausedTime(timer) - GetTotalElapsedFreezeTime(timer);
				return timer.Duration - timerDurationUntilStopWithWaitings;
			}

			var timerDurationWithWaitings = timer.Duration + GetTotalElapsedPausedTime(timer) + GetTotalElapsedFreezeTime(timer);
			var timedElapsedFromStart = dateTimeProvider.UtcNow - timer.StartTime;
			var remainingTime = timerDurationWithWaitings - timedElapsedFromStart;
			return remainingTime <= TimeSpan.Zero ? TimeSpan.Zero : remainingTime;
		}

		public TimeSpan GetTotalElapsedPausedTime(Timer timer)
		{
			if (timer.State is TimerState.Paused or TimerState.Stopped)
			{
				return timer.TotalPausedTime + GetElapsedPausedTime(timer);
			}

			return timer.TotalPausedTime;
		}

		public TimeSpan GetElapsedPausedTime(Timer timer)
		{
			if (timer.State == TimerState.Paused)
			{
				return dateTimeProvider.UtcNow - timer.PauseStartTime;
			}

			if (timer.PreviousState == TimerState.Paused && timer.State == TimerState.Stopped)
			{
				return timer.StopTime - timer.PauseStartTime;
			}

			return TimeSpan.Zero;
		}

		public TimeSpan GetTotalElapsedFreezeTime(Timer timer)
		{
			if (timer.State is TimerState.Frozen or TimerState.Paused or TimerState.Stopped)
			{
				return timer.TotalFreezedTime + GetElapsedFreezeTime(timer);
			}

			return timer.TotalFreezedTime;
		}

		public TimeSpan GetElapsedFreezeTime(Timer timer)
		{
			if (timer.State == TimerState.Frozen)
			{
				return dateTimeProvider.UtcNow - timer.TotalPausedDuringFreezedTime - timer.FreezeStartTime;
			}

			if (timer.PreviousState == TimerState.Frozen && timer.State == TimerState.Paused)
			{
				return dateTimeProvider.UtcNow - GetElapsedPausedTime(timer) - timer.TotalPausedDuringFreezedTime - timer.FreezeStartTime;
			}

			if (timer.PreviousState == TimerState.Frozen && timer.State == TimerState.Stopped)
			{
				return timer.StopTime - GetElapsedPausedTime(timer) - timer.TotalPausedDuringFreezedTime - timer.FreezeStartTime;

			}

			return TimeSpan.Zero;
		}

		public bool IsTimerDefrosted(Timer timer)
		{
			if (timer.State != TimerState.Frozen && timer.State != TimerState.Paused)
			{
				return true;
			}

			var isDefrosted = GetElapsedFreezeTime(timer) >= timer.FreezeDuration;

			return isDefrosted;
		}

		public bool IsTimerExpired(Timer timer)
		{
			return GetTimerRemainingTime(timer) <= TimeSpan.Zero;
		}
	}
}
