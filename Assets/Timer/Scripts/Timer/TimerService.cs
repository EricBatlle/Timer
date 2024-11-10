using System;

namespace TimerModule
{
	public class TimerService
	{
		private readonly IDateTimeProvider dateTimeProvider;

		public TimerService(IDateTimeProvider dateTimeProvider)
		{
			this.dateTimeProvider = dateTimeProvider;
		}

		public void ResetTimer(ref Timer timer)
		{
			timer = new Timer(timer.Duration);
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
			timer.State = TimerState.Running;
			timer.StartTime = dateTimeProvider.UtcNow;
		}

		public void PauseTimer(Timer timer)
		{
			if (timer.State == TimerState.Paused)
			{
				return;
			}

			timer.State = TimerState.Paused;
			timer.PauseStartTime = dateTimeProvider.UtcNow;
		}

		public void ResumeTimer(Timer timer)
		{
			if (timer.State != TimerState.Paused)
			{
				return;
			}

			timer.State = timer.PreviousState;
			timer.PauseEndTime = dateTimeProvider.UtcNow;
			if (timer.State == TimerState.Freeze)
			{
				timer.TotalPausedDuringFreezedTime += timer.GetLastPauseTime;
			}
		}

		public void FreezeTimer(Timer timer, TimeSpan freezeDuration)
		{
			if (timer.State == TimerState.Freeze || timer.State == TimerState.Paused)
			{
				return;
			}

			timer.State = TimerState.Freeze;
			timer.FreezeDuration = freezeDuration;
			timer.FreezeStartTime = dateTimeProvider.UtcNow;
			timer.TotalPausedDuringFreezedTime = TimeSpan.Zero;
		}

		public void DefrostTimer(Timer timer)
		{
			if (timer.State != TimerState.Freeze)
			{
				return;
			}

			if (IsTimerDefrosted(timer))
			{
				timer.State = TimerState.Running;
				timer.TotalFreezedTime += timer.FreezeDuration;
			}
		}

		public TimeSpan GetTimerRemainingTime(Timer timer)
		{
			if (timer.State == TimerState.Default)
			{
				return timer.Duration;
			}

			var timerDurationWithWaitings = timer.Duration + GetTotalElapsedPausedTime(timer) + GetTotalElapsedFreezeTime(timer);
			var timedElapsedFromStart = dateTimeProvider.UtcNow - timer.StartTime;
			var remainingTime = timerDurationWithWaitings - timedElapsedFromStart;
			return remainingTime <= TimeSpan.Zero ? TimeSpan.Zero : remainingTime;
		}

		public TimeSpan GetTotalElapsedPausedTime(Timer timer)
		{
			if (timer.State == TimerState.Paused)
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

			return TimeSpan.Zero;
		}

		public TimeSpan GetTotalElapsedFreezeTime(Timer timer)
		{
			if (timer.State == TimerState.Freeze || timer.State == TimerState.Paused)
			{
				return timer.TotalFreezedTime + GetElapsedFreezeTime(timer);
			}

			return timer.TotalFreezedTime;
		}

		public TimeSpan GetElapsedFreezeTime(Timer timer)
		{
			if (timer.State == TimerState.Freeze)
			{
				return dateTimeProvider.UtcNow - timer.TotalPausedDuringFreezedTime - timer.FreezeStartTime;
			}

			if (timer.PreviousState == TimerState.Freeze && timer.State == TimerState.Paused)
			{
				return dateTimeProvider.UtcNow - GetElapsedPausedTime(timer) - timer.TotalPausedDuringFreezedTime - timer.FreezeStartTime;
			}

			return TimeSpan.Zero;
		}

		public bool IsTimerDefrosted(Timer timer)
		{
			if (timer.State != TimerState.Freeze && timer.State != TimerState.Paused)
			{
				return true;
			}

			var isDefrosted = GetElapsedFreezeTime(timer) >= timer.FreezeDuration;

			return isDefrosted;
		}
	}
}
