using NSubstitute;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
	private Timer timer;
	private IDateTimeProvider dateTimeProvider;
	private TimerService timerService;

	// Start is called before the first frame update
	private void Start()
	{
		timer = new Timer(TimeSpan.FromSeconds(3));

		dateTimeProvider = new DateTimeProvider();
		timerService = new TimerService(dateTimeProvider);

		timerService.StartTimer(timer);
	}

	private void FixedUpdate()
	{
		if(timer != null)
		{
			Debug.LogWarning(timerService.GetTimerRemainingTime(timer).Seconds);
		}
	}
}
