using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Kinect;

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
                Ksensor.ColorStream.Enable();
                Ksensor.DepthStream.Enable();
                Ksensor.SkeletonStream.Enable();
                SetKinectInfo();
            }

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
            stopSensor();
        }

        private void StartSensor(object sender, RoutedEventArgs e)
        {
            stopButton.IsEnabled = true;
            startButton.IsEnabled = false;
            startSensor();
        }
    }
}
