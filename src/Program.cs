////////////////////////////////////////////////////////////////////////
//
// This file is part of gmic-sharp-cli-example, a.NET Core-based
// CLI example application for gmic-sharp.
//
// Copyright (c) 2020 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using GmicSharp;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GmicSharpCliExample
{
    class Program
    {
        static string BuildGmicCommandString(List<string> commands)
        {
            int totalLength = 0;

            int count = commands.Count;
            int lastCommandIndex = count - 1;

            for (int i = 0; i < count; i++)
            {
                string command = commands[i];

                if (string.IsNullOrEmpty(command))
                {
                    continue;
                }

                totalLength += command.Length;

                if (i < lastCommandIndex)
                {
                    // Add the space that separates commands.
                    totalLength++;
                }
            }

            StringBuilder builder = new StringBuilder(totalLength);

            for (int i = 0; i < count; i++)
            {
                string command = commands[i];

                if (string.IsNullOrEmpty(command))
                {
                    continue;
                }

                builder.Append(command);

                if (i < lastCommandIndex)
                {
                    // Add the space that separates commands.
                    builder.Append(' ');
                }
            }

            return builder.ToString();
        }


        static void PrintUsage(OptionSet options)
        {
            Console.WriteLine("Usage: gmic-sharp-cli [OPTIONS] G'MIC commands");
            Console.WriteLine("Runs the specified G'MIC commands");
            Console.WriteLine();
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
        }

        static void Main(string[] args)
        {
            string inputImagePath = null;
            string outputFolderPath = null;
            bool showHelp = false;

            OptionSet options = new OptionSet
            {
                { "i|input=", "The input image path.", v => inputImagePath = v  },
                { "o|output-folder=", "The output folder path.", v => outputFolderPath = v  },
                { "h|help", "Print the help.", v => showHelp = v != null }
            };

            List<string> remaining = options.Parse(args);

            if (remaining.Count == 0 || showHelp)
            {
                PrintUsage(options);
            }
            else
            {
                List<GdiPlusGmicBitmap> inputImages = null;

                try
                {
                    Gmic<GdiPlusGmicBitmap> gmic = new Gmic<GdiPlusGmicBitmap>(new GdiPlusOutputImageFactory());

                    if (!string.IsNullOrEmpty(inputImagePath))
                    {
                        using (Bitmap bitmap = new Bitmap(inputImagePath))
                        {
                            inputImages = new List<GdiPlusGmicBitmap>(1)
                            {
                                new GdiPlusGmicBitmap(bitmap) { Name = "Image 1" }
                            };
                        }
                    }

                    string gmicCommands = BuildGmicCommandString(remaining);

                    using (CancellationTokenSource cts = new CancellationTokenSource())
                    {
                        Console.CancelKeyPress += new ConsoleCancelEventHandler(delegate (object sender, ConsoleCancelEventArgs e)
                        {
                            // Send a cancellation request to G'MIC, the process will exit after G'MIC finishes.
                            cts.Cancel();
                            e.Cancel = true;
                        });

                        Task<OutputImageCollection<GdiPlusGmicBitmap>> task = gmic.RunGmicAsync(gmicCommands, inputImages, cts.Token);

                        // Using WaitAny allows any exception that occurred
                        // during the task execution to be examined.
                        Task.WaitAny(task);

                        if (task.IsFaulted)
                        {
                            Exception exception = task.Exception.GetBaseException();

                            Console.WriteLine("Error running G'MIC: " + exception.Message);
                        }
                        else if (!task.IsCanceled)
                        {
                            OutputImageCollection<GdiPlusGmicBitmap> outputImages = task.Result;

                            if (outputImages.Count > 0)
                            {
                                if (string.IsNullOrEmpty(outputFolderPath))
                                {
                                    // If the user does not specify a folder for the output images
                                    // use a randomly named sub-folder in the application directory.
                                    string appDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                                    outputFolderPath = Path.Combine(appDirectory, Path.GetRandomFileName());

                                    Console.WriteLine("No output folder specified, using: {0}", outputFolderPath);
                                }

                                DirectoryInfo directoryInfo = new DirectoryInfo(outputFolderPath);
                                if (!directoryInfo.Exists)
                                {
                                    directoryInfo.Create();
                                }

                                for (int i = 0; i < outputImages.Count; i++)
                                {
                                    var image = outputImages[i];

                                    string path;

                                    if (string.IsNullOrWhiteSpace(image.Name))
                                    {
                                        path = Path.Combine(outputFolderPath, i.ToString(CultureInfo.CurrentCulture) + ".png");
                                    }
                                    else
                                    {
                                        path = Path.Combine(outputFolderPath, image.Name + ".png");
                                    }

                                    using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                                    {
                                        image.Image.Save(stream, ImageFormat.Png);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (AggregateException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (ExternalException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (GmicException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (NotSupportedException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (OutOfMemoryException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (SecurityException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (inputImages != null)
                    {
                        for (int i = 0; i < inputImages.Count; i++)
                        {
                            inputImages[i]?.Dispose();
                        }
                    }
                }
            }
        }
    }
}
