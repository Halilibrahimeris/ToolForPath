using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class HideableObject : MonoBehaviour
{
    public PostProcessVolume postProcessVolume;
    private Vignette vignette;
    private Coroutine vignetteRoutine;

    private void Start()
    {
        if (postProcessVolume.profile.TryGetSettings(out vignette))
        {
            vignette.enabled.value = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHideController>().isHide = true;

            if (vignetteRoutine != null)
            {
                StopCoroutine(vignetteRoutine);
            }
            vignetteRoutine = StartCoroutine(AdjustVignetteSmoothness(0f, 0.7f));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHideController>().isHide = false;

            if (vignetteRoutine != null)
            {
                StopCoroutine(vignetteRoutine);
            }
            vignette.enabled.value = false;
        }
    }


    private IEnumerator AdjustVignetteSmoothness(float minSmoothness, float maxSmoothness)
    {
        vignette.enabled.value = true;

        float time = 0f;
        bool increasing = true;

        while (true)
        {

            time += Time.deltaTime * (increasing ? 1f : -1f);
            vignette.smoothness.value = Mathf.Lerp(minSmoothness, maxSmoothness, time);

            if (time >= 1f)
            {
                increasing = false;
            }
            else if (time <= 0f)
            {
                increasing = true;
            }

            yield return null;
        }
    }
}
