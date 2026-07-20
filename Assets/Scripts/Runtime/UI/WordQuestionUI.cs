using UnityEngine;
using TMPro;

namespace WordWheel.Runtime.UI
{
    public class WordQuestionUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private GameObject panelObject;

        private void Awake()
        {
            if (panelObject == null)
            {
                panelObject = gameObject;
            }
            Hide();
        }

        public void ShowWord(string word)
        {
            if (questionText != null)
            {
                questionText.text = word;
            }
            if (panelObject != null)
            {
                panelObject.SetActive(true);
            }
        }

        public void Hide()
        {
            if (panelObject != null)
            {
                panelObject.SetActive(false);
            }
        }
    }
}
