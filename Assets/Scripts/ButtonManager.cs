using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public Settings_End settings_End;
    public GameObject Settings_Image;
    public GameObject Settings_Panel;

    public void OpenSettingsButton()
    {
        Settings_Image.SetActive(true);
        Settings_Panel.SetActive(true);
    }

    public void Settings_End()
    {
        Settings_Image.SetActive(false);
        Settings_Panel.SetActive(false);
    }

    public void CloseSettingsButton()
    {
        settings_End.operateSettings_Down();
    }
}
