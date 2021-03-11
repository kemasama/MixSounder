using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace MixSounder.Effect
{
    class EffectStream : ISampleProvider
    {
        private ISampleProvider provider;
        private object effectLock = new object();
        private List<Effect> effects = new List<Effect>();

        public EffectStream(ISampleProvider sourceProvider)
        {
            this.provider = sourceProvider;
        }

        public WaveFormat WaveFormat
        {
            get
            {
                return provider.WaveFormat;
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = provider.Read(buffer, offset, count);

            lock (effectLock)
            {
                Process(buffer, offset, read);
            }

            return read;
        }

        private void Process(float[] buffer, int offset, int count)
        {
            int samples = count;
            foreach (var effect in effects)
            {
                effect.Block(samples);
            }

            for (int sample = 0; sample < samples; sample++)
            {
                float sampleLeft = buffer[offset];
                float sampleRight = sampleLeft;

                if (WaveFormat.Channels == 2)
                {
                    sampleRight = buffer[offset + 1];
                    sample++;
                }

                foreach (var effect in effects)
                {
                    effect.onSample(ref sampleLeft, ref sampleRight);
                }

                buffer[offset++] = sampleLeft;
                if (WaveFormat.Channels == 2)
                {
                    buffer[offset++] = sampleRight;
                }
            }
        }

        public void UpdateEffect(Effect eff)
        {
            eff.SampleRate = WaveFormat.SampleRate;
            eff.Init();

            if (effects.Contains(eff))
            {
                effects.Remove(eff);
            }

            effects.Add(eff);
        }
    }
}
