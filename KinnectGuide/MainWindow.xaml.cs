using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;

namespace KinnectGuide
{
    public partial class MainWindow : Window
    {
        private KinectSensor Ksensor;
        private MainWindowViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            viewModel = new MainWindowViewModel();
            DataContext = viewModel;
        }

        protected void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                Ksensor = KinectSensor.KinectSensors[0];
                KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
                stopButton.IsEnabled = false;
                angleBox.IsEnabled = false;
            }
            else {
                MessageBox.Show("No hay kinect conectado");
                this.Close();
            }
        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Disconnected:
                    MessageBox.Show("No hay kinect conectado");
                    this.Close();
                    break;
            }
        }

        public void SetKinectInfo() {
            if (Ksensor != null)
            {
                viewModel.ConnectionID = Ksensor.DeviceConnectionId;
            }
        }

        private void startSensor()
        {
            if (Ksensor != null && !Ksensor.IsRunning)
            {
                Ksensor.Start();
                Ksensor.ColorStream.Enable(ColorImageFormat.InfraredResolution640x480Fps30);
                //Ksensor.ColorFrameReady += Ksensor_ColorFrameReady;
                Ksensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(sensor_DepthFrameReady);
                SetKinectInfo();
            }

        }

        private void sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using ( DepthImageFrame mDepthImageFrame = e.OpenDepthImageFrame() )
            {
                if (mDepthImageFrame == null)
                    return;
                imageWindow.Source = getDepthImageFrame(mDepthImageFrame);
            }
        }

        private ImageSource getDepthImageFrame(DepthImageFrame mDepthImageFrame)
        {
            WriteableBitmap mWriteableBitmap;
            short[] pixelData = new short[mDepthImageFrame.PixelDataLength];
            int stride = mDepthImageFrame.Width * 2;
            mDepthImageFrame.CopyPixelDataTo(pixelData);
            mWriteableBitmap = new WriteableBitmap(Ksensor.DepthStream.FrameWidth, Ksensor.DepthStream.FrameHeight, 96, 96, PixelFormats.Gray16, null);
            mWriteableBitmap.WritePixels(new Int32Rect(0, 0, mWriteableBitmap.PixelWidth, mWriteableBitmap.PixelHeight), pixelData, stride, 0);

            return mWriteableBitmap;
        }

        private void Ksensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame mColorImageFrame = e.OpenColorImageFrame())
            {
                if (mColorImageFrame == null)
                    return;
                imageWindow.Source = getColorImageFromKinect(mColorImageFrame);
            }
        }
        
        public WriteableBitmap getColorImageFromKinect(ColorImageFrame mColorImageFrame)
        {
            var pixelData = new byte[mColorImageFrame.PixelDataLength];
            int stride = mColorImageFrame.Width * mColorImageFrame.BytesPerPixel;
            WriteableBitmap mWriteableBitmap;
            mColorImageFrame.CopyPixelDataTo(pixelData);
            mWriteableBitmap = new WriteableBitmap(Ksensor.ColorStream.FrameWidth, Ksensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Gray16, null);
            mWriteableBitmap.WritePixels( new Int32Rect(0,0, mWriteableBitmap.PixelWidth, mWriteableBitmap.PixelHeight), pixelData,stride, 0 );
            return mWriteableBitmap;
        }

        private void stopSensor()
        {
            if (Ksensor != null && Ksensor.IsRunning)
                Ksensor.Stop();    
        }

        private void StopSensor(object sender, RoutedEventArgs e)
        {
            startButton.IsEnabled = true;
            stopButton.IsEnabled = false;
            angleBox.IsEnabled = false;
            stopSensor();
        }

        private void StartSensor(object sender, RoutedEventArgs e)
        {
            stopButton.IsEnabled = true;
            startButton.IsEnabled = false;
            angleBox.IsEnabled = true;
            startSensor();
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int angle = 0;
            bool parsed = Int32.TryParse(angleBox.Text, out angle);

            if (Ksensor != null && parsed)
                if (angle < Ksensor.MaxElevationAngle && angle > Ksensor.MinElevationAngle)
                {
                    try
                    {
                        Ksensor.ElevationAngle = angle;
                    }
                   catch (InvalidOperationException ex)
                    {
                        
                    }
                }
                    
            
        }
    }
}
