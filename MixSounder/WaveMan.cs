using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace MixSounder
{
    class WaveMan
    {
        private WaveIn waveIn;
        private BufferedWaveProvider provider;

        public WaveMan(WaveIn win)
        {
            this.waveIn = win;
            this.provider = new BufferedWaveProvider(win.WaveFormat);
        }

        public void StartRecord()
        {
            this.waveIn.DataAvailable += (_, e) =>
            {
                this.provider.AddSamples(e.Buffer, 0, e.BytesRecorded);
            };

            this.waveIn.StartRecording();
        }

        public void StopRecord()
        {
            this.waveIn.StopRecording();
        }

        public void Dispose()
        {
            this.waveIn.Dispose();
            this.provider = null;
            this.waveIn = null;
        }

        public WaveIn getWave()
        {
            return waveIn;
        }

        public BufferedWaveProvider getProvider()
        {
            return provider;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            WaveMan c = (WaveMan)obj;
            if (c.getWave().DeviceNumber == this.waveIn.DeviceNumber)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return waveIn.DeviceNumber;
        }
    }
}
