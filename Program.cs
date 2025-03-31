using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace stegano;

public class ImagePixel
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class Program
{
    private static List<ImagePixel> _modifiedPixels = new List<ImagePixel>();

    private static void EmbedText(string message, string inputImagePath, string outputImagePath)
    {
        using (Image<Rgba32> image = Image.Load<Rgba32>(inputImagePath))
        {
            int charIndex = 0;
            int charValue = 0;
            int bitIndex = 0;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    if (charIndex >= message.Length && bitIndex == 0)
                    {
                        image.Save(outputImagePath);
                        return;
                    }

                    Rgba32 pixel = image[x, y];
                    // Save modified pixels (used with -H flag)
                    _modifiedPixels.Add(new ImagePixel
                    {
                        X = x,
                        Y = y
                    });
                    // Totally did not steal this
                    pixel.R = (byte)(pixel.R & ~1 | GetBit(message, ref charIndex, ref charValue, ref bitIndex));
                    pixel.G = (byte)(pixel.G & ~1 | GetBit(message, ref charIndex, ref charValue, ref bitIndex));
                    pixel.B = (byte)(pixel.B & ~1 | GetBit(message, ref charIndex, ref charValue, ref bitIndex));
                    image[x, y] = pixel;
                }
            }

            image.Save(outputImagePath);
        }
    }

    // Extract hidden text from a PNG image
    private static string ExtractText(string inputImagePath)
    {
        using (Image<Rgba32> image = Image.Load<Rgba32>(inputImagePath))
        {
            StringBuilder extractedMessage = new StringBuilder();
            int charValue = 0;
            int bitIndex = 0;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Rgba32 pixel = image[x, y];

                    charValue = (charValue << 1) | (pixel.R & 1);
                    bitIndex++;
                    if (bitIndex == 8)
                    {
                        if (charValue == 0)
                        {
                            return extractedMessage.ToString();
                        }

                        extractedMessage.Append((char)charValue);
                        charValue = 0;
                        bitIndex = 0;
                    }

                    charValue = (charValue << 1) | (pixel.G & 1);
                    bitIndex++;
                    if (bitIndex == 8)
                    {
                        if (charValue == 0)
                        {
                            return extractedMessage.ToString();
                        }

                        extractedMessage.Append((char)charValue);
                        charValue = 0;
                        bitIndex = 0;
                    }

                    charValue = (charValue << 1) | (pixel.B & 1);
                    bitIndex++;
                    if (bitIndex == 8)
                    {
                        if (charValue == 0)
                        {
                            return extractedMessage.ToString();
                        }

                        extractedMessage.Append((char)charValue);
                        charValue = 0;
                        bitIndex = 0;
                    }
                }
            }

            return extractedMessage.ToString();



        }
    }

    private static int GetBit(string message, ref int charIndex, ref int charValue, ref int bitIndex)
    {
        if (bitIndex == 0)
        {
            charValue = charIndex < message.Length ? message[charIndex++] : 0;
        }
        // Totally did not steal this also
        int bit = (charValue >> (7 - bitIndex)) & 1;
        bitIndex = (bitIndex + 1) % 8;
        return bit;
    }

    private static void HighlightModified(string inputImagePath, string outputImagePath)
    {
        using (Image<Rgba32> image = Image.Load<Rgba32>(inputImagePath))
        {
            int count = 0;
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    // colour modified pixels in blue and red
                    foreach (ImagePixel location in _modifiedPixels)
                    {
                        //  Console.WriteLine($"X:{location.X} Y:{location.Y}");
                        var pixel = image[location.X, location.Y];
                        if (count % 2 == 0)
                        {
                            pixel.B = 0;
                            pixel.G = 0;
                            pixel.R = 255;
                        }
                        else
                        {
                            //RGB(139, 233, 253)
                            pixel.B = 255;
                            pixel.G = 0;
                            pixel.R = 0;
                        }

                        count++;
                        image[location.X, location.Y] = pixel;
                    }

                }
            }

            image.Save(outputImagePath);

        }
    }

    private static void HelpMenu()
    {
        Console.WriteLine("usage: ./stegano -m <message> -i <inputfile> -o <outputfile>");
        Console.WriteLine("usage: ./stegano -m <message> -i <inputfile> -o <outputfile>");
        Console.WriteLine("usage: ./stegano -d <image to decrypt>");
        Console.WriteLine("optionally use the -h flag to output an additional image displaying the modified pixels");
        Environment.Exit(exitCode: 0);
    }

   
    public static void Main(string[] args)
    {
       
        
        if (args.Length == 0 || args.Contains("--help"))
        {
            HelpMenu();
            return;
        }

        string inputPath = string.Empty;
        string outputPath = string.Empty;
        string message = string.Empty;
        string decryptPath = string.Empty;
        bool highlight = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-i":
                    inputPath = args[i + 1];
                    i++;
                    break;
                case "-o":
                    outputPath = args[i + 1];
                    i++;
                    break;
                case "-m":
                    message = args[i + 1];
                    i++;
                    break;
                case "-d":
                    decryptPath = args[i + 1];
                    i++;
                    break;
                case "-h":
                    highlight = true;
                    break;
                case "--help":
                    HelpMenu();
                    break;
                default:
                    Console.WriteLine($"Flag {args[i + 1]} does not exist");
                    return;
            }
        }

        try
        {
            if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(inputPath) && !string.IsNullOrEmpty(outputPath))
            {
                EmbedText(message, inputPath, outputPath);
                Console.WriteLine("Message has been embedded");
            }

            if (!string.IsNullOrEmpty(decryptPath))
            {
                string extractedMessage = ExtractText(decryptPath);
                Console.WriteLine($"Extracted message: {extractedMessage}");
            }

            if (highlight && !string.IsNullOrEmpty(outputPath))
            {
                HighlightModified(outputPath, "modifiedPixels-" + outputPath);
                Console.WriteLine("Modified pixels highlighted");
            }
        }
        catch (Exception ex)
        {
                
            Console.WriteLine($"error: {ex.Message}");
        }
    }
    

}