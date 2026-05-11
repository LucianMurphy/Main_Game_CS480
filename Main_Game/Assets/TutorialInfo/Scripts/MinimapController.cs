using System.Collections;
using UnityEngine;

// Attach to a dedicated MinimapController GameObject in the PacMan scene.
// Wire minimapCamera and minimapPanel in the Inspector.
public class MinimapController : MonoBehaviour
{
    public Camera minimapCamera;
    public GameObject minimapPanel;

    void Start()
    {
        minimapCamera.enabled = false;
        minimapPanel.SetActive(false);
    }

    public void ShowMinimap()
    {
        StopAllCoroutines();
        minimapCamera.enabled = true;
        minimapPanel.SetActive(true);
        StartCoroutine(HideAfter(ItemCatalog.Items[ItemType.Map].duration));
    }

    private IEnumerator HideAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        minimapCamera.enabled = false;
        minimapPanel.SetActive(false);
    }
}
