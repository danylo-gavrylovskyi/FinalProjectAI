using UnityEngine;
using UnityEngine.UI;

namespace CursedDungeon.UI
{
    [RequireComponent(typeof(Image))]
    public class HealthBarColor : MonoBehaviour
    {
        private static readonly Color ColorFull    = new Color(0.2f, 0.85f, 0.2f); // green
        private static readonly Color ColorMid     = new Color(1f,   0.85f, 0f);   // yellow
        private static readonly Color ColorLow     = new Color(0.9f, 0.15f, 0.1f); // red

        private Image fill;

        private void Awake()
        {
            fill = GetComponent<Image>();
        }

        private void Update()
        {
            var t = fill.fillAmount;

            Color target;
            if (t > 0.5f)
                target = Color.Lerp(ColorMid, ColorFull, (t - 0.5f) * 2f);
            else
                target = Color.Lerp(ColorLow, ColorMid, t * 2f);

            fill.color = target;
        }
    }
}