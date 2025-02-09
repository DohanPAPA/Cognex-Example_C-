/**
 * @file Program.cs
 * @brief Example demonstrating the use of the runtime library in a console application
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ViDi2;
using ViDi2.Local;

namespace Example.Runtime
{
    class Program
    {
        /**
         * @brief This example creates a VisionPro Deep Learning control, loads the runtime workspace
         * from the watch dial tutorial. Processes the blue, localize tool from
         * that workspace and prints out the score for each match and feature.
         * It also processes a red tool connected to the blue tool.
         */
        static void Main(string[] args)
        {
            // Initializes the control
            // This initialization does not allocate any gpu ressources.
            using(ViDi2.Runtime.Local.Control control = new ViDi2.Runtime.Local.Control(GpuMode.Deferred))
            {
                // Initializes all CUDA devices
                control.InitializeComputeDevices(GpuMode.SingleDevicePerTool, new List<int>() { });

                // set 3GB
                control.OptimizedGPUMemory((ulong)(3.0 * 1024 * 1024 * 1024));

                // Open a runtime workspace from file
                // the path to this file relative to the example root folder
                // and assumes the resource archive was extracted there.


                ViDi2.Runtime.IWorkspace workspace = control.Workspaces.Add("workspace", @"D:\temp\NORMAL TYPE(20240808).vrws");

                // Store a reference to the stream 'default'
                //IStream stream = workspace.Streams["default"];

                IStream stream = workspace.Streams["BOTTOM"];

                Stopwatch _stopwatch = new Stopwatch();

                string foldername = @"D:\temp\03. Bottom";
                DirectoryInfo di = new DirectoryInfo(foldername);

                FileInfo[] filelist = di.GetFiles();
                IImage img = new LibraryImage(filelist[0].FullName);
                stream.Process(img); // warm up

                using (StreamWriter file = new StreamWriter(@"test.csv"))
                {
                    file.WriteLine("ProcessTime (ms),Stopwatch Time (ms)");

                    for (int i = 0; i < filelist.Length; i++)
                    {
                        double processtime = 0;
                        using (IImage image = new LibraryImage(filelist[i].FullName))
                        {
                            _stopwatch.Reset();
                            _stopwatch.Start();
                            using (ISample sample = stream.Process(image))
                            {
                                processtime = 0;
                                foreach (var marking in sample.Markings)
                                {
                                    // check results
                                    processtime += marking.Value.Duration;
                                }
                                Console.WriteLine("Process : " + (processtime * 1000.0f) + "ms");
                            }
                            _stopwatch.Stop();
                            Console.WriteLine("Stopwatch : " + _stopwatch.ElapsedMilliseconds + "ms");
                        }
                        file.WriteLine("{0},{1}", (processtime * 1000).ToString(), _stopwatch.ElapsedMilliseconds.ToString());

                    }
                }

            }
            
        }
    }
}
