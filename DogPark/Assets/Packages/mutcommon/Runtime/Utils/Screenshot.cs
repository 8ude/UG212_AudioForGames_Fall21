using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MutCommon
{
  public class Screenshot : MonoBehaviour
  {
    // TODO: descobrir onde iss vai parar
    //public string Path = "Screenshots";
    [Tooltip("$NAME = productName, $DATE = current time formatted as specified below")]
    public string NameTemplate = "$NAME_$DATE";
    public string DateTimeFormat = "yyyy-MM-dd_HH-mm-ss";
    public void TakeScreenshot()
    {
      ScreenCapture.CaptureScreenshot(
        NameTemplate
        .Replace("$NAME", Application.productName)
        .Replace("$DATE", System.DateTime.Now.ToString(DateTimeFormat)) + ".png");
    }
  }
}