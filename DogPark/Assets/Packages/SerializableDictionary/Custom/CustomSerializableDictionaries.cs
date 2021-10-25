using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class StringToAudioClip : SerializableDictionary<string, AudioClip> {}

[Serializable]
public class ListOfAudioClipsStorage : SerializableDictionary.Storage<List<AudioClip>> {}

[Serializable]
public class StringToListOfAudioClips : SerializableDictionary<string, List<AudioClip>, ListOfAudioClipsStorage> {}
