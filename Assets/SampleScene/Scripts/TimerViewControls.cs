using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimerSampleScene
{
	public class TimerViewControls : MonoBehaviour
	{
		[SerializeField]
		private Button startButton;
		[SerializeField]
		private TMP_InputField duration;
		[SerializeField]
		private Button pauseButton;
		[SerializeField]
		private Button resumeButton;
		[SerializeField]
		private Button resetButton;
		[SerializeField]
		private Button freezeButton;
		[SerializeField]
		private TMP_InputField freezeDuration;

		public Action<float> TimerStarted;
		public Action TimerPaused;
		public Action TimerResumed;
		public Action TimerReset;
		public Action<float> TimerFreeze;

		private void Awake()
		{
			startButton.onClick.AddListener(() => TimerStarted?.Invoke(float.Parse(duration.text)));
			pauseButton.onClick.AddListener(() => TimerPaused?.Invoke());
			resumeButton.onClick.AddListener(() => TimerResumed?.Invoke());
			resetButton.onClick.AddListener(() => TimerReset?.Invoke());
			freezeButton.onClick.AddListener(() => TimerFreeze?.Invoke(float.Parse(freezeDuration.text)));
		}
	}
}
