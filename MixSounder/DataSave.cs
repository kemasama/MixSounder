using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MixSounder
{
    class DataSave
    {
        public static void Save(DataSave.Data settings)
        {
            BinaryFormatter format = new BinaryFormatter();
            using (FileStream fs = new FileStream("settings.config", FileMode.Create))
            {
                format.Serialize(fs, settings);
                fs.Close();
            }
        }

        public static DataSave.Data Load()
        {
            Data result = new Data();

            BinaryFormatter format = new BinaryFormatter();
            if (File.Exists("settings.config"))
            {
                using (FileStream fs = new FileStream("settings.config", FileMode.Open))
                {
                    try
                    {
                        result = (DataSave.Data)format.Deserialize(fs);
                    }
                    catch
                    {
                        // not 
                    }

                    fs.Close();
                }
            }

            return result;
        }

        [Serializable]
        public class Data
        {
            public bool recordDesktop;
            public int[] inputNumbers;
            public int outputNumber;
            public int Axf_Output;
            public int FxE_Output;

            public Data()
            {
                recordDesktop = true;
                inputNumbers = new int[10];
                outputNumber = -1;

                for (int i = 0; i < inputNumbers.Length; i++)
                {
                    inputNumbers[i] = -1;
                }
            }
        }
    }
}
