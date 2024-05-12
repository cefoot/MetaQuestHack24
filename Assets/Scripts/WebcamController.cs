using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WebcamController : MonoBehaviour
{
    // public WebCamTexture WebCam;

    [SerializeField]
    private UnityEvent<Texture> _textureReady;

    // Start is called before the first frame update
    void Start()
    {
        // var ovrOverLay = FindObjectsByType<OVROverlay>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        // Debug.Log($"[{nameof(WebcamController)}] found {ovrOverLay.Length} Overlays");
        // foreach (var item in ovrOverLay)
        // {
        //     try
        //     {
        //         Debug.Log($"[{nameof(WebcamController)}] found OVROverlay:{item.name} with TextureCount({item.textures.Length})");
        //         Debug.Log($"[{nameof(WebcamController)}] using first texture");
        //         _textureReady?.Invoke(ovrOverLay[0].textures[0]);
        //         return;
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogError($"[{nameof(WebcamController)}] ({e.Message}) Exception... next overlay");
        //     }
        // }

        Camera.onPostRender += OnCameraPostRender;

        // WebCam = new WebCamTexture();
        // WebCam.Play();
        // _textureReady?.Invoke(WebCam);
    }

    private void OnCameraPostRender(Camera cam)
    {
        _textureReady?.Invoke(cam.activeTexture);
    }

    private void EditorPreview(OVROverlay overlay)
    {
        var previewObject = new GameObject();
        previewObject.transform.SetParent(overlay.transform, worldPositionStays: false);
        previewObject.AddComponent<OVROverlayMeshGenerator>().SetOverlay(overlay);
    }

}
