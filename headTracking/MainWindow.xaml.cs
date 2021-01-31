using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using DlibDotNet;
using OpenCvSharp;
using DlibDotNet.Extensions;
using System.Runtime.InteropServices;
using System.Windows.Shapes;
using WeCantSpell.Hunspell;
using System.Diagnostics;
using System.Windows.Threading;
using headTracking.SwipeType;

namespace headTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public static double h = 700;
        public static double w = 1600;
        public double fX = w / 2;
        public double fY = h / 2;
        public double thirdX = w / 2;
        public double thirdY = h / 2;
        VideoCaptureDevice LocalWebCam;
        public FilterInfoCollection LocalWebCamsCollection;
        private FacialLandmarks frontalLandmarksDetector = new FacialLandmarks();
        private InputAnalyze inputAnalyzer;
        public Dictionary<long, double[]> moveHistory = new Dictionary<long, double[]>();
        Stopwatch stopwatch = new Stopwatch();
        Stopwatch blinkStopwatch = new Stopwatch();
        List<double> blinkingRatio = new List<double>();
        bool isChoosingWord = false;
        bool isWriting = false;
        public int framesSkipped = 0;
        double[] headRotationAverage1 = new double[3];
        double[] headRotationAverage2 = new double[3];
        double[] headRotationAverage3 = new double[3];

        public MainWindow()
        {
            InitializeComponent();

            blinkStopwatch.Start();
            Loaded += MainWindow_Loaded;

            Dictionary<char, Button> keyboardGrid = new Dictionary<char, Button>();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (System.Windows.Controls.Button button in allLetters.Children)
                {
                    keyboardGrid.Add(button.Name[6], new Button
                    {
                        X1 = button.Margin.Left,
                        X2 = button.Margin.Left + button.Width,
                        Y1 = button.Margin.Top,
                        Y2 = button.Margin.Top + button.Height
                    });
                }

            }), DispatcherPriority.ContextIdle, null);

            inputAnalyzer = new InputAnalyze(keyboardGrid, WordList.CreateFromFiles(@"./English (British).dic"));
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            LocalWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            LocalWebCam = new VideoCaptureDevice(LocalWebCamsCollection[0].MonikerString);
            LocalWebCam.VideoResolution = LocalWebCam.VideoCapabilities[1];
            LocalWebCam.NewFrame += new NewFrameEventHandler(Cam_NewFrame);
            LocalWebCam.Start();
        }
        async void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {

            try
            {
                var img = (Bitmap)eventArgs.Frame.Clone();
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, img.Width, img.Height);
                System.Drawing.Imaging.BitmapData data = img.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, img.PixelFormat);
                var array = new byte[data.Stride * data.Height];
                Marshal.Copy(data.Scan0, array, 0, array.Length);

                double[] headRotation = new double[3];

                using (var ArrayImg = Dlib.LoadImageData<RgbPixel>(array, (uint)img.Height, (uint)img.Width, (uint)data.Stride))
                {
                    headRotation = frontalLandmarksDetector.detectFaceLandmarks(ArrayImg);
                }

               /* if (framesSkipped!=2)
                {
                    headRotationAverage1[framesSkipped] =headRotation[0];
                    headRotationAverage2[framesSkipped] =headRotation[1];
                    framesSkipped++;
                    return;
                }

                headRotation[0] = (headRotationAverage1[0] + headRotationAverage1[1]) / 2;
                headRotation[1] = (headRotationAverage2[0] + headRotationAverage2[1]) / 2;
                headRotation[2] = (headRotationAverage3[0] + headRotationAverage3[1]) / 2;
                framesSkipped = 0;*/
                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                bi.Freeze();
                await Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    pictureBox.Source = bi;

                    if (headRotation[0] != 0 || headRotation[1] != 0 && false)
                    {
                        bool blinked = false;
                        blinkingRatio.Add(headRotation[2]);
                        if (blinkingRatio.Count == 8)
                        {
                            if ((blinkingRatio[0] + blinkingRatio[1]) / 2 < 0.50 && blinkStopwatch.ElapsedMilliseconds > 3000)
                            {
                                Blinkratio.Text = "Blinked";
                                blinked = true;
                                blinkStopwatch.Restart();
                            }
                            else
                            {
                                Blinkratio.Text = (blinkingRatio[0]).ToString();
                            }
                            blinkingRatio.RemoveAt(0);
                        }
                        double[] lineVal = drawLine(headRotation[0], headRotation[1]);
                        var kursor = new Line
                        {
                            X1 = keyboard.Margin.Left + lineVal[0],
                            Y1 = keyboard.Margin.Top + lineVal[1],
                            X2 = keyboard.Margin.Left + lineVal[2],
                            Y2 = keyboard.Margin.Top + lineVal[3],
                            Stroke = System.Windows.Media.Brushes.Black,
                            StrokeThickness = 3,
                            
                        };

                        keyboard.Children.Add(kursor);
                        if (keyboard.Children.Count > 5)
                        {
                            keyboard.Children.RemoveAt(0);
                        }


                        if (isWriting == true && inputAnalyzer.settleButton(kursor.X2 + keyboard.Margin.Left, kursor.Y2 + keyboard.Margin.Top) != '`')
                        {

                            if (!stopwatch.IsRunning)
                            {
                                stopwatch.Start();
                            }

                            moveHistory.Add(stopwatch.ElapsedMilliseconds, new double[] {
                                kursor.X2 + keyboard.Margin.Left,
                                kursor.Y2 + keyboard.Margin.Top
                            });
                        }

                        if (blinked == true && isChoosingWord == false)
                        {
                            blinked = false;
                            if (isWriting == false)
                            {
                                isWriting = true;
                                Suggestions.Text = "Writing now!";
                            }
                            if (moveHistory.Count > 60 && isWriting == true)
                            {
                                isWriting = false;
                                IEnumerable<string> suggestions = inputAnalyzer.analyzeInputNeargye(moveHistory);

                                if (suggestions == null || suggestions.Count() == 0)
                                {
                                    Suggestions.Text = "Not Found any!";
                                }
                                else
                                {
                                    suggestionOption1.Text = suggestions.ElementAtOrDefault(0);
                                    suggestionOption2.Text = suggestions.ElementAtOrDefault(1);
                                    suggestionOption3.Text = suggestions.ElementAtOrDefault(2);
                                    suggestionOption1.Visibility = Visibility.Visible;
                                    suggestionOption2.Visibility = Visibility.Visible;
                                    suggestionOption3.Visibility = Visibility.Visible;
                                    isChoosingWord = true;

                                }
                            }
                        }
                        if (isChoosingWord == true && blinked == true)
                        {
                            if (suggestionOption1.Margin.Left < kursor.X2 + keyboard.Margin.Left && suggestionOption1.Margin.Left + suggestionOption1.Width > kursor.X2 + keyboard.Margin.Left &&
                            suggestionOption1.Margin.Top < kursor.Y2 + keyboard.Margin.Top && suggestionOption1.Margin.Top + suggestionOption1.Height > kursor.Y2 + keyboard.Margin.Top)
                            {
                                Letters.Text += suggestionOption1.Text + " ";
                            }
                            if (suggestionOption2.Margin.Left < kursor.X2 + keyboard.Margin.Left && suggestionOption2.Margin.Left + suggestionOption2.Width > kursor.X2 + keyboard.Margin.Left &&
                            suggestionOption2.Margin.Top < kursor.Y2 + keyboard.Margin.Top && suggestionOption2.Margin.Top + suggestionOption2.Height > kursor.Y2 + keyboard.Margin.Top)
                            {
                                Letters.Text += suggestionOption2.Text + " ";
                            }
                            if (suggestionOption3.Margin.Left < kursor.X2 + keyboard.Margin.Left && suggestionOption3.Margin.Left + suggestionOption3.Width > kursor.X2 + keyboard.Margin.Left &&
                            suggestionOption3.Margin.Top < kursor.Y2 + keyboard.Margin.Top && suggestionOption3.Margin.Top + suggestionOption3.Height > kursor.Y2 + keyboard.Margin.Top)
                            {
                                Letters.Text += suggestionOption3.Text + " ";
                            }
                            isChoosingWord = false;
                            isWriting = false;
                            Suggestions.Text = "Waiting for blink";
                            suggestionOption1.Visibility = Visibility.Hidden;
                            suggestionOption2.Visibility = Visibility.Hidden;
                            suggestionOption3.Visibility = Visibility.Hidden;

                        }
                    }
                })).Task;

            }
            catch (Exception ex)
            {
            }
        }
        public double[] drawLine(double sX, double sY)
        {
            sX = -sX * w / 400 + w / 2 + 400;
            sY = sY * h / 240 - h / 2;


            double[] lineValues = new double[4];
            lineValues[0] = fX;
            lineValues[1] = fY;

            if (fX - (sX + fX) / 2 < 0)
                lineValues[2] = fX + Math.Sqrt(Math.Abs(fX - (sX + fX) / 2) * 2);
            else
                lineValues[2] = fX - Math.Sqrt(Math.Abs(fX - (sX + fX) / 2) * 2);


            if (fY - (sY + fY) / 2 < 0)
                lineValues[3] = fY + Math.Sqrt(Math.Abs(fY - (sY + fY) / 2) * 2);
            else
                lineValues[3] = fY - Math.Sqrt(Math.Abs(fY - (sY + fY) / 2) * 2);

            if (ShowPoint.Children.Count > 0)
                ShowPoint.Children.RemoveAt(0);

            ShowPoint.Children.Add(new System.Windows.Shapes.Rectangle
            {
                Width = 10,
                Height = 10,
                Fill = System.Windows.Media.Brushes.Red,
                Margin = new Thickness((sX + fX + thirdX) / 3 + keyboard.Margin.Left + 30, (sY + fY + thirdY) / 3 + keyboard.Margin.Top + 20, 0, 0)
            });


            thirdX = fX;
            thirdY = fY;
            fX = lineValues[2];
            fY = lineValues[3];

            return lineValues;

        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            LocalWebCam.SignalToStop();
            LocalWebCam.WaitForStop();
            //Environment.Exit(Environment.ExitCode);
            //e.Cancel = true;
        }
    }
}
