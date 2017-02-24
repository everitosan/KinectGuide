using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Kinect;
using System.Linq;
using TobiasErichsen.teVirtualMIDI;

namespace KinnectGuide
{
    public partial class MainWindow : Window
    {
        private KinectSensor Ksensor;
        private MainWindowViewModel viewModel;
        public static TeVirtualMIDI vPort;

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
                TestMidi.IsEnabled = false;
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
                Ksensor.SkeletonStream.Enable();
                Ksensor.SkeletonFrameReady += Ksensor_SkeletonFrameReady;
                SetKinectInfo();
                vPort = new TeVirtualMIDI("KinectMIDI");
            }

        }

        private void Ksensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] totalSkeletons = new Skeleton[6];
            using (SkeletonFrame mSkeletonFrame = e.OpenSkeletonFrame())
            {
                if (mSkeletonFrame == null)
                    return;
                mSkeletonFrame.CopySkeletonDataTo(totalSkeletons);
                Skeleton firstSkeleton = (from trackedSkeleton in totalSkeletons where trackedSkeleton.TrackingState == SkeletonTrackingState.Tracked select trackedSkeleton).FirstOrDefault();

                if (firstSkeleton == null)
                    return;
                if (firstSkeleton.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
                {
                    this.MapJointsWithUIElement(firstSkeleton);
                }
            }
        }

        private void MapJointsWithUIElement( Skeleton sk )
        {
            Point mappedPoint = this.ScalePosition(sk.Joints[JointType.HandRight].Position);
            Canvas.SetLeft(righthand, mappedPoint.X);
            Canvas.SetTop(righthand, mappedPoint.Y);
        }

        private Point ScalePosition(SkeletonPoint skPoint)
        {
            DepthImagePoint depthPoint = Ksensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skPoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
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
            TestMidi.IsEnabled = false;
            stopSensor();
            vPort.shutdown();
        }

        private void StartSensor(object sender, RoutedEventArgs e)
        {
            stopButton.IsEnabled = true;
            startButton.IsEnabled = false;
            angleBox.IsEnabled = true;
            startSensor();
            TestMidi.IsEnabled = true;
        }
        private void SendMidi(object sender, RoutedEventArgs e) {
            var midimessage = new byte[] { 144, 103, 100};
            vPort.sendCommand(midimessage);
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
