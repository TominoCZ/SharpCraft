using NAudio.Vorbis;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace SharpCraft.sound
{
    internal static class SoundEngine
    {
        private static readonly ConcurrentDictionary<string, ValueTuple<int, int>> Sounds = new ConcurrentDictionary<string, (int, int)>();

        private static AudioContext _audioContext;

        static SoundEngine()
        {
            _audioContext = new AudioContext();
        }

        public static void RegisterSound(string soundName)
        {
            var file = $"{SharpCraft.Instance.GameFolderDir}/assets/sharpcraft/sounds/{soundName}.ogg";

            if (!File.Exists(file))
            {
                Console.WriteLine($"ERROR: Couldn't find sound '{soundName}'");
                return;
            }

            using (var wr = new VorbisWaveReader(file))
            {
                byte[] data = new byte[wr.Length];
                wr.Read(data, 0, data.Length);

                int buffer = AL.GenBuffer();
                int source = AL.GenSource();

                AL.BufferData(buffer, ALFormat.VorbisExt, data, data.Length, wr.WaveFormat.SampleRate);

                AL.Source(source, ALSourcei.Buffer, buffer);
                AL.BindBufferToSource(source, buffer);

                Sounds.TryAdd(soundName, new ValueTuple<int, int>(source, buffer));
            }
        }

        public static void PlaySound(string soundName, float volume = 1)
        {
            //if (Sounds.TryGetValue(soundName, out var sound))
            //{
            //AL.SourcePlay(sound.Item1);
            //}
        }

        public static void StopSound(string soundName)
        {
            //if (Sounds.TryGetValue(soundName, out var sound))
            //{
            //AL.SourceStop(sound.Item1);
            //}
        }

        public static void Reload()
        {
            var bkp = new Dictionary<string, ValueTuple<int, int>>(Sounds);

            Destroy();

            foreach (var pair in bkp)
            {
                RegisterSound(pair.Key);
            }
        }

        public static void Destroy()
        {
            foreach (var pair in Sounds)
            {
                var source = pair.Value.Item1;
                var buffer = pair.Value.Item2;

                AL.DeleteSource(source);
                AL.DeleteBuffer(buffer);
            }

            Sounds.Clear();
        }
    }
}