using UnityEngine;
using TMPro;

public class TimerViewValues : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI durationText;
	[SerializeField]
	private TextMeshProUGUI remainingText;
	[SerializeField]
	private TextMeshProUGUI pausedText;
	[SerializeField]
	private TextMeshProUGUI totalPausedText;
	[SerializeField]
	private TextMeshProUGUI freezeDurationText;
	[SerializeField]
	private TextMeshProUGUI freezeText;
	[SerializeField]
	private TextMeshProUGUI totalFreezeText;

	public void UpdateView(Timer timer, TimerService timerService)
	{
		durationText.text = $"Duration Time: {timer.Duration}";
		remainingText.text = $"Remaining Time: {timerService.GetTimerRemainingTime(timer)}";
		pausedText.text = $"Paused Time: {timerService.GetElapsedPausedTime(timer)}";
		totalPausedText.text = $"Total Paused Time: {timerService.GetTotalElapsedPausedTime(timer)}";
		freezeDurationText.text = $"Freeze Duration: {timer.FreezeDuration}";
		freezeText.text = $"Freeze Time: {timerService.GetElapsedFreezeTime(timer).TotalSeconds}";
		totalFreezeText.text = $"Total Freezed Time: {timerService.GetTotalElapsedFreezeTime(timer)}";
	}
}
