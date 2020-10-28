using NetAutumnClassLibrary;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

using System.Windows;
using System.Runtime.CompilerServices;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace WpfView
{
    class ViewModel : INotifyPropertyChanged
    {
        private readonly SynchronizationContext initialContext;

        public event PropertyChangedEventHandler PropertyChanged;

        private delegate void InterruptingHandler();
        private event InterruptingHandler Interrupt;
        
        private ConcurrentImageProcessor mConcurrentImageProcessor;

        private string mCurrentChosenDirectory = Directory.GetCurrentDirectory();
        public string CurrentChosenDirectory { 
            get 
            { 
                return mCurrentChosenDirectory; 
            } 
            set 
            { 
                mCurrentChosenDirectory = value;
                NotifyPropertyChanged(); 
            } 
        }

        public bool IsProcessingNow { get; private set; } = false;

        public ObservableCollection<ClassOfImages> ClassesOfImagesList { get; }

        public ViewModel()
        {
            initialContext = SynchronizationContext.Current;
            ClassesOfImagesList = new ObservableCollection<ClassOfImages>();
            Interrupt += () =>
            {
                mConcurrentImageProcessor.isStopped.Set();
            };
        }
        public void Start()
        {
            IsProcessingNow = true;
            var processing = new Thread(new ThreadStart(() =>
            {
               this.Process();
            }));
            processing.Start();
        }

        public void Process()
        {
            mConcurrentImageProcessor = new ConcurrentImageProcessor(CurrentChosenDirectory);

            var writing = new Thread(new ThreadStart(() =>
            {
                string info = "";
                Prediction recievedPrediction = new Prediction();
                for ( ; ; )
                {
                    if (mConcurrentImageProcessor.isStopped.WaitOne(0))
                        break;
                    if ((info = mConcurrentImageProcessor.GetInfo()) != "")
                    {
                        string[] wordsFromInfo = info.Split(new string[] { ": " }, StringSplitOptions.None);
                        recievedPrediction.Path = wordsFromInfo[1].Remove(wordsFromInfo[1].LastIndexOf(' '));
                        recievedPrediction.Label = wordsFromInfo[2].Remove(wordsFromInfo[2].LastIndexOf(' '));
                        recievedPrediction.Confidence = float.Parse(wordsFromInfo[3]);
                        AddImageToList(recievedPrediction);
                    }
                }

            }));

            writing.Start();
            mConcurrentImageProcessor.Work();

            writing.Join();

            IsProcessingNow = false;

            return;
        }

        private void AddImageToList(Prediction prediction)
        {
            Image image = Image.Load(prediction.Path);

            foreach (var item in ClassesOfImagesList)
                if (item.Name == prediction.Label)
                {
                    initialContext.Send(x => item.Images.Add(prediction.Path), null);
                    return;
                }

            ObservableCollection<string> newImageType = new ObservableCollection<string> { prediction.Path };
            ClassOfImages newClassOfImages = new ClassOfImages
            {
                Name = prediction.Label,
                Images = newImageType
            };
            initialContext.Send(x => ClassesOfImagesList.Add(newClassOfImages), null);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Stop()
        {
            Interrupt.Invoke();
        }
    }
}
