using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;

public class Clipboard : MonoBehaviour
{
    public void CopyStringToClipboard(string str) {
        GUIUtility.systemCopyBuffer = str;
    }

    public void CopyStringToClipboard(StringVariable str) => CopyStringToClipboard(str.Value);
}
