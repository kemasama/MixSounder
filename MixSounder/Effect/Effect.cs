using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixSounder.Effect
{
    public abstract class Effect
    {
        public float SampleRate { get; set; }
        public float Tempo { get; set; }

        public Effect()
        {
            Tempo = 120;
            SampleRate = 44100;
        }

        public virtual void Init()
        {
        }

        public virtual void Block(int sampleBlock)
        {
        }

        public abstract void onSample(ref float left, ref float right);

        public abstract string Name { get; }
        protected float min(float a, float b) { return Math.Min(a, b); }
        protected float max(float a, float b) { return Math.Max(a, b); }
        protected float abs(float a) { return Math.Abs(a); }
        protected float exp(float a) { return (float)Math.Exp(a); }
        protected float sqrt(float a) { return (float)Math.Sqrt(a); }
        protected float sin(float a) { return (float)Math.Sin(a); }
        protected float tan(float a) { return (float)Math.Tan(a); }
        protected float cos(float a) { return (float)Math.Cos(a); }
        protected float pow(float a, float b) { return (float)Math.Pow(a, b); }
        protected float sign(float a) { return Math.Sign(a); }
        protected float log(float a) { return (float)Math.Log(a); }
        protected float PI { get { return (float)Math.PI; } }
    }
}
