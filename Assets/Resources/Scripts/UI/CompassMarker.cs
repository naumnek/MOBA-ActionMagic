using UnityEngine;
using UnityEngine.UI;
using Platinum.Controller;
using Platinum.Info;
using Platinum.Controller.AI;

namespace Platinum.UI
{
    public class CompassMarker : MonoBehaviour
    {
        [Tooltip("Main marker image")] public Image MainImage;

        [Tooltip("Canvas group for the marker")]
        public CanvasGroup CanvasGroup;

        [Header("Enemy element")]
        [Tooltip("Default color for the marker")]
        public Color DefaultColor;

        [Tooltip("Alternative color for the marker")]
        public Color AltColor;

        [Header("Direction element")]
        [Tooltip("Use this marker as a magnetic direction")]
        public bool IsDirection;

        [Tooltip("Text content for the direction")]
        public TMPro.TextMeshProUGUI TextContent;

        ControllerBot m_ControllerBot;

        public void Initialize(CompassElement compassElement, string textDirection)
        {
            if (IsDirection && TextContent)
            {
                TextContent.text = textDirection;
            }
            else
            {
                m_ControllerBot = compassElement.transform.GetComponent<ControllerBot>();

                if (m_ControllerBot)
                {
                    m_ControllerBot.AiModule.DetectionModule.onDetectedTarget += DetectTarget;
                    //m_ControllerBot.AiModule.DetectionModule.onLostTarget += LostTarget;

                    LostTarget();
                }
            }
        }

        public void DetectTarget(DetectionTarget target)
        {
            switch (target.TypeTarget)
            {
                case TypeTarget.Enemy:
                    MainImage.color = AltColor;
                    break;
                default:
                    MainImage.color = DefaultColor;
                    break;
            }
        }

        public void LostTarget()
        {
            MainImage.color = DefaultColor;
        }
    }
}