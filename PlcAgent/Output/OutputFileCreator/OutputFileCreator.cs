﻿using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using _PlcAgent.DataAquisition;
using _PlcAgent.General;
using _PlcAgent.Log;
using _PlcAgent.Output.Template;

namespace _PlcAgent.Output.OutputFileCreator
{
    class OutputFileCreator : OutputModule
    {
        #region Variables

        private readonly Thread _communicationThread;

        #endregion


        #region Properties

        public OutputWriter OutputWriter { get; set; }

        public OutputHandlerFile OutputHandlerFile { get; set; }
        public OutputHandlerInterfaceAssignmentFile OutputHandlerInterfaceAssignmentFile { get; set; }

        #endregion


        #region Constructors

        public OutputFileCreator(uint id, string name, CommunicationInterfaceHandler communicationInterfaceHandler)
            : base(id, name, communicationInterfaceHandler)
        {
            OutputWriter = new OutputXmlWriter();

            _communicationThread = new Thread(OutputCommunicationThread);
            _communicationThread.SetApartmentState(ApartmentState.STA);
            _communicationThread.IsBackground = true;
        }

        #endregion


        #region Methods

        public void CreateOutput(string fileName, OutputDataTemplateComposite outputDataTemplateComposite)
        {
            var settings = new XmlWriterSettings {Indent = true, IndentChars = "\t"};
            using (var writer = XmlWriter.Create(fileName, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Composite");

                WriteComponentToTheFile(writer, outputDataTemplateComposite);

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            Logger.Log(fileName + " XML output file created");
        }

        public void WriteComponentToTheFile(XmlWriter writer, OutputDataTemplateComposite outputDataTemplateComposite)
        {
            foreach (var component in outputDataTemplateComposite.Cast<OutputDataTemplateComponent>())
            {
                writer.WriteStartElement(component.Name);
                if (component.GetType() == typeof (OutputDataTemplateComposite))
                {
                    WriteComponentToTheFile(writer, component as OutputDataTemplateComposite);
                }
                else
                {
                    writer.WriteElementString("Position", component.Component.Pos.ToString(CultureInfo.InvariantCulture));
                    writer.WriteElementString("Name", component.Component.Name);
                    writer.WriteElementString("Type", component.Component.Type.ToString());
                    writer.WriteElementString("Value", CleanInvalidXmlChars(component.Component.StringValue()).Trim());
                }
                writer.WriteEndElement();
            }
        }

        internal static string CleanInvalidXmlChars(string text)
        {
            const string re = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(text, re, "");
        }

        #endregion


        #region Background methods

        private void OutputCommunicationThread()
        {
        }

        #endregion


        #region Auxiliaries

        public class OutputFileCreatorException : ApplicationException
        {
            public OutputFileCreatorException(string info) : base(info) { }
        }

        public override void Initialize()
        {
            _communicationThread.Start();
            Logger.Log("ID: " + Header.Id + " Output File Creator Initialized");
        }

        public override void Deinitialize()
        {
            _communicationThread.Abort();
            Logger.Log("ID: " + Header.Id + " Output File Creator Deinitialized");
        }

        public override void UpdateAssignment()
        {
            throw new NotImplementedException();
        }

        protected override bool CheckInterface()
        {
            throw new NotImplementedException();
        }

        protected override void CreateInterfaceAssignment(uint id, string[][] assignment)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
