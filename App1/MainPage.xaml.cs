using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using DataAccessLibrary;
using Sensors.Dht;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int LED_PIN = 22;
        private const int DHT_PIN = 11;

        private GpioPin gpioPinLED;

        private GpioPin gpioPinDHT;

        private Dht22 dht22 = null;
        private DhtReading dDhtReading = new DhtReading();
        private DispatcherTimer timer;

        private GpioController gpioController;

        public MainPage()
        {
            this.InitializeComponent();

            Unloaded += MainPage_Unloaded;

            InitGPIO();
        }

        private void InitGPIO()

        {
            gpioController = GpioController.GetDefault();

            if (gpioController == null)
            {
                GpioStatus.Text = "There is no GPIO controller on this device.";

                return;
            }

            GpioStatus.Text = "GPIO Great";

            gpioPinLED = gpioController.OpenPin(LED_PIN);
            gpioPinLED.SetDriveMode(GpioPinDriveMode.Output);
            gpioPinLED.Write(GpioPinValue.Low);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1500);
            timer.Tick += Timer_Tick;

            gpioPinDHT = gpioController.OpenPin(DHT_PIN, GpioSharingMode.Exclusive);
            if (gpioPinDHT != null)
            {
                //DhtReading dhtReading = await gpioPinDHT.GetReadingAsync()
                dht22 = new Dht22(gpioPinDHT, GpioPinDriveMode.Input);
                timer.Start();
            }
        }

        private async void Timer_Tick(object sender, object e)
        {
            timer.Stop();
            try
            {
                dDhtReading = await dht22.GetReadingAsync().AsTask().ConfigureAwait(true);
                if (dDhtReading.IsValid)
                {
                    var temperature = Convert.ToSingle(dDhtReading.Temperature);
                    var humidity = Convert.ToSingle(dDhtReading.Humidity);
                    lblTemperature.Text = string.Format("{0:0.0} °C", temperature);
                    lblHumidity.Text = string.Format("{0:0.0} %", humidity);
                    //_SuccessCount++;
                }
                else
                {
                    lblLog.Text = "reading fail";
                    //_FailCount++;
                }
            }
            catch (Exception ex)
            {
                lblLog.Text = ex.ToString();
                //_FailCount++;
            }
            finally
            {
                timer.Start();
            }
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (gpioController != null)
            {
                gpioPinLED.Write(GpioPinValue.Low);
                gpioPinLED.Dispose();
            }
        }

        private void High_Click(object sender, RoutedEventArgs e)
        {
            if (gpioController != null)
            {
                gpioPinLED.Write(GpioPinValue.High);
            }
        }

        private void Low_Click(object sender, RoutedEventArgs e)
        {
            if (gpioController != null)
            {
                gpioPinLED.Write(GpioPinValue.Low);
            }
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            DataAccess.AddData(Input_Box.Text);

            Output.ItemsSource = DataAccess.GetData();
        }
    }
}