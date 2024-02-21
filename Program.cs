using System.Drawing;
using System.Drawing.Imaging;

class Stickerator
{
    static string[] names = ["normal",   "happy",    "pain",      "angry",      "worried",
                             "sad",      "crying",   "shouting",  "teary_eyed", "determined",
                             "joyous",   "inspired", "surprised", "dizzy",      "special0",
                             "special1", "sigh",     "stunned",   "special2",   "special3"];

    const int inputsize = 40;
    const int scale = 12;
    const int padding = 16;
    const int size = 512;

    static void Main(string[] args)
    {
        Console.WriteLine("SpritesheetIterator v1.0 started. Checking for files...");
        if(CheckInputFolder()) {
            int generated = 0;
            foreach (string file in Directory.EnumerateFiles("input"))
            {
                if (ProcessInput(file)) generated++;
                Console.WriteLine();
            }
            Console.WriteLine(String.Format("Process completed. Successfully generated {0} output folders.", generated));
        }
        Console.WriteLine();
        Console.WriteLine("Press any key to exit.");

        Console.ReadKey(true);
    }

    private static bool CheckInputFolder()
    {
        if (Directory.Exists("input")) return true;

        Directory.CreateDirectory("input");
        Console.WriteLine(String.Format("Input folder has been generated. Please place the images to process inside of it and restart the program."));
        return false;
    }

    private static bool ProcessInput(string filepath)
    {
        string filename = Path.GetFileNameWithoutExtension(filepath);
        Console.WriteLine(String.Format("File {0} found.", filename));

        Bitmap? img = LoadImage(filepath, filename);

        if(img == null) return false;
        // check image validity
        if (!CheckSizes(img)) {
            Console.WriteLine(String.Format("    Image \"output\\{0}\" is not in the correct spritesheet format.", filename));
            Console.WriteLine("    Images must be either 200x160 or 200x320 pixels exactly.");
            Console.WriteLine("    Skipping....");
            return false;
        }

        if (Directory.Exists(String.Format("output\\{0}", filename))) {
            Console.WriteLine(String.Format("    Folder \"output\\{0}\" already exists. Skipping...", filename));
            return false;
        }

        // Generate output dir
        Directory.CreateDirectory(String.Format("output\\{0}", filename));

        // Generate all new images
        Console.WriteLine(String.Format("    Processing \"{0}.png\"", filename));
        
        int generated = 0;
        for (int r = 0; r < 8; r++) {
            for (int c = 0; c < 5; c++) {
                if (img.Height / 40 <= r) break;

                Image? output = GenerateOutput(img, c, r);
                if (output == null) continue;
                int index = (r * 5) + c;
                bool is_mirror = index/20 != 0;
                index %= 20;

                string suffix = "";
                if (is_mirror) suffix = "_mirrored";
                string file = String.Format("output\\{0}\\{1}{2}.png", filename, names[index], suffix);
                output.Save(file, ImageFormat.Png);
                output.Dispose();
                generated++;
            }
        }
        img.Dispose();
        Console.WriteLine(String.Format("    \"{0}.png\" has been processed. Created {1} images", filename, generated));
        return true;
    }

    private static bool CheckSizes(Bitmap? img)
    {
        return (img.Height == 160 || img.Height == 320) && img.Width == 200;
    }

    private static Bitmap? LoadImage(string filepath, string filename)
    {
        Image? image = null;
        try
        {
            image = Image.FromFile(String.Format(filepath));
        }
        catch (Exception)
        {
            try
            {
                image = Image.FromFile(String.Format(filepath));
            }
            catch (Exception)
            {
                Console.WriteLine(String.Format("An error occurred while opening \"{0}.png\"", filename));
            }
        }
        if (image == null) return null;
        Bitmap img = new Bitmap(image);
        image.Dispose();
        return img;
    }

    private static Image? GenerateOutput(Bitmap img, int c, int r)
    {
        int startx = c * inputsize;
        int starty = r * inputsize;

        Bitmap output = new(size, size, img.PixelFormat);

        // Copy all pixels in the new size format
        bool brk = false;
        for (int i=0; i<inputsize; i++)
        {
            for(int j=0; j<inputsize; j++)
            {
                Color color = img.GetPixel(startx+i, starty+j);
                if (color.A == 0) {
                    brk = true;
                    break;
                }

                for (int h=0; h<scale; h++)
                {
                    for (int k=0; k<scale; k++)
                    {
                        int x = padding + (i*scale) + h;
                        int y = padding + (j*scale) + k;

                        output.SetPixel(x, y, color);
                    }
                }
            }
            if(brk) break;
        }
        // Return result
        if (brk) return null;
        return output;
    }
}