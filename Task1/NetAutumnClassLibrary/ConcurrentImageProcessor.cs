using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System;

namespace NetAutumnClassLibrary
{
    public class ConcurrentImageProcessor
    {
        public readonly ManualResetEvent     isStopped = new ManualResetEvent(false);

        readonly ConcurrentQueue<Prediction> mSummaryInfo = new ConcurrentQueue<Prediction>();

        readonly ConcurrentQueue<string>     imagePaths;

        public ConcurrentImageProcessor(string directoryPath)
        {
            imagePaths = new ConcurrentQueue<string>(Directory.GetFiles(directoryPath, "*.jpg"));
        }

        public string GetInfo()
        {
            if (mSummaryInfo.TryDequeue(out Prediction info))
                return $"Path: {info.Path} Label: {info.Label} Confidence: {info.Confidence}";
            else
                return "";
        }

        void ImageProcessingThread()
        {
            while (imagePaths.TryDequeue(out string name))
            {
                if (isStopped.WaitOne(0))
                {
                    Console.WriteLine("Stopping thread by signal.");
                    return;
                }
                SingleImageProcessor imageProcessor = new SingleImageProcessor(name);
                Prediction info = imageProcessor.GetPrediction();
                mSummaryInfo.Enqueue(info);
            }

            Console.WriteLine("Thread has finished working.");
        }

        public void Work()
        {
            int maxProcCount = Environment.ProcessorCount;
            var threads = new Thread[maxProcCount];

            for (int i = 0; i < maxProcCount; ++i)
            {
                Console.WriteLine($"Starting thread {i}.");
                threads[i] = new Thread(ImageProcessingThread);
                threads[i].Start();
            }

            for (var i = 0; i < maxProcCount; ++i)
            {
                threads[i].Join();
            }

            isStopped.Set();
        }
    }
}
