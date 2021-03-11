using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using NAudio.MediaFoundation;
using NAudio.Wave;
using DevRas;

namespace MixSounder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            this.tswitch = new ToggleSwitch();
            this.tswitch.Location = new Point(260, 196);
            this.tswitch.Size = new Size(50, 20);
            this.tswitch.Checked = true;

            this.Controls.Add(this.tswitch);

            this.muteHotKey = new HotKey(MOD_KEY.SHIFT, Keys.M);
            this.muteHotKey.HotKeyPush += new EventHandler(toggleMute);
        }

        private DataSave.Data saveData;
        private ToggleSwitch tswitch;
        private HotKey muteHotKey;

        private Mixer mixer = new Mixer();
        private bool isMix = false;

        private void Form1_Load(object sender, EventArgs e)
        {
            saveData = DataSave.Load();
            UpdateDevices();

            /*
            Console.Write("Outputs: ");
            Console.WriteLine(saveData.outputNumber);
            Console.Write("Inputs: ");
            for (int i = 0; i < saveData.inputNumbers.Length; i++)
            {
                Console.Write(saveData.inputNumbers[i]);
                Console.Write(", ");
            }
            Console.WriteLine(";");
            Console.WriteLine("RecordDesktop: " + saveData.recordDesktop);
            */
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            muteHotKey.Dispose();
            DataSave.Save(saveData);
        }

        public void UpdateDevices()
        {
            try
            {
                this.tswitch.Checked = saveData.recordDesktop;
                // Clear
                listBox1.Items.Clear();
                checkedListBox1.Items.Clear();

                // Output
                for (int i = 0; i < WaveOut.DeviceCount; i++)
                {
                    var device = WaveOut.GetCapabilities(i);
                    listBox1.Items.Add(device.ProductName);
                }

                // Input
                for (int i = 0; i < WaveIn.DeviceCount; i++)
                {
                    var device = WaveIn.GetCapabilities(i);
                    checkedListBox1.Items.Add(device.ProductName);
                }

                for (int i = 0; i < saveData.inputNumbers.Length; i++)
                {
                    if (saveData.inputNumbers[i] != -1)
                    {
                        checkedListBox1.SetItemChecked(saveData.inputNumbers[i], true);
                    }
                }

                listBox1.SelectedIndex = saveData.outputNumber;

            } catch (Exception /*e*/)
            {
                // e
            }

            EmurateDevices();
        }

        public void EmurateDevices()
        {
            var emu = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            foreach (var device in emu)
            {
            }
        }

        private delegate void SafeCallUpdate(Control control, String propertyName, object propertyValue);
        public void SafeUpdate(Control control, String property, object value)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SafeCallUpdate(SafeUpdate), new object[] { control, property, value });
            }
            else
            {
                control.GetType().InvokeMember(
                    property,
                    System.Reflection.BindingFlags.SetProperty,
                    null,
                    control,
                    new object[] { value });
            }
        }
        public void UpdateProgress(object _, WaveInEventArgs e)
        {
            int index = 0;
            float maxV = 0;
            var buffer = new WaveBuffer(e.Buffer);
            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                short sample = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index + 0]);
                var sample32 = sample / 32768f;

                if (sample32 < 0)
                {
                    sample32 = -sample32;
                }
                if (sample32 > maxV)
                {
                    maxV = sample32;
                }
            }

            //Console.WriteLine(maxV * 100);

            try
            {
                SafeUpdate(this.progressBar1, "Value", (int)(100 * maxV));
            }
            catch (Exception)
            {
                Console.WriteLine("Error at #UpdateProgress");
            }
        }

        public void toggleMute(object sender, EventArgs e)
        {
            if (mixer != null)
            {
                mixer.toggleMute();
            }
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            if (isMix)
            {
                return;
            }

            isMix = true;

            int pn = this.listBox1.SelectedIndex;
            if (pn == -1 || pn > this.listBox1.Items.Count)
            {
                pn = 0;
                this.listBox1.SelectedIndex = 0;
            }

            mixer.clearMix();

            // Capture Desktop Audio
            var win = new WasapiLoopbackCapture();

            win.DataAvailable += (_, ev) =>
            {
                UpdateProgress(_, ev);
            };

            if (this.tswitch.Checked)
            {
                mixer.startMix(win, pn);
            } else
            {
                mixer.startMix(null, pn);
            }

            foreach (int min in this.checkedListBox1.CheckedIndices)
            {
                if (min >= WaveIn.DeviceCount)
                {
                    continue;
                }

                var win2 = new WaveIn();
                win2.WaveFormat = win.WaveFormat;
                win2.DeviceNumber = min;
                var man = new WaveMan(win2);
                mixer.addMix(man);
            }


            saveData.recordDesktop = this.tswitch.Checked;
            saveData.outputNumber = pn;
            for (int i = 0; i < this.checkedListBox1.CheckedIndices.Count; i++)
            {
                if (saveData.inputNumbers.Length > i)
                {
                    saveData.inputNumbers[i] = this.checkedListBox1.CheckedIndices[i];
                }
            }

            this.listBox1.Enabled = false;
            //this.checkedListBox1.Enabled = false;
            this.tswitch.Enabled = false;
        }

        private void stopBtn_Click(object sender, EventArgs e)
        {
            if (!isMix)
            {
                return;
            }

            isMix = false;

            mixer.stopMix();

            this.listBox1.Enabled = true;
            this.checkedListBox1.Enabled = true;
            this.tswitch.Enabled = true;

            SafeUpdate(this.progressBar1, "Value", 0);

        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (mixer == null || mixer.WaveFormat == null)
            {
                return;
            }

            //mixer.clearMix();

            foreach (int min in this.checkedListBox1.CheckedIndices)
            {
                if (min >= WaveIn.DeviceCount)
                {
                    continue;
                }

                Console.WriteLine(checkedListBox1.Items[min]);

                var win2 = new WaveIn();
                win2.WaveFormat = mixer.WaveFormat;
                win2.DeviceNumber = min;
                var man = new WaveMan(win2);
                if (mixer.isMixing(man))
                {
                    // Skip
                    continue;
                }
                mixer.addMix(man);
            }
        }

        private void volumeBar_Scroll(object sender, EventArgs e)
        {
            float f = (float) volumeBar.Value;
            float vol = (f / 20);
            mixer.Volume = vol;
        }
    }
}
