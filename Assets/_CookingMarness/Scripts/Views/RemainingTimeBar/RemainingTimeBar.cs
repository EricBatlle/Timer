using UnityEngine;
using UnityEngine.UI;

namespace CookingMarness
{
	public class RemainingTimeBar : MonoBehaviour
	{
		[SerializeField]
		private Image barFillImage;

		public void Fill(float fillAmount)
		{
			barFillImage.fillAmount = fillAmount;
		}
	}
}
