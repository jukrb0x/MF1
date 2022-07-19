using TMPro;
using UnityEngine;

namespace Gameplay
{
    public class BaseTrigger : MonoBehaviour
    {
        // fixme: test code
        private TextMeshProUGUI text;
        [SerializeField] private GameObject player;
        private Collider _collider;


        private void Start()
        {
            // player = GameObject.FindWithTag("Player");
            _collider = player.GetComponent<Collider>();
            text = GameObject.Find("DebugMsg").GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            OnTriggerStay(_collider);
        }

        private void OnTriggerStay(Collider other)
        {
            text.SetText("You have entered the trigger");
        }
    }
}