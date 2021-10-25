using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MutCommon
{
  public class Snapshot : MonoBehaviour
  {
    [SerializeField] private Camera cam;

    private void OnValidate()
    {
      if (cam == null) cam = GetComponent<Camera>();
      if (cam == null) cam = GetComponentInChildren<Camera>();
    }

    [Tooltip("$NAME = productName, $DATE = current time formatted as specified below")]
    [SerializeField] private string nameTemplate = "$NAME_$DATE";
    [SerializeField] private string dateTimeFormat = "yyyy-MM-dd_HH-mm-ss";
    [SerializeField] private string path = "Snapshots";

    [SerializeField] private int width = 1920;
    [SerializeField] private int height = 1080;

    [SerializeField] private RenderTexture _renderTexture;
    private RenderTexture renderTexture
    {
      get
      {
        if (_renderTexture != null) return _renderTexture;
        if (cam?.targetTexture != null)
        {
          _renderTexture = cam.targetTexture;
          Debug.Log("Snapshot is overriding render texture settings to use the camera's texture");
        }
        else
        {
          _renderTexture = new RenderTexture(width, height, 16, RenderTextureFormat.Default);
          _renderTexture.Create();

        }
        return _renderTexture;
      }
    }

    public void TakeSnapshot()
    {
      if (cam == null)
      {
        Debug.LogError("There is no camera set for the snapshot");
        return;
      }

      RenderTexture activeRenderTexture = RenderTexture.active;
      RenderTexture.active = renderTexture;
      var camTexture = cam.targetTexture;
      cam.targetTexture = renderTexture;

      cam.Render();

      Texture2D image = new Texture2D(renderTexture.width, renderTexture.height);
      image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
      image.Apply();

      RenderTexture.active = activeRenderTexture;
      cam.targetTexture = camTexture;

      byte[] bytes = image.EncodeToPNG();

      Destroy(image);

      var fileName = nameTemplate
            .Replace("$NAME", Application.productName)
            .Replace("$DATE", System.DateTime.Now.ToString(dateTimeFormat)) + ".png";
      var location = $"{Application.dataPath}//{path}//{fileName}";

      Directory.CreateDirectory(Path.GetDirectoryName(location));
      File.WriteAllBytes(location, bytes);

      Debug.Log($"New image created at: {location}");
    }
  }
}