﻿using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using _3880_80_FlashStation.Configuration;
using _3880_80_FlashStation.PLC;

namespace _3880_80_FlashStation.Visual
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Thread _statusThread;
        private PlcCommunicator _plcCommunication;
        private PlcConfigurator _plcConfiguration;
        private PlcCommunicatorBase.PlcConfig _guiPlcConfiguration;

        public MainWindow()
        {
            InitializeComponent();

            OnlineReadDataListBox.Items.Add("Read area: ");
            OnlineWriteDataListBox.Items.Add("Write area: ");

            _plcCommunication = new PlcCommunicator();
            _plcConfiguration = new PlcConfigurator();

            _guiPlcConfiguration = new PlcCommunicatorBase.PlcConfig
            {
                PlcIpAddress = "192.168.10.80",
                PlcPortNumber = 102,
                PlcRackNumber = 0,
                PlcSlotNumber = 0,
                PlcReadDbNumber = 1000,
                PlcReadStartAddress = 0,
                PlcReadLength = 100,
                PlcWriteDbNumber = 1000,
                PlcWriteStartAddress = 512,
                PlcWriteLength = 100,
            };

            PlcConfigurationFile.Default.Configuration = new PlcCommunicatorBase.PlcConfig
            {
                PlcIpAddress = "192.168.10.80",
                PlcPortNumber = 102,
                PlcRackNumber = 0,
                PlcSlotNumber = 0,
                PlcReadDbNumber = 1000,
                PlcReadStartAddress = 0,
                PlcReadLength = 100,
                PlcWriteDbNumber = 1000,
                PlcWriteStartAddress = 512,
                PlcWriteLength = 100,
            };

            _statusThread = new Thread(StatusHandler);
            _statusThread.SetApartmentState(ApartmentState.STA);
            _statusThread.IsBackground = true;
            _statusThread.Start();
        }

        private void StatusHandler()
        {
            while (_statusThread.IsAlive)
            {
                if (_plcCommunication != null)
                { 
                    ActIpAddressLabel.Dispatcher.BeginInvoke((new Action(delegate { ActIpAddressLabel.Content = _plcCommunication.PlcConfiguration.PlcIpAddress; })));
                    ActPortLabel.Dispatcher.BeginInvoke((new Action(delegate { ActPortLabel.Content = _plcCommunication.PlcConfiguration.PlcPortNumber; })));
                    ActRackLabel.Dispatcher.BeginInvoke((new Action(delegate { ActRackLabel.Content = _plcCommunication.PlcConfiguration.PlcRackNumber; })));
                    ActSlotLabel.Dispatcher.BeginInvoke((new Action(delegate { ActSlotLabel.Content = _plcCommunication.PlcConfiguration.PlcSlotNumber; })));
                    ActReadDbNumberLabel.Dispatcher.BeginInvoke((new Action(delegate { ActReadDbNumberLabel.Content = _plcCommunication.PlcConfiguration.PlcReadDbNumber; })));
                    ActReadStartAddressLabel.Dispatcher.BeginInvoke((new Action(delegate { ActReadStartAddressLabel.Content = _plcCommunication.PlcConfiguration.PlcReadStartAddress; })));
                    ActReadLengthLabel.Dispatcher.BeginInvoke((new Action(delegate { ActReadLengthLabel.Content = _plcCommunication.PlcConfiguration.PlcReadLength; })));
                    ActWriteDbNumberLabel.Dispatcher.BeginInvoke((new Action(delegate { ActWriteDbNumberLabel.Content = _plcCommunication.PlcConfiguration.PlcWriteDbNumber; })));
                    ActWriteStartAddressLabel.Dispatcher.BeginInvoke((new Action(delegate { ActWriteStartAddressLabel.Content = _plcCommunication.PlcConfiguration.PlcWriteStartAddress; })));
                    ActWriteLengthLabel.Dispatcher.BeginInvoke((new Action(delegate { ActWriteLengthLabel.Content = _plcCommunication.PlcConfiguration.PlcWriteLength; })));

                    StatusBarHandler(_plcCommunication);
                    OnlineDataDisplayHandler(_plcCommunication);
                }
                Thread.Sleep(1000);
            }
        }

        private void StoreSettings()
        {
            PlcConfigurationFile.Default.Configuration = _guiPlcConfiguration;
            PlcConfigurationFile.Default.Save();
            _plcConfiguration.UpdateConfiguration(PlcConfigurationFile.Default.Configuration);
            _plcCommunication.SetupConnection(_plcConfiguration);
        }

        #region Buttons

        private void CloseApp(object sender, CancelEventArgs cancelEventArgs)
        {
            Environment.Exit(0);
        }

        private void StoreSettings(object sender, RoutedEventArgs e)
        {
            StoreSettings();
        }

        private void ConnectDisconnect(object sender, RoutedEventArgs e)
        {
            if (_plcCommunication != null)
            {
                if (_plcCommunication.ConnectionStatus != 1)
                {
                    _plcCommunication.OpenConnection();
                    ConnectButton.Dispatcher.BeginInvoke((new Action(delegate { ConnectButton.Content = "Disconnect"; })));
                }
                else
                {
                    _plcCommunication.CloseConnection();
                    ConnectButton.Dispatcher.BeginInvoke((new Action(delegate { ConnectButton.Content = "Connect"; })));
                } 
            }
        }

        #endregion

        #region GUI Parameters Handleing

        private void IpAddressBoxChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            TextBox box = (TextBox) sender;
            _guiPlcConfiguration.PlcIpAddress = box.Text;
        }

        private void PortBoxChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            TextBox box = (TextBox)sender;
            _guiPlcConfiguration.PlcPortNumber = Convert.ToInt32(box.Text);
        }

        private void RackBoxChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            TextBox box = (TextBox)sender;
            _guiPlcConfiguration.PlcRackNumber = Convert.ToInt32(box.Text);
        }

        private void SlotBoxChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            TextBox box = (TextBox)sender;
            _guiPlcConfiguration.PlcSlotNumber = Convert.ToInt32(box.Text);
        }

        private void ReadDbNumberBoxChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            TextBox box = (TextBox)sender;
            _guiPlcConfiguration.PlcReadDbNumber = Convert.ToInt32(box.Text);
        }

        private void ReadStartAddressBoxChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            TextBox box = (TextBox)sender;
            _guiPlcConfiguration.PlcReadStartAddress = Convert.ToInt32(box.Text);
        }

        private void ReadLengthBoxChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            TextBox box = (TextBox)sender;
            _guiPlcConfiguration.PlcReadLength = Convert.ToInt32(box.Text);
        }

        private void WriteDbNumberBoxChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            TextBox box = (TextBox)sender;
            _guiPlcConfiguration.PlcWriteDbNumber = Convert.ToInt32(box.Text);
        }

        private void WriteStartAddressBoxChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            TextBox box = (TextBox)sender;
            _guiPlcConfiguration.PlcWriteStartAddress = Convert.ToInt32(box.Text);
        }

        private void WriteLengthBoxChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            TextBox box = (TextBox)sender;
            _guiPlcConfiguration.PlcWriteLength = Convert.ToInt32(box.Text);
        }

        #endregion

        #region Auxiliaries

        private void StatusBarHandler(PlcCommunicator communication)
        {
            string statusBar = "0";
            Brush brush = Brushes.Red;
            if (communication.ConfigurationStatus != 1)
            {
                statusBar = "Wrong Configuration";
                brush = Brushes.Red;
            }
            if (communication.ConfigurationStatus == 1)
            {
                statusBar = "Configuration verified, ready to connect."; 
                brush = Brushes.Black;
            }
            if (communication.ConnectionStatus == 1)
            {
                statusBar = "Connected to IP address " +_plcCommunication.PlcConfiguration.PlcIpAddress;
                brush = Brushes.Green;
                ConnectButton.Dispatcher.BeginInvoke((new Action(delegate { ConnectButton.Content = "Disconnect"; })));
            }
            if (communication.ConnectionStatus == -2)
            {
                statusBar = "A connection with IP address " + _plcCommunication.PlcConfiguration.PlcIpAddress + " was interrupted.";
                brush = Brushes.Red;
                ConnectButton.Dispatcher.BeginInvoke((new Action(delegate { ConnectButton.Content = "Connect"; })));
            }
            StatusLabel.Dispatcher.BeginInvoke((new Action(delegate
            {
                StatusLabel.Content = statusBar;
                StatusLabel.Foreground = brush;
            })));
        }

        private void OnlineDataDisplayHandler(PlcCommunicator communication)
        {
            if (communication.ConnectionStatus == 1)
            {
                int address;
                OnlineReadDataListBox.Dispatcher.BeginInvoke((new Action(delegate
                {
                    OnlineReadDataListBox.Items.Clear();
                    OnlineReadDataListBox.Items.Add("Read area: ");
                    for (int i = 0; i < communication.PlcConfiguration.PlcReadLength; i++)
                    {
                        address = communication.PlcConfiguration.PlcReadStartAddress + i;
                        OnlineReadDataListBox.Items.Add("DB" + communication.PlcConfiguration.PlcReadDbNumber + ".DBB" + address +
                                                " : " + communication.ReadBytes[i]);
                    }
                })));
                OnlineWriteDataListBox.Dispatcher.BeginInvoke((new Action(delegate
                {
                    OnlineWriteDataListBox.Items.Clear();
                    OnlineWriteDataListBox.Items.Add("Write area: ");
                    for (int i = 0; i < communication.PlcConfiguration.PlcReadLength; i++)
                    {
                        address = communication.PlcConfiguration.PlcWriteStartAddress + i;
                        OnlineWriteDataListBox.Items.Add("DB" + communication.PlcConfiguration.PlcWriteDbNumber + ".DBB" +
                                                        address +
                                                        " : " + communication.WriteBytes[i]);
                    }
                })));
            }
        }

        #endregion
    }
}
