using SixLabors.ImageSharp;

using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace WpfView
{
    class ClassOfImages : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string mName;
        public string Name 
        { 
            get
            {
                return mName;
            }
            set
            {
                mName = value;
                NotifyPropertyChanged();
            }
        }

        private int mCount;
        public int Count
        {
            get
            {
                return mCount;
            }
            private set
            {
                mCount = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<string> mImages;
        public ObservableCollection<string> Images
        {
            get
            {
                return mImages;
            }
            set
            {
                Count = value.Count;
                mImages = value;
                mImages.CollectionChanged += ImageListChanged;
            }
        }

        public ClassOfImages()
        {
            mImages = new ObservableCollection<string>();
        }

        public void ImageListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Count = Images.Count();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
