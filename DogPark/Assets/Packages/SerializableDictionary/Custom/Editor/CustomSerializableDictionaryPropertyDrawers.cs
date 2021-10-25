using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(StringToListOfAudioClips))]
[CustomPropertyDrawer(typeof(StringToAudioClip))]
public class CustomAnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer {}

[CustomPropertyDrawer(typeof(ListOfAudioClipsStorage))]
public class CustomAnySerializableDictionaryStoragePropertyDrawer: SerializableDictionaryStoragePropertyDrawer {}
