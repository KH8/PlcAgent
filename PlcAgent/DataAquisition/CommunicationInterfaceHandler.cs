﻿using System;
using System.Collections.ObjectModel;
using System.Windows;
using _PlcAgent.General;
using _PlcAgent.Log;
using _PlcAgent.PLC;
using _PlcAgent.Visual;

namespace _PlcAgent.DataAquisition
{
    public class CommunicationInterfaceHandler : Module
    {
        private CommunicationInterfaceComposite _readInterfaceComposite;
        private CommunicationInterfaceComposite _writeInterfaceComposite;

        private readonly ObservableCollection<DisplayDataBuilder.DisplayData> _readInterfaceCollection = new ObservableCollection<DisplayDataBuilder.DisplayData>();
        private readonly ObservableCollection<DisplayDataBuilder.DisplayData> _writeInterfaceCollection = new ObservableCollection<DisplayDataBuilder.DisplayData>();

        public ObservableCollection<DisplayDataBuilder.DisplayData> ReadInterfaceCollection { get { return _readInterfaceCollection; } }
        public ObservableCollection<DisplayDataBuilder.DisplayData> WriteInterfaceCollection { get { return _writeInterfaceCollection; } }

        public CommunicationInterfaceHandler(uint id, string name, PlcCommunicator plcCommunicator, CommunicationInterfacePath pathFile) : base(id, name)
        {
            PlcCommunicator = plcCommunicator;
            PathFile = pathFile;
            Logger.Log("ID: " + Header.Id + " Communication interface component created");
        }

        public CommunicationInterfaceComposite ReadInterfaceComposite
        {
            get { return _readInterfaceComposite; }
        }

        public CommunicationInterfaceComposite WriteInterfaceComposite
        {
            get { return _writeInterfaceComposite; }
        }

        public CommunicationInterfacePath PathFile { get; set; }

        public PlcCommunicator PlcCommunicator { get; set; }

        public void InitializeInterface()
        {
            if (PathFile.ConfigurationStatus[Header.Id] == 1)
            {
                try { Initialize(); }
                catch (Exception)
                {
                    PathFile.ConfigurationStatus[Header.Id] = 0;
                    PathFile.Save();
                    MessageBox.Show("ID: " + Header.Id + " Interface initialization failed\nRestart application", "Initialization Failed");
                    Logger.Log("ID: " + Header.Id + " Communication interface initialization failed");
                    Environment.Exit(0);
                }
            }
            else
            {
                PathFile.Path[Header.Id] = "DataAquisition\\DB1000_NEW.csv";
                PathFile.ConfigurationStatus[Header.Id] = 1;
                PathFile.Save();

                try { Initialize(); }
                catch (Exception)
                {
                    PathFile.ConfigurationStatus[Header.Id] = 0;
                    PathFile.Save();
                    MessageBox.Show("ID: " + Header.Id + " Interface initialization failed\nRestart application", "Initialization Failed");
                    Logger.Log("ID: " + Header.Id + " Communication interface initialization failed");
                    Environment.Exit(0);
                }
                Logger.Log("ID: " + Header.Id + " Communication interface initialized with file: " + PathFile.Path[Header.Id]);
            }
        }

        public override void Initialize()
        {
            _readInterfaceComposite = CommunicationInterfaceBuilder.InitializeInterface(Header.Id, CommunicationInterfaceComponent.InterfaceType.ReadInterface, PathFile);
            _writeInterfaceComposite = CommunicationInterfaceBuilder.InitializeInterface(Header.Id, CommunicationInterfaceComponent.InterfaceType.WriteInterface, PathFile);
            DisplayDataBuilder.Build(_readInterfaceCollection, _writeInterfaceCollection, this);
        }

        public override void Deinitialize()
        {
            //
        }

        public void MaintainConnection()
        {
            if (PlcCommunicator.ConnectionStatus != 1) return;
            if (_readInterfaceComposite != null) _readInterfaceComposite.ReadValue(PlcCommunicator.ReadBytes);
            if (_writeInterfaceComposite != null) _writeInterfaceComposite.WriteValue(PlcCommunicator.WriteBytes);
        }

        public void UpdateObservableCollections()
        {
            foreach (var displayDataComponent in _readInterfaceCollection) { displayDataComponent.Update(); }
            foreach (var displayDataComponent in _writeInterfaceCollection) { displayDataComponent.Update(); }
        }

        #region Auxiliaries

        public class InitializerException : ApplicationException
        { public InitializerException(string info) : base(info) { }}

        #endregion
    }
}
