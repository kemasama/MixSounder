﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixSounder.Effect
{
    class PitchEffect : Effect
    {
        int bufsize;
        float xfade;
        int bufloc0;
        int bufloc1;
        int buffer0;
        int buffer1;
        int bufdiff;
        float pitch;

        float denorm;
        bool filter;
        float v0;
        float h01, h02, h03, h04;
        float h11, h12, h13, h14;
        float a1, a2, a3, b1, b2;
        float t0, t1;
        float drymix;
        float wetmix;
        float[] buffer = new float[64000];

        public float cents { get; set; }
        public float semitones;
        public float octaves;

        /// <summary>
        /// Change Pitch
        /// </summary>
        /// <param name="c">Cents</param>
        /// <param name="s">Semitones</param>
        /// <param name="o">Octaves</param>
        public void changePitch(int c, int s, int o)
        {
            cents = (float)c;
            semitones = (float)s;
            octaves = (float)o;
            Slider();
        }

        public override string Name
        {
            get
            {
                return "SuperPitch";
            }
        }

        public override void Init()
        {
            bufsize = (int)SampleRate;
            xfade = 100;
            bufloc0 = 10000;
            bufloc1 = bufloc0 + bufsize + 1000;

            buffer0 = bufloc0;
            buffer1 = bufloc1;
            bufdiff = bufloc1 - bufloc0;
            pitch = 1.0f;
            denorm = pow(10, -20);

            cents = 0;
            semitones = 5;
            octaves = 0;

            Slider();
        }

        // n = default, minimum, maximum, increment
        // 1 = 0, -100, 100, 1   // cents pitch
        // 2 = 5, -12, 12, 1    // semitones pitch
        // 3 = 0, -8, 8, 1       // octaves pitch
        // 4 = 50, 1, 200, 1     // window size ms
        // 5 = 20, 0.5f, 50, 0.5f// overlap size ms
        // 6 = 0, -120, 6, 1     // wet mix dB
        // 7 = -120, -120, 6, 1  // dry mix dB
        // 8 = 1, 0, 1, 1 // filter

        protected void Slider()
        {
            filter = 1 > 0.5;
            int bsnew = (int)(Math.Min(50, 1000) * 0.001 * SampleRate);
            //   bsnew=(min(slider4,1000)*0.001*srate)|0;
            if (bsnew != bufsize)
            {
                bufsize = bsnew;
                v0 = buffer0 + bufsize * 0.5f;
                if (v0 > bufloc0 + bufsize)
                {
                    v0 -= bufsize;
                }
            }

            xfade = (int)(20 * 0.001 * SampleRate);
            if (xfade > bsnew * 0.5)
            {
                xfade = bsnew * 0.5f;
            }

            float npitch = pow(2, ((semitones + cents * 0.01f) / 12 + octaves));
            if (pitch != npitch)
            {
                pitch = npitch;
                float lppos = (pitch > 1.0f) ? 1.0f / pitch : pitch;
                if (lppos < (0.1f / SampleRate))
                {
                    lppos = 0.1f / SampleRate;
                }
                float r = 1.0f;
                float c = 1.0f / tan(PI * lppos * 0.5f);
                a1 = 1.0f / (1.0f + r * c + c * c);
                a2 = 2 * a1;
                a3 = a1;
                b1 = 2.0f * (1.0f - c * c) * a1;
                b2 = (1.0f - r * c + c * c) * a1;
                h01 = h02 = h03 = h04 = 0;
                h11 = h12 = h13 = h14 = 0;
            }

            drymix = pow(2, (-120 / 6));
            wetmix = pow(2, (0 / 6));
        }

        public override void onSample(ref float spl0, ref float spl1)
        {
            int iv0 = (int)(v0);
            float frac0 = v0 - iv0;
            int iv02 = (iv0 >= (bufloc0 + bufsize - 1)) ? iv0 - bufsize + 1 : iv0 + 1;

            float ren0 = (buffer[iv0 + 0] * (1 - frac0) + buffer[iv02 + 0] * frac0);
            float ren1 = (buffer[iv0 + bufdiff] * (1 - frac0) + buffer[iv02 + bufdiff] * frac0);
            float vr = pitch;
            float tv, frac, tmp, tmp2;
            if (vr >= 1.0)
            {
                tv = v0;
                if (tv > buffer0) tv -= bufsize;
                if (tv >= buffer0 - xfade && tv < buffer0)
                {
                    // xfade
                    frac = (buffer0 - tv) / xfade;
                    tmp = v0 + xfade;
                    if (tmp >= bufloc0 + bufsize) tmp -= bufsize;
                    tmp2 = (tmp >= bufloc0 + bufsize - 1) ? bufloc0 : tmp + 1;
                    ren0 = ren0 * frac + (1 - frac) * (buffer[(int)tmp + 0] * (1 - frac0) + buffer[(int)tmp2 + 0] * frac0);
                    ren1 = ren1 * frac + (1 - frac) * (buffer[(int)tmp + bufdiff] * (1 - frac0) + buffer[(int)tmp2 + bufdiff] * frac0);
                    if (tv + vr > buffer0 + 1) v0 += xfade;
                }
            }
            else
            {// read pointer moving slower than write pointer
                tv = v0;
                if (tv < buffer0) tv += bufsize;
                if (tv >= buffer0 && tv < buffer0 + xfade)
                {
                    // xfade
                    frac = (tv - buffer0) / xfade;
                    tmp = v0 + xfade;
                    if (tmp >= bufloc0 + bufsize) tmp -= bufsize;
                    tmp2 = (tmp >= bufloc0 + bufsize - 1) ? bufloc0 : tmp + 1;
                    ren0 = ren0 * frac + (1 - frac) * (buffer[(int)tmp + 0] * (1 - frac0) + buffer[(int)tmp2 + 0] * frac0);
                    ren1 = ren1 * frac + (1 - frac) * (buffer[(int)tmp + bufdiff] * (1 - frac0) + buffer[(int)tmp2 + bufdiff] * frac0);
                    if (tv + vr < buffer0 + 1) v0 += xfade;
                }
            }


            if ((v0 += vr) >= (bufloc0 + bufsize)) v0 -= bufsize;

            float os0 = spl0;
            float os1 = spl1;
            if (filter && pitch > 1.0)
            {

                t0 = spl0; t1 = spl1;
                spl0 = a1 * spl0 + a2 * h01 + a3 * h02 - b1 * h03 - b2 * h04 + denorm;
                spl1 = a1 * spl1 + a2 * h11 + a3 * h12 - b1 * h13 - b2 * h14 + denorm;
                h02 = h01; h01 = t0;
                h12 = h11; h11 = t1;
                h04 = h03; h03 = spl0;
                h14 = h13; h13 = spl1;
            }


            buffer[buffer0 + 0] = spl0; // write after reading it to avoid clicks
            buffer[buffer0 + bufdiff] = spl1;

            spl0 = ren0 * wetmix;
            spl1 = ren1 * wetmix;

            if (filter && pitch < 1.0)
            {
                t0 = spl0; t1 = spl1;
                spl0 = a1 * spl0 + a2 * h01 + a3 * h02 - b1 * h03 - b2 * h04 + denorm;
                spl1 = a1 * spl1 + a2 * h11 + a3 * h12 - b1 * h13 - b2 * h14 + denorm;
                h02 = h01; h01 = t0;
                h12 = h11; h11 = t1;
                h04 = h03; h03 = spl0;
                h14 = h13; h13 = spl1;
            }

            spl0 += os0 * drymix;
            spl1 += os1 * drymix;

            if ((buffer0 += 1) >= (bufloc0 + bufsize)) buffer0 -= bufsize;
        }
    }
}
