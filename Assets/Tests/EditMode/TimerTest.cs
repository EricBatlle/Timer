using NSubstitute;
using NUnit.Framework;
using System;

public class TimerTest
{
	private Timer timer;
	private IDateTimeProvider dateTimeProvider;
	private TimerService timerService;

	private DateTime utcNow;

	[SetUp]
	public void SetUp()
	{
		dateTimeProvider = Substitute.For<IDateTimeProvider>();
		timerService = new TimerService(dateTimeProvider);
	}

	[Test]
	public void When_TimerIsPartiallyElapsed_Expect_RemainingTimeMatchesElapsedTime()
	{
		var timerDuration = 3;
		timer = new Timer(TimeSpan.FromSeconds(timerDuration));

		StartTimer(timer);
		MoveForwardInTime(timerDuration - 1);
		Assert.IsTrue(timerService.GetTimerRemainingTime(timer).Seconds == (timerDuration - utcNow.Second));
	}

	[Test]
	public void When_TimerIsPausedBeforeExpiry_Expect_RemainingTime()
	{
		var timerDuration = 3;
		timer = new Timer(TimeSpan.FromSeconds(timerDuration));

		StartTimer(timer);
		MoveForwardInTime(timerDuration - 1);
		timerService.PauseTimer(timer);
		MoveForwardInTime(timerDuration);
		Assert.IsTrue(timerService.GetTimerRemainingTime(timer).Seconds > 0);
	}

	[Test]
	public void When_TimerIsPausedBeforeExpiry_And_ResumedUntilExpiring_Expect_NoRemainingTime()
	{
		var timerDuration = 3;
		timer = new Timer(TimeSpan.FromSeconds(timerDuration));

		StartTimer(timer);
		MoveForwardInTime(timerDuration - 1);
		timerService.PauseTimer(timer);
		MoveForwardInTime(timerDuration);
		timerService.ResumeTimer(timer);
		MoveForwardInTime(timerDuration);

		Assert.IsTrue(timerService.GetTimerRemainingTime(timer).Seconds <= 0);
	}

	[Test]
	public void When_TimerIsFreeze_Expect_RemainingTime_And_FreezeTimer()
	{
		var timerDuration = 3;
		var timerFreezeDuration = 3;
		timer = new Timer(TimeSpan.FromSeconds(timerDuration));

		StartTimer(timer);
		MoveForwardInTime(timerDuration - 1);

		timerService.FreezeTimer(timer, TimeSpan.FromSeconds(timerFreezeDuration));

		Assert.IsFalse(timerService.IsTimerDefrosted(timer));
		Assert.IsTrue(timerService.GetTimerRemainingTime(timer).Seconds > 0);
	}

	[Test]
	public void When_TimerIsFreeze_And_DefrostedUntilExpiring_Expect_NoRemainingTime_And_DefrostedTimer()
	{
		var timerDuration = 3;
		var timerFreezeDuration = 3;
		timer = new Timer(TimeSpan.FromSeconds(timerDuration));

		StartTimer(timer);
		MoveForwardInTime(timerDuration - 1);
		timerService.FreezeTimer(timer, TimeSpan.FromSeconds(timerFreezeDuration));
		MoveForwardInTime(timerFreezeDuration);
		timerService.DefrostTimer(timer);
		MoveForwardInTime(timerDuration);

		Assert.IsTrue(timerService.IsTimerDefrosted(timer));
		Assert.IsTrue(timerService.GetTimerRemainingTime(timer).Seconds <= 0);
	}

	[Test]
	public void When_TimerIsFreeze_Then_Paused_Then_Resume_Then_DefrostedUntilExpiring_Then_ExpireTimer_Expect_NoRemainingTime_And_DefrostedTimer()
	{
		var timerDuration = 3;
		var timerFreezeDuration = 3;
		timer = new Timer(TimeSpan.FromSeconds(timerDuration));

		StartTimer(timer);
		MoveForwardInTime(timerDuration - 1);
		timerService.FreezeTimer(timer, TimeSpan.FromSeconds(timerFreezeDuration));
		MoveForwardInTime(timerFreezeDuration - 1);
		timerService.PauseTimer(timer);
		MoveForwardInTime(timerDuration);
		timerService.ResumeTimer(timer);
		MoveForwardInTime(1);
		timerService.DefrostTimer(timer);
		MoveForwardInTime(1);

		Assert.IsTrue(timerService.IsTimerDefrosted(timer));
		Assert.IsTrue(timerService.GetTimerRemainingTime(timer).Seconds <= 0);
	}

	[Test]
	public void When_TimerIsFreeze_Then_PausedFreezingTimeDuration_Then_Resume_Expect_FreezingTimer()
	{
		var timerDuration = 3;
		var timerFreezeDuration = 3;
		timer = new Timer(TimeSpan.FromSeconds(timerDuration));

		StartTimer(timer);
		timerService.FreezeTimer(timer, TimeSpan.FromSeconds(timerFreezeDuration));
		timerService.PauseTimer(timer);
		MoveForwardInTime(timerFreezeDuration);

		Assert.IsFalse(timerService.IsTimerDefrosted(timer));
	}

	[Test]
	public void When_TimerFreezed_And_PausedMultipleTimes()
	{
		var timerDuration = 3;
		var timerFreezeDuration = 3;
		timer = new Timer(TimeSpan.FromSeconds(timerDuration));

		StartTimer(timer);
		timerService.FreezeTimer(timer, TimeSpan.FromSeconds(timerFreezeDuration));
		PauseMoveForwardInTimeAndResumeTimer(timer, 1);
		MoveForwardInTime(1);
		PauseMoveForwardInTimeAndResumeTimer(timer, 1);


		Assert.IsFalse(timerService.IsTimerDefrosted(timer));
		MoveForwardInTime(timerFreezeDuration - 1);
		timerService.DefrostTimer(timer);

		Assert.IsTrue(timerService.IsTimerDefrosted(timer));
		Assert.IsTrue(timerService.GetTimerRemainingTime(timer).Seconds > 0);

		MoveForwardInTime(timerDuration);
		Assert.IsTrue(timerService.GetTimerRemainingTime(timer).Seconds == 0);
	}

	[Test]
	public void When_TimerFreezed_Then_Paused_Expect_SameRemainingTimeBeforeAndAfterPause()
	{
		var timerDuration = 3;
		var timerFreezeDuration = 3;
		timer = new Timer(TimeSpan.FromSeconds(timerDuration));

		StartTimer(timer);
		MoveForwardInTime(1);
		timerService.FreezeTimer(timer, TimeSpan.FromSeconds(timerFreezeDuration));
		MoveForwardInTime(1);
		var remainingTimeBeforePause = timerService.GetTimerRemainingTime(timer);
		timerService.PauseTimer(timer);
		MoveForwardInTime(1);
		var remainingTimeAfterPause = timerService.GetTimerRemainingTime(timer);
		Assert.AreEqual(remainingTimeBeforePause, remainingTimeAfterPause);
	}

	[Test]
	public void When_TimerFreezed_Then_Paused_Then_Resume_Then_PauseAgain_Expect_SameRemainingFreezeTimeBeforeAndAfterSecondPause()
	{
		var timerDuration = 3;
		var timerFreezeDuration = 3;
		timer = new Timer(TimeSpan.FromSeconds(timerDuration));

		StartTimer(timer);
		MoveForwardInTime(1);
		timerService.FreezeTimer(timer, TimeSpan.FromSeconds(timerFreezeDuration));
		MoveForwardInTime(1);
		PauseMoveForwardInTimeAndResumeTimer(timer, 1);
		MoveForwardInTime(1);
		var remainingTimeBeforePause = timerService.GetElapsedFreezeTime(timer);
		timerService.PauseTimer(timer);
		MoveForwardInTime(1);
		var remainingTimeAfterPause = timerService.GetElapsedFreezeTime(timer);

		Assert.AreEqual(remainingTimeBeforePause, remainingTimeAfterPause);
	}

	[Test]
	public void When_TimerFreezed_Then_Paused_Then_TryDefrostWithoutPreviousResume_Expect_TimerNotRunning()
	{
		var timerDuration = 3;
		var timerFreezeDuration = 3;
		timer = new Timer(TimeSpan.FromSeconds(timerDuration));
		StartTimer(timer);
		timerService.FreezeTimer(timer, TimeSpan.FromSeconds(timerFreezeDuration));
		timerService.PauseTimer(timer);
		timerService.DefrostTimer(timer);
		Assert.IsFalse(timer.State == TimerState.Running);
	}

	private void StartTimer(Timer timer)
	{
		utcNow = dateTimeProvider.UtcNow;
		dateTimeProvider.UtcNow.Returns(utcNow);
		timerService.StartTimer(timer);
	}

	private void MoveForwardInTime(int seconds)
	{
		utcNow += TimeSpan.FromSeconds(seconds);
		dateTimeProvider.UtcNow.Returns(utcNow);
	}

	private void PauseMoveForwardInTimeAndResumeTimer(Timer timer, int seconds)
	{
		timerService.PauseTimer(timer);
		MoveForwardInTime(seconds);
		timerService.ResumeTimer(timer);
	}
}
