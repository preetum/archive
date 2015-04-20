using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace MainGame
{
    public static class SoundManager
    {
        static SoundEffectInstance theme1;
        static SoundEffectInstance theme2;
        static SoundEffect shotEf;
        static SoundEffect sonarEf;
        static SoundManager()
        {
            var theme_f = TitleContainer.OpenStream(@"Content\RunningBlind_snd.wav");
            var anxiety = TitleContainer.OpenStream(@"Content\Anxiety_snd.wav");
            var shotf = TitleContainer.OpenStream(@"Content\Shot_snd.wav");
            var sonarf = TitleContainer.OpenStream(@"Content\Sonar_snd.wav");


            var theme_ef = SoundEffect.FromStream(theme_f);
            var theme_eft = SoundEffect.FromStream(anxiety);
            shotEf = SoundEffect.FromStream(shotf);
            sonarEf = SoundEffect.FromStream(sonarf);

            theme1 = theme_ef.CreateInstance();
            theme1.IsLooped = true;
            theme1.Volume = 1f;
            theme2 = theme_eft.CreateInstance();
            theme2.IsLooped = true;
            theme2.Volume = 0f;
        }
        static float vol2 = 0f;
        public static void StartTheme()
        {
            theme1.Play();
            theme2.Play();
        }
        public static void StopTheme()
        {
            theme1.Stop();
            theme2.Stop();
        }
        public static void StopAll()
        {
            StopTheme();
        }
        public static void PushTheme2()
        {
            vol2 = 0.5f;
        }
        public static void Shot()
        {
            var shot = shotEf.CreateInstance();
            shot.IsLooped = false;
            shotEf.Play();
        }
        public static void Sonar()
        {
            var son = sonarEf.CreateInstance();
            son.IsLooped = false;
            son.Play();
        }
        public static void Update(GameTime gt)
        {
            float volScale = 0.5f;
            theme1.Volume = volScale * (1f - vol2);
            theme2.Volume = volScale * (vol2);

            vol2 -= (float)gt.ElapsedGameTime.TotalSeconds * 0.1f;
            if (vol2 < 0)
                vol2 = 0;
        }
    }
}
