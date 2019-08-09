using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using static System.Console;

namespace Transprite
{
    class Sprite
    {
        static void Main(string[] args)
        {
            const string ARGFILE = "./Sprite.txt";
            Start:
            string directory = null;
            string output = null;
            string animationFrames = null;
            switch(args.Length)
            {
                case 0:

                    Dictionary<string, string> prev = File.Exists(ARGFILE) ? Load(ARGFILE) : null;
                    string default_directory = prev?["Directory"] ?? Directory.GetCurrentDirectory();

                    WriteLine("File input directory?".PadRight(32) + writePrev(default_directory));
                    directory = ReadLine();
                    if (string.IsNullOrWhiteSpace(directory)) {
                        UsePrevious();
                        directory = default_directory;
                    }

                    //If we didn't choose the default directory, then we should adjust the suggested output to the chosen directory
                    string default_output = directory == default_directory ? prev?["Output"] ?? Path.Combine(directory, "Facings.png") : Path.Combine(directory, "Facings.png");
                    WriteLine("File output?".PadRight(32) + writePrev(default_output));
                    output = ReadLine();
                    if(string.IsNullOrWhiteSpace(output)) {
                        UsePrevious();
                        output = default_output;
                    }

                    string default_animationFrames = prev?["Animation Frames"] ?? "1";
                    WriteLine("Animation frames?".PadRight(32) + writePrev(default_animationFrames));
                    animationFrames = ReadLine();
                    if (string.IsNullOrWhiteSpace(animationFrames)) {
                        UsePrevious();
                        animationFrames = default_animationFrames;
                    }

                    void UsePrevious() {
                        Console.CursorTop--;
                        Console.WriteLine("Using previous");
                    }
                    string writePrev(string s) => string.IsNullOrWhiteSpace(s) ? "" : $"Previous: {s}";
                    break;
                case 1:
                    directory = args[0];
                    break;
                case 2:
                    directory = args[0];
                    output = args[1];
                    break;
                case 3:
                    directory = args[0];
                    output = args[1];
                    animationFrames = args[2];
                    break;
            }

            directory = directory ?? Directory.GetCurrentDirectory();
            output = output ?? Path.Combine(directory, "Facings.png");
            animationFrames = animationFrames ?? "1";

            var arguments = new Dictionary<string, string>{
                { "Directory", directory },
                { "Output", output },
                { "Animation Frames", animationFrames }
            };
            WriteLine(string.Join("\n", arguments.Select(p => $"{p.Key.PadRight(16)}: {p.Value}")));
            WriteLine();

            int animationFramesCount;
            try
            {
                animationFramesCount = int.Parse(animationFrames);
            } catch
            {
                WriteLine("Failure: Invalid Animation Frames");
                WriteLine();
                goto Start;
            }
            File.Delete(output);

            string[] files;
            try
            {
                files = Directory.GetFiles(directory);
            }
            catch {
                WriteLine("Failure: Invalid Directory");
                WriteLine();
                goto Start;
            };


            int count = files.Length;
            int facings = count / animationFramesCount;

            int rows = facings;
            int columns = animationFramesCount;


            var first = new Bitmap(files.First());
            var width = first.Width;
            var height = first.Height;
            Bitmap result = new Bitmap(width * columns, height * rows, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(result);


            bool ignore = false;
            for (int facing = 0; facing < rows; facing++) {
                for (int frame = 0; frame < columns; frame++)
                {
                    string file = files[facing + frame * facings];
                    var f = new Bitmap(file);
                    if(!ignore && !(f.Width == width && f.Height == height)) {
                        WriteLine($"Failure: {file} dimensions ({f.Width}, {f.Height}) do not match first frame dimensions ({width}, {height})");
                        WriteLine("Continue? (Y)");
                        if (ReadKey(true).Key == ConsoleKey.Y)
                            ignore = true;
                        else
                            goto Start;
                    }
                    g.DrawImage(f, frame * width, facing * height);
                    f.Dispose();
                    WriteLine($"Frame {frame} done");
                }
                WriteLine($"Facing {facing} done");
            }
            g.Dispose();
            //File.Create(output);
            //result.Save(output, ImageFormat.Png);

            using (MemoryStream memory = new MemoryStream()) {
                using (FileStream fs = new FileStream(output, FileMode.Create, FileAccess.ReadWrite)) {
                    result.Save(memory, ImageFormat.Png);
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            WriteLine($"Sprite done");
            result.Dispose();

            arguments.Save(ARGFILE);
            WriteLine();
            goto Start;
        }
        public static Dictionary<string, string> Load(string file)
        {
            //https://stackoverflow.com/a/16885493
            return File
                .ReadLines(file).Select((v, i) => new { Index = i, Value = v })
                .GroupBy(p => p.Index / 2)
                .ToDictionary(g => g.First().Value, g => g.Last().Value);
        }
    }
    public static class Helper
    {
        public static void Save(this Dictionary<string, string> dict, string file)
        {
            File.WriteAllLines(file, dict.Select(x => x.Key + Environment.NewLine + x.Value).ToArray());
        }
    }
}
