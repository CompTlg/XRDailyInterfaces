using UnityEngine;

public class PanelVisibility : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject Panel;
    public void OpenPanel()
    {
        if(Panel != null)
        {
            Panel.SetActive(true);
        }
    }

        public void ClosePanel()
        {
            if(Panel != null)
            {
                Panel.SetActive(false);
            }
    }
}
