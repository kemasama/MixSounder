using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.MediaFoundation;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using MixSounder.Effect;

namespace MixSounder
{
    class Mixer
    {
        // Mixer
        private BufferedWaveProvider provider;
        private MixingWaveProvider32 mixProvider;

        private List<WaveMan> mixBuffered = new List<WaveMan>();

        private IWaveIn wavIn = null;
        private WaveOut rePlayer = null;

        private int muteTime = 0;

        public WaveFormat WaveFormat { get; set; }
        public float Volume
        {
            get
            {
                return CustomWaveProvider.Volume;
            }
            set
            {
                CustomWaveProvider.Volume = value;
            }
        }

        public void toggleMute()
        {
            Console.WriteLine("Toggle Mute CUR: " + rePlayer.PlaybackState);
            if (rePlayer.PlaybackState == PlaybackState.Playing)
            {
                rePlayer.Pause();
                muteTime = Environment.TickCount;
            } else
            {
                try
                {
                    int ticks = ((Environment.TickCount - muteTime) / 1000) + 1;
                    byte[] buffer = new byte[mixProvider.WaveFormat.AverageBytesPerSecond];
                    int count = mixProvider.WaveFormat.AverageBytesPerSecond;

                    Console.WriteLine("E: " + ticks);
                    Console.WriteLine("B: " + count);

                    for (int i = 0; i < ticks; i++)
                    {
                        mixProvider.Read(buffer, 0, count);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }

                // Soft Reset
                rePlayer.Init(mixProvider);
                rePlayer.Play();

                //rePlayer.Resume();
            }
        }

        public bool isMute()
        {
            return rePlayer.PlaybackState != PlaybackState.Playing;
        }

        /**
         * WaveIn waveIn  : Main Solution
         *  int   pnumber : Player device number
         *  
         *  If waveIn null, none capture
         */
        public void startMix(IWaveIn waveIn, int pnumber)
        {
            if (waveIn == null)
            {
                var wwww = new WasapiLoopbackCapture();
                provider = new BufferedWaveProvider(wwww.WaveFormat);
            }
            else
            {
                provider = new BufferedWaveProvider(waveIn.WaveFormat);

                waveIn.DataAvailable += (_, e) =>
                {
                    provider.AddSamples(e.Buffer, 0, e.BytesRecorded);
                };
            }

            WaveFormat = provider.WaveFormat;

            mixProvider = new MixingWaveProvider32();
            mixProvider.AddInputStream(provider);

            rePlayer = new WaveOut();
            rePlayer.DeviceNumber = pnumber;
            rePlayer.DesiredLatency = 100;
            rePlayer.Init(mixProvider);

            rePlayer.Play();

            this.wavIn = waveIn;
            waveIn?.StartRecording();
        }

        public void stopMix()
        {
            if (wavIn != null)
            {
                wavIn.StopRecording();
            }

            rePlayer?.Stop();
            rePlayer?.Dispose();
        }

        public void addMix(WaveMan man)
        {
            mixProvider.AddInputStream(man.getProvider());

            man.StartRecord();
            mixBuffered.Add(man);
        }

        public bool isMixing(WaveMan man)
        {
            return mixBuffered.IndexOf(man) != -1;
        }

        public void removeMix(WaveMan man)
        {
            mixProvider.RemoveInputStream(man.getProvider());
            man.StopRecord();
            mixBuffered.Remove(man);
        }

        public WaveMan getWaveMan(int n)
        {
            WaveMan man = null;

            foreach (var m in this.mixBuffered)
            {
                if (m.getWave().DeviceNumber == n)
                {
                    man = m;
                }
            }

            return man;
        }

        public void clearMix()
        {
            foreach (var man in this.mixBuffered)
            {
                mixProvider.RemoveInputStream(man.getProvider());
                man.StopRecord();
            }

            mixBuffered.Clear();
        }


    }
}
