using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyAudio
{
    public class AudioManager : SingletonMono<AudioManager>
    {
        // ʵ�ֳ������ԣ�ָ���Ƿ�ʹ�� DontDestroyOnLoad
        protected override bool isDontDestroyOnLoad => false;

        //�������ֺ���Ч��Sound����
        public Sound[] musicSounds, sfxSounds;
        //���ֺ���Ч��AudioSource
        public AudioSource musicSource, sfxSource;

        protected override void Awake()
        {
            base.Awake();
            // Resources��load
            // ��������������Ч
            musicSounds = LoadSounds("Audios/Music");
            // ��������SFX��Ч
            sfxSounds = LoadSounds("Audios/SFX");
        }

        /// <summary>
        /// ������ͷ�SFX��Ч������3DЧ��
        /// </summary>
        /// <param name="sfx_name"></param>
        /// <param name="pos"></param>
        //public void ObjectPoolSFXPlay(string sfx_name, Vector3 pos, float volume =1)
        //{
        //    // �Ӷ�����л�ȡ3Dһ��shot����
        //    GameObject audio = GameObjectPoolManager.Instance.GetObject("3DAudioSource", pos);
        //    if(audio == null)
        //    {
        //        Debug.LogError("��ȡ3D��Դʧ��");
        //        return;
        //    }

        //    AudioSource audio_source = audio.GetComponent<AudioSource>();
        //    //������Sounds�������ҵ�����ƥ���Sound����
        //    Sound sound = Array.Find(sfxSounds, x => x.name == sfx_name);
        //    if(sound == null)
        //    {
        //        Debug.LogError("��ȡ��Чʧ��");
        //        return;
        //    }
        //    audio_source.volume = sound.volume;
        //    audio_source.PlayOneShot(sound.clip);

        //    // ʹ��һ���������ٵ����崴��Э�̣������Լ������Э�̽���
        //    GameObjectPoolManager.Instance.StartCoroutine(GameObjectPoolManager.Instance.ReleaseAfterDelay("3DAudioSource", audio, sound.clip.length));
        //}


        /// <summary>
        /// �������ֵķ���������Ϊ��������
        /// </summary>
        /// <param name="name"></param>
        public void PlayMusic(string name, bool loop, float volume=1)
        {
            //������Sounds�������ҵ�����ƥ���Sound����
            Sound s = Array.Find(musicSounds, x => x.name == name);
            //����Ҳ�����Ӧ��Sound�����������Ϣ
            if (s == null)
            {
                Debug.Log("û���ҵ�����");
            }
            //��������Դ��clip����Ϊ��ӦSound��clip������
            else
            {
                musicSource.clip = s.clip;  // ��������
                musicSource.volume = volume;
                musicSource.loop = loop;
                musicSource.Play();
            }
        }

        /// <summary>
        /// ������Ч�ķ���������Ϊ��Ч����
        /// </summary>
        /// <param name="name"></param>
        public void PlaySFX(string name, float volume=1)
        {
            //����ЧSounds�������ҵ�����ƥ���Sound����
            Sound s = Array.Find(sfxSounds, x => x.name == name);
            //����Ҳ�����Ӧ��Sound�����������Ϣ
            if (s == null)
            {
                Debug.Log("û���ҵ���Ч");
            }
            //���򲥷Ŷ�ӦSound��clip
            else
            {
                sfxSource.volume = volume;  // ��������
                sfxSource.PlayOneShot(s.clip);
            }
        }

        /// <summary>
        /// �л����ֵľ���״̬
        /// </summary>
        public void ToggleMusic()
        {
            musicSource.mute = !musicSource.mute;
        }

        /// <summary>
        /// �л���Ч�ľ���״̬
        /// </summary>
        public void ToggleSFX()
        {
            sfxSource.mute = !sfxSource.mute;
        }

        /// <summary>
        /// ��ָ������Դ�ļ����м��� Sound ��������
        /// </summary>
        /// <param name="folderName">��Դ�ļ�������</param>
        /// <returns>���ص� Sound ��������</returns>
        private Sound[] LoadSounds(string folderName)
        {
            // �� Resources �ļ����м������� AudioClip
            AudioClip[] audioClips = Resources.LoadAll<AudioClip>(folderName);

            // ���� Sound ���飬����ʼ��
            Sound[] sounds = new Sound[audioClips.Length];

            // ����ÿ�� AudioClip������ Sound ���󲢳�ʼ��
            for (int i = 0; i < audioClips.Length; i++)
            {
                AudioClip clip = audioClips[i];
                sounds[i] = new Sound
                {
                    name = clip.name,
                    clip = clip,
                    volume = 1.0f // Ĭ�����������Ը�����Ҫ�޸�
                };
            }

            return sounds;
        }
    }
}
