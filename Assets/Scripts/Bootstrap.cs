using System;
using System.Threading.Tasks;
using UnityEngine;

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

		timer = new Timer(TimeSpan.FromSeconds(3));

		timerViewControls.TimerStarted += (duration) => timerService.StartTimer(timer, TimeSpan.FromSeconds(duration));
		timerViewControls.TimerPaused += () => timerService.PauseTimer(timer);
		timerViewControls.TimerResumed += () => timerService.ResumeTimer(timer);
		timerViewControls.TimerReset += () => timerService.ResetTimer(ref timer);
		timerViewControls.TimerFreeze += (freezeDuration) => timerService.FreezeTimer(timer, TimeSpan.FromSeconds(freezeDuration));

		//SequenceAsync();
	}

	[ContextMenu("Start")]
	private async Task SequenceAsync()
	{
		timerService.StartTimer(timer, TimeSpan.FromSeconds(3));
		await Task.Delay(1000);
		timerService.FreezeTimer(timer, TimeSpan.FromSeconds(3));
		await Task.Delay(1000);
		Debug.Log(GetTimerRemainingTimeNormalized(timer));
		timerService.PauseTimer(timer);
		await Task.Delay(1000);
		Debug.Log(GetTimerRemainingTimeNormalized(timer));
	}

	private void FixedUpdate()
	{
		if (timer != null)
		{
			timerViewValues.UpdateView(timer, timerService);
			timerRemainingTimerBar.Fill(GetTimerRemainingTimeNormalized(timer));
			timerFreezingRemainingTimerBar.Fill(GetTimerFreezingRemainingTimeNormalized(timer));
			if(timerService.IsTimerDefrosted(timer))
			{
				timerService.DefrostTimer(timer);
			}
		}
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
