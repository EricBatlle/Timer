using UnityEngine;
using UnityEngine.UI;

public class RemainingTimeBar : MonoBehaviour
{
	[SerializeField]
	private Image barFillImage;

	public void Fill(float fillAmount)
	{
		barFillImage.fillAmount = fillAmount;
	}
}

