using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace MixSounder
{
    class CustomWaveProvider : IWaveProvider
    {
        // Global
        public static float Volume
        {
            get;
            set;
        }

        public CustomWaveProvider(IWaveProvider sourceProvider)
        {
            this.sourceProvider = sourceProvider;
        }

        private readonly IWaveProvider sourceProvider;
        private float volume
        {
            get
            {
                return Volume;
            }
            set
            {
                Volume = value;
            }
        }

        public WaveFormat WaveFormat
        {
            get
            {
                return sourceProvider.WaveFormat;
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = sourceProvider.Read(buffer, offset, count);
            if (volume == 0.0f)
            {
                for (int n = 0; n < bytesRead; n++)
                {
                    buffer[offset++] = 0;
                }
            } else
            {
                for (int n = 0; n < bytesRead; n += 2)
                {
                    short sample = (short)((buffer[offset + 1] << 8) | buffer[offset]);
                    var newSample = sample * this.volume;
                    sample = (short)newSample;

                    if (this.volume > 1.0f)
                    {
                        if (newSample > Int16.MaxValue) sample = Int16.MaxValue;
                        else if (newSample < Int16.MinValue) sample = Int16.MinValue;
                    }

                    buffer[offset++] = (byte)(sample & 0xFF);
                    buffer[offset++] = (byte)(sample >> 8);
                }
            }

            return bytesRead;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == this.GetType())
            {
                return sourceProvider.Equals(obj);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return sourceProvider.GetHashCode();
            //return base.GetHashCode();
        }
    }
}
