using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace headTracking
{
    class Webcam
    {
        public FilterInfoCollection VideoCaptureDevices;
        public VideoCaptureDevice FinalVideo;
        
        public Webcam()
        {
            VideoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach(FilterInfo VideoCaptureDevice in VideoCaptureDevices)
            {
                ((MainWindow)Application.Current.MainWindow).devices.Items.Add(VideoCaptureDevice.Name);
            }

            ((MainWindow)Application.Current.MainWindow).devices.SelectedIndex = 0;
            FinalVideo = new VideoCaptureDevice();
        }

        public BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        public void OpenVideoSource()
        {
            if (FinalVideo.IsRunning == true) FinalVideo.Stop();
            
            FinalVideo = new VideoCaptureDevice(VideoCaptureDevices[((MainWindow)Application.Current.MainWindow).devices.SelectedIndex].MonikerString);
            FinalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);

            FinalVideo.Start();
        }

        private void FinalVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {

            Bitmap video = (Bitmap)eventArgs.Frame.Clone();
            ((MainWindow)Application.Current.MainWindow).pictureBox.Source = Convert(video);
        }
    }
}
