using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vosk;

namespace ConsoleApp1
{
    class Program
    {
        private static string modelPath_EN = Path.Combine(Directory.GetCurrentDirectory(), "vosk-model-en-us-0.20");
        private static string modelPath_RU = Path.Combine(Directory.GetCurrentDirectory(), "vosk-model-ru-0.10");
        
        private const string voicePath = @"C:\Users\sss63\YandexDisk\CreditDepartament\Temp\Sound_22123.wav";

        private static List<byte> micBuffer;
        private static Model model;

        static void Main(string[] args)
        {
            Vosk.Vosk.SetLogLevel(-1); // что бы не писал лог

            Console.WriteLine("Select a language (Выберите язык):");
            Console.WriteLine("1 - English (Английский)");
            Console.WriteLine("2 - Russian (Русский)");
            var landNumStr = Console.ReadLine();

            if (int.TryParse(landNumStr, out int langNum))
            {
                switch (langNum)
                {
                    case 1:
                        Console.WriteLine("The language model is being loaded... (Идет загрузка языковой модели...)");
                        model = new Model(modelPath_EN);
                        break;
                    case 2:
                        Console.WriteLine("The language model is being loaded... (Идет загрузка языковой модели...)");
                        model = new Model(modelPath_RU);
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("language selection error (ошибка выбора языка)");
                        Console.ResetColor();

                        Console.WriteLine("Press any key to exit (Нажмите любую клавишу для выхода)");
                        Console.ReadKey();
                        return;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("language selection error (ошибка выбора языка)");
                Console.ResetColor();

                Console.WriteLine("Press any key to exit (Нажмите любую клавишу для выхода)");
                Console.ReadKey();
                return;
            }

            micBuffer = new List<byte>();
            NAudioTest();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("I'm ready to listen... (Я готов слушать...)");
            Console.ResetColor();

            Console.Read();
        }

        #region NAudio
        private static void NAudioTest()
        {
            var waveIn = new WaveInEvent();
            waveIn.DataAvailable += WaveOnDataAvailable;
            waveIn.WaveFormat = new WaveFormat(16000, 1);
            waveIn.StartRecording();
        }

        static bool isRecord = false;
        static int countLevelDown = 0;

        private static void WaveOnDataAvailable(object sender, WaveInEventArgs e)
        {
            var levels = new List<float>();

            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index + 0]);

                float amplitude = sample / 32768f;
                float level = Math.Abs(amplitude);
                levels.Add(level);
            }

            var levelAvg = levels.Average() * 100;
            if (levelAvg > 7.5)
            {
                //Console.ForegroundColor = ConsoleColor.Green;
                //Console.WriteLine("Уровень: {0}%.", Math.Round(levelAvg, 2));
                //Console.ResetColor();

                countLevelDown = 0;
                isRecord = true;
            }
            else if(levelAvg < 5)
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.WriteLine("Уровень: {0}%.", Math.Round(levelAvg, 2));
                //Console.ResetColor();

                countLevelDown++;
                if (countLevelDown > 10)
                {
                    isRecord = false;
                    countLevelDown = 0;
                }
            }

            if (isRecord)
            {
                micBuffer.AddRange(e.Buffer);
            }
            else
            {
                if (micBuffer?.Count > 0)
                {
                    DemoBytes1(micBuffer.ToArray());
                    micBuffer.Clear();
                }
            }
            
        }
        #endregion

        #region Vosk
        private static void VoskTest()
        {
            //Model model = new Model(modelPath);
            //DemoBytes(model);
            //DemoFloats(model);
            //DemoSpeaker(model);
        }

        private static void DemoBytes1(byte[] buffer)
        {
            // Demo byte buffer
            VoskRecognizer rec = new VoskRecognizer(model, 16000.0f);
            rec.SetMaxAlternatives(0);
            rec.SetWords(true);
            if (rec.AcceptWaveform(buffer, buffer.Length))
            {
                //Console.WriteLine($"--- {rec.Result()}");
            }
            else
            {
                //Console.WriteLine($"=== {rec.PartialResult()}");
            }
            //Console.WriteLine($"+++ {rec.FinalResult()}");

            string words = JConverter.JsonConvertDynamic(rec.FinalResult()).text;
            if (!string.IsNullOrWhiteSpace(words))
            {
                Console.WriteLine(words);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("............");
                Console.ResetColor();
            }
        }

        public static void DemoBytes(Model model)
        {
            // Demo byte buffer
            VoskRecognizer rec = new VoskRecognizer(model, 16000.0f);
            rec.SetMaxAlternatives(0);
            rec.SetWords(true);
            using (Stream source = File.OpenRead(voicePath))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (rec.AcceptWaveform(buffer, bytesRead))
                    {
                        Console.WriteLine(rec.Result());
                    }
                    else
                    {
                        Console.WriteLine(rec.PartialResult());
                    }
                }
            }
            Console.WriteLine(rec.FinalResult());
        }

        public static void DemoFloats(Model model)
        {
            // Demo float array
            VoskRecognizer rec = new VoskRecognizer(model, 16000.0f);
            using (Stream source = File.OpenRead(voicePath))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    float[] fbuffer = new float[bytesRead / 2];
                    for (int i = 0, n = 0; i < fbuffer.Length; i++, n += 2)
                    {
                        fbuffer[i] = (short)(buffer[n] | buffer[n + 1] << 8);
                    }
                    if (rec.AcceptWaveform(fbuffer, fbuffer.Length))
                    {
                        Console.WriteLine(rec.Result());
                    }
                    else
                    {
                        Console.WriteLine(rec.PartialResult());
                    }
                }
            }
            Console.WriteLine(rec.FinalResult());
        }

        public static void DemoSpeaker(Model model)
        {
            // Output speakers
            SpkModel spkModel = new SpkModel("model");
            VoskRecognizer rec = new VoskRecognizer(model, 16000.0f);
            rec.SetSpkModel(spkModel);

            using (Stream source = File.OpenRead(voicePath))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (rec.AcceptWaveform(buffer, bytesRead))
                    {
                        Console.WriteLine(rec.Result());
                    }
                    else
                    {
                        Console.WriteLine(rec.PartialResult());
                    }
                }
            }
            Console.WriteLine(rec.FinalResult());
        }
        #endregion
    }
}
