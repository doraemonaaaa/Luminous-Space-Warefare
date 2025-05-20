using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyAudio
{
    public class AudioManager : SingletonMono<AudioManager>
    {
        // 实现抽象属性，指定是否使用 DontDestroyOnLoad
        protected override bool isDontDestroyOnLoad => false;

        //定义音乐和音效的Sound数组
        public Sound[] musicSounds, sfxSounds;
        //音乐和音效的AudioSource
        public AudioSource musicSource, sfxSource;

        protected override void Awake()
        {
            base.Awake();
            // Resources下load
            // 加载所有音乐音效
            musicSounds = LoadSounds("Audios/Music");
            // 加载所有SFX音效
            sfxSounds = LoadSounds("Audios/SFX");
        }

        /// <summary>
        /// 对象池释放SFX音效，具有3D效果
        /// </summary>
        /// <param name="sfx_name"></param>
        /// <param name="pos"></param>
        //public void ObjectPoolSFXPlay(string sfx_name, Vector3 pos, float volume =1)
        //{
        //    // 从对象池中获取3D一次shot对象
        //    GameObject audio = GameObjectPoolManager.Instance.GetObject("3DAudioSource", pos);
        //    if(audio == null)
        //    {
        //        Debug.LogError("获取3D音源失败");
        //        return;
        //    }

        //    AudioSource audio_source = audio.GetComponent<AudioSource>();
        //    //从音乐Sounds数组中找到名字匹配的Sound对象
        //    Sound sound = Array.Find(sfxSounds, x => x.name == sfx_name);
        //    if(sound == null)
        //    {
        //        Debug.LogError("获取音效失败");
        //        return;
        //    }
        //    audio_source.volume = sound.volume;
        //    audio_source.PlayOneShot(sound.clip);

        //    // 使用一个不会销毁的物体创建协程，否则自己死后会协程结束
        //    GameObjectPoolManager.Instance.StartCoroutine(GameObjectPoolManager.Instance.ReleaseAfterDelay("3DAudioSource", audio, sound.clip.length));
        //}


        /// <summary>
        /// 播放音乐的方法，参数为音乐名称
        /// </summary>
        /// <param name="name"></param>
        public void PlayMusic(string name, bool loop, float volume=1)
        {
            //从音乐Sounds数组中找到名字匹配的Sound对象
            Sound s = Array.Find(musicSounds, x => x.name == name);
            //如果找不到对应的Sound，输出错误信息
            if (s == null)
            {
                Debug.Log("没有找到音乐");
            }
            //否则将音乐源的clip设置为对应Sound的clip并播放
            else
            {
                musicSource.clip = s.clip;  // 音量设置
                musicSource.volume = volume;
                musicSource.loop = loop;
                musicSource.Play();
            }
        }

        /// <summary>
        /// 播放音效的方法，参数为音效名称
        /// </summary>
        /// <param name="name"></param>
        public void PlaySFX(string name, float volume=1)
        {
            //从音效Sounds数组中找到名字匹配的Sound对象
            Sound s = Array.Find(sfxSounds, x => x.name == name);
            //如果找不到对应的Sound，输出错误信息
            if (s == null)
            {
                Debug.Log("没有找到音效");
            }
            //否则播放对应Sound的clip
            else
            {
                sfxSource.volume = volume;  // 音量设置
                sfxSource.PlayOneShot(s.clip);
            }
        }

        /// <summary>
        /// 切换音乐的静音状态
        /// </summary>
        public void ToggleMusic()
        {
            musicSource.mute = !musicSource.mute;
        }

        /// <summary>
        /// 切换音效的静音状态
        /// </summary>
        public void ToggleSFX()
        {
            sfxSource.mute = !sfxSource.mute;
        }

        /// <summary>
        /// 从指定的资源文件夹中加载 Sound 对象数组
        /// </summary>
        /// <param name="folderName">资源文件夹名称</param>
        /// <returns>加载的 Sound 对象数组</returns>
        private Sound[] LoadSounds(string folderName)
        {
            // 从 Resources 文件夹中加载所有 AudioClip
            AudioClip[] audioClips = Resources.LoadAll<AudioClip>(folderName);

            // 创建 Sound 数组，并初始化
            Sound[] sounds = new Sound[audioClips.Length];

            // 遍历每个 AudioClip，创建 Sound 对象并初始化
            for (int i = 0; i < audioClips.Length; i++)
            {
                AudioClip clip = audioClips[i];
                sounds[i] = new Sound
                {
                    name = clip.name,
                    clip = clip,
                    volume = 1.0f // 默认音量，可以根据需要修改
                };
            }

            return sounds;
        }
    }
}
