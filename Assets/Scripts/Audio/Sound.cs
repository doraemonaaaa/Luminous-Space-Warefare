using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyAudio
{
    [System.Serializable]
    public class Sound
    {
        public string name;         // 音频剪辑的名称
        public AudioClip clip;      // 音频剪辑
        [Range(0f, 1f)]
        public float volume = 0.7f; // 音量大小
    }
}
