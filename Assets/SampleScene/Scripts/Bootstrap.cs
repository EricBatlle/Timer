using System;
using TimerModule;
using UnityEngine;

namespace TimerSampleScene
{
	public class Bootstrap : MonoBehaviour
	{
		[SerializeField]
		private TimerViewControls timerViewControls;
		[SerializeField]
		private TimerViewValues timerViewValues;
		[SerializeField]
		private RemainingTimeBar timerRemainingTimerBar;
		[SerializeField]
		private RemainingTimeBar timerFreezingRemainingTimerBar;

		private Timer timer;
		private IDateTimeProvider dateTimeProvider;
		private TimerService timerService;

		private void Start()
		{
			dateTimeProvider = new DateTimeProvider();
			timerService = new TimerService(dateTimeProvider);
			timerService.TimerStateChanged += OnTimerStateChanged;

			timer = new Timer(TimeSpan.FromSeconds(3));

			timerViewControls.TimerStarted += (duration) => timerService.StartTimer(timer, TimeSpan.FromSeconds(duration));
			timerViewControls.TimerStopped += () => timerService.StopTimer(timer);
			timerViewControls.TimerPaused += () => timerService.PauseTimer(timer);
			timerViewControls.TimerResumed += () => timerService.ResumeTimer(timer);
			timerViewControls.TimerReset += () => timerService.ResetTimer(ref timer);
			timerViewControls.TimerFreeze += (freezeDuration) => timerService.FreezeTimer(timer, TimeSpan.FromSeconds(freezeDuration));
		}

		private void Update()
		{
			if (timer != null)
			{
				if (timerService.GetTimerRemainingTime(timer) <= TimeSpan.Zero)
				{
					timerService.StopTimer(timer);
				}

				if (timerService.IsTimerDefrosted(timer))
				{
					timerService.DefrostTimer(timer);
				}

				timerViewValues.UpdateView(timer, timerService);
				timerRemainingTimerBar.Fill(GetTimerRemainingTimeNormalized(timer));
				timerFreezingRemainingTimerBar.Fill(GetTimerFreezingRemainingTimeNormalized(timer));
			}
		}

		private void OnTimerStateChanged(Timer timer)
		{
			Debug.LogWarning($"Timer state changed: \n{timer.PreviousState} --> {timer.State}");
		}

		private float GetTimerRemainingTimeNormalized(Timer timer)
		{
			return (float)(timerService.GetTimerRemainingTime(timer).TotalSeconds / timer.Duration.TotalSeconds);
		}
		private float GetTimerFreezingRemainingTimeNormalized(Timer timer)
		{
			return (float)((timer.FreezeDuration.TotalSeconds - timerService.GetElapsedFreezeTime(timer).TotalSeconds) / timer.FreezeDuration.TotalSeconds);
		}
	}
}
