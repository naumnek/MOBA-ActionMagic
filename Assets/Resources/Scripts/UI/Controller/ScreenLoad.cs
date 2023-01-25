using UnityEngine;

namespace Platinum.UI
{
    public enum StateScreen
    {
        Visibly,
        Unvisibly,
    }
    public class ScreenLoad : StateMachineBehaviour
    {

        public StateScreen ScreenVisibly;
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            FindObjectOfType<LoadingScreenController>().AnimationScreenVisibility(ScreenVisibly);
        }
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (ScreenVisibly == StateScreen.Visibly)
            {
                FindObjectOfType<LoadingScreenController>().AnimationScreenVisibility(ScreenVisibly);
            }
        }
    }
}
