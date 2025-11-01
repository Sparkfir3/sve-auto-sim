using UnityEngine;
using Sirenix.OdinInspector;

namespace SVESimulator
{
    public class PlayPointSphere : MonoBehaviour
    {
        public enum Status { Disabled, Empty, Full }

        public void SetStatus(Status status)
        {
            switch(status)
            {
                case Status.Disabled:
                    gameObject.SetActive(false);
                    break;
                case Status.Empty:
                    gameObject.SetActive(true);
                    break;
                case Status.Full:
                    gameObject.SetActive(true);
                    break;
            }
        }
    }
}
