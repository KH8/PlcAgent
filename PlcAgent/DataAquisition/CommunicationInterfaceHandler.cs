﻿using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using _PlcAgent.General;
using _PlcAgent.Log;
using _PlcAgent.MainRegistry;
using _PlcAgent.PLC;
using _PlcAgent.Visual.Gui;
using _PlcAgent.Visual.Gui.DataAquisition;
using _PlcAgent.Visual.TreeListView;

namespace _PlcAgent.DataAquisition
{
    public class CommunicationInterfaceHandler : Module
    {
        #region Variables

        private CommunicationInterfaceComposite _readInterfaceComposite;
        private CommunicationInterfaceComposite _writeInterfaceComposite;

        private readonly DisplayDataBuilder.DisplayDataContainer _readInterfaceCollection = new DisplayDataBuilder.DisplayDataContainer();
        private readonly DisplayDataBuilder.DisplayDataContainer _writeInterfaceCollection = new DisplayDataBuilder.DisplayDataContainer();

        private Thread _communicationThread;

        #endregion


        #region Properties

        public CommunicationInterfaceComposite ReadInterfaceComposite
        { get { return _readInterfaceComposite; } }
        public CommunicationInterfaceComposite WriteInterfaceComposite
        { get { return _writeInterfaceComposite; } }

        public PlcCommunicator PlcCommunicator { get; set; }
        public CommunicationInterfacePath PathFile { get; set; }

        public DisplayDataBuilder.DisplayDataContainer ReadInterfaceCollection { get { return _readInterfaceCollection; } }
        public DisplayDataBuilder.DisplayDataContainer WriteInterfaceCollection { get { return _writeInterfaceCollection; } }

        public delegate void InterfaceUpdatedDelegate();
        public InterfaceUpdatedDelegate OnInterfaceUpdatedDelegate;

        public override string Description
        {
            get { return Header.Name + " ; assigned components: " + PlcCommunicator.Header.Name; }
        }

        #endregion


        #region Constructors

        public CommunicationInterfaceHandler(uint id, string name, PlcCommunicator plcCommunicator,
            CommunicationInterfacePath pathFile)
            : base(id, name)
        {
            ReferencePosition = 2;
            PlcCommunicator = plcCommunicator;
            PathFile = pathFile;

            _communicationThread = new Thread(CommunicationHandler);
            _communicationThread.SetApartmentState(ApartmentState.STA);
            _communicationThread.IsBackground = true;

            Logger.Log("ID: " + Header.Id + " Communication interface component created");
        }

        #endregion


        #region Methods

        public override void Initialize()
        {
            if (_communicationThread.IsAlive)
            {
                _communicationThread.Abort();
                Thread.Sleep(500);

                _communicationThread = new Thread(CommunicationHandler);
                _communicationThread.SetApartmentState(ApartmentState.STA);
                _communicationThread.IsBackground = true;
            }

            _readInterfaceComposite = new CommunicationInterfaceHierarchicalBuilder().InitializeInterface(Header.Id,
                CommunicationInterfaceComponent.InterfaceType.ReadInterface, PathFile, this);
            _writeInterfaceComposite = new CommunicationInterfaceHierarchicalBuilder().InitializeInterface(Header.Id,
                CommunicationInterfaceComponent.InterfaceType.WriteInterface, PathFile, this);
            new DisplayDataHierarchicalBuilder().Build(_readInterfaceCollection, _writeInterfaceCollection, this);

            if (OnInterfaceUpdatedDelegate != null) OnInterfaceUpdatedDelegate();

            _communicationThread.Start();

            Logger.Log("ID: " + Header.Id + " Communication interface Initialized");
        }

        public override void Deinitialize()
        {
            OnInterfaceUpdatedDelegate = null;
            _communicationThread.Abort();

            Logger.Log("ID: " + Header.Id + " Communication interface Deinitialized");
        }

        public override void TemplateGuiUpdate(TabControl mainTabControl, TabControl outputTabControl,
            TabControl connectionTabControl, Grid footerGrid)
        {
            var newtabItem = new TabItem { Header = Header.Name };
            connectionTabControl.Items.Add(newtabItem);
            connectionTabControl.SelectedItem = newtabItem;

            var newScrollViewer = new ScrollViewer();
            newtabItem.Content = newScrollViewer;

            var newGrid = new Grid();
            newScrollViewer.Content = newGrid;

            var gridGuiCommunicationInterfaceConfiguration = (GuiComponent)RegistryContext.Registry.GuiComInterfacemunicationConfigurations.ReturnComponent(Header.Id);
            gridGuiCommunicationInterfaceConfiguration.Initialize(0, 0, newGrid);

            newtabItem = new TabItem { Header = Header.Name + " Online" };
            mainTabControl.Items.Add(newtabItem);
            mainTabControl.SelectedItem = newtabItem;

            newGrid = new Grid();
            newtabItem.Content = newGrid;

            newGrid.Height = Limiter.DoubleLimit(mainTabControl.Height - 32, 0);
            newGrid.Width = Limiter.DoubleLimit(mainTabControl.Width - 10, 0);

            var gridGuiCommunicationInterfaceOnline = (GuiComponent)RegistryContext.Registry.GuiCommunicationInterfaceOnlines.ReturnComponent(Header.Id);
            gridGuiCommunicationInterfaceOnline.Initialize(0, 0, newGrid);
            var guiComponent = (GuiCommunicationInterfaceOnlineHierarchical)gridGuiCommunicationInterfaceOnline.UserControl;
            guiComponent.UpdateSizes(newGrid.Height, newGrid.Width);
            guiComponent.TabItem = newtabItem;
        }

        public override void TemplateRegistryComponentUpdateRegistryFile()
        {
            MainRegistryFile.Default.CommunicationInterfaceHandlers[Header.Id] = new uint[9];
            MainRegistryFile.Default.CommunicationInterfaceHandlers[Header.Id][0] = Header.Id;
            MainRegistryFile.Default.CommunicationInterfaceHandlers[Header.Id][1] = PlcCommunicator.Header.Id;
            MainRegistryFile.Default.CommunicationInterfaceHandlers[Header.Id][2] = 0;
            MainRegistryFile.Default.CommunicationInterfaceHandlers[Header.Id][3] = 0;
            MainRegistryFile.Default.CommunicationInterfaceHandlers[Header.Id][4] = 0;
        }

        public override void TemplateRegistryComponentCheckAssignment(RegistryComponent component)
        {
            if (MainRegistryFile.Default.CommunicationInterfaceHandlers[Header.Id][component.ReferencePosition] == component.Header.Id) throw new Exception("The component is still assigned to another one");
        }

        public void InitializeInterface()
        {
            if (PathFile.ConfigurationStatus[Header.Id] == 1)
            {
                try
                {
                    Initialize();
                }
                catch (Exception)
                {
                    PathFile.ConfigurationStatus[Header.Id] = 0;
                    PathFile.Save();
                    MessageBox.Show("ID: " + Header.Id + " Interface initialization failed\nRestart application",
                        "Initialization Failed");
                    Logger.Log("ID: " + Header.Id + " Communication interface initialization failed");
                    Environment.Exit(0);
                }
            }
            else
            {
                PathFile.Path[Header.Id] = "DataAquisition\\DB1000_NEW.csv";
                PathFile.ConfigurationStatus[Header.Id] = 1;
                PathFile.Save();

                try
                {
                    Initialize();
                }
                catch (Exception)
                {
                    PathFile.ConfigurationStatus[Header.Id] = 0;
                    PathFile.Save();
                    MessageBox.Show("ID: " + Header.Id + " Interface initialization failed\nRestart application",
                        "Initialization Failed");
                    Logger.Log("ID: " + Header.Id + " Communication interface initialization failed");
                    Environment.Exit(0);
                }
                Logger.Log("ID: " + Header.Id + " Communication interface initialized with file: " +
                           PathFile.Path[Header.Id]);
            }
        }

        private void UpdateObservableCollections()
        {
            _readInterfaceCollection.Update();
            _writeInterfaceCollection.Update();
        }

        #endregion


        #region Thread Methods

        private void CommunicationHandler()
        {
            while (_communicationThread.IsAlive)
            { 
                MaintainConnection();
                UpdateObservableCollections();
                Thread.Sleep(10);
            }
        }

        private void MaintainConnection()
        {
            if (PlcCommunicator.ConnectionStatus != 1) return;
            try
            {
                if (_readInterfaceComposite != null) _readInterfaceComposite.ReadValue(PlcCommunicator.ReadBytes);
                if (_writeInterfaceComposite != null) _writeInterfaceComposite.WriteValue(PlcCommunicator.WriteBytes);
            }
            catch (Exception)
            {
                PlcCommunicator.CloseConnection();
                MessageBox.Show("ID: " + Header.Id + " Interface does not meet a connection set up",
                        "Interface Fault");
                Logger.Log("ID: " + Header.Id + " Interface does not meet a connection set up");
            }
        }

        #endregion


        #region Auxiliaries

        public class InitializerException : ApplicationException
        { public InitializerException(string info) : base(info) { }}

        #endregion
    }
}
