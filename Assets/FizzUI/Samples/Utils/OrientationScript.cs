using UnityEngine;

namespace Fizz.Demo
{
    public class OrientationScript : MonoBehaviour
    {
        [SerializeField] private ScreenOrientation Orientation = ScreenOrientation.Portrait;

        void Start()
        {
            Screen.orientation = Orientation;
        }
    }
}