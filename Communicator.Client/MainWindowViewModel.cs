using Communicator.Shared;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using ProtoBuf;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Communicator.Client
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly NamedPipeClientStream namedPipeClientStream;
        private string status;
        private string text;
        public MainWindowViewModel()
        {
            namedPipeClientStream = new NamedPipeClientStream(".", "Communicator", PipeDirection.InOut);

            SendBack = new DelegateCommand(() =>
            {
                var person = new Person
                {
                    FirstName = Text
                };

                string serialised = JsonConvert.SerializeObject(person);

                if (!namedPipeClientStream.IsConnected)
                {
                    namedPipeClientStream.Connect();
                    namedPipeClientStream.ReadMode = PipeTransmissionMode.Message;

                    StringBuilder messageBuilder = new StringBuilder();
                    string messageChunk = string.Empty;
                    byte[] messageBuffer = new byte[5];
                    do
                    {
                        namedPipeClientStream.Read(messageBuffer, 0, messageBuffer.Length);
                        messageChunk = Encoding.UTF8.GetString(messageBuffer);
                        messageBuilder.Append(messageChunk);
                        messageBuffer = new byte[messageBuffer.Length];
                    }
                    while (!namedPipeClientStream.IsMessageComplete);

                    var p = JsonConvert.DeserializeObject<Person>(messageBuilder.ToString());

                    Text = p.FirstName;

                    //Task.Run(Connect);
                }
                byte[] messageBytes = Encoding.UTF8.GetBytes(serialised);
                namedPipeClientStream.Write(messageBytes, 0, messageBytes.Length);
            });
        }

        private void Connect()
        {

            while (true)
            {
                if (!namedPipeClientStream.IsConnected)
                    continue;

                namedPipeClientStream.ReadMode = PipeTransmissionMode.Message;

                StringBuilder messageBuilder = new StringBuilder();
                byte[] messageBuffer = new byte[5];
                do
                {
                    namedPipeClientStream.Read(messageBuffer, 0, messageBuffer.Length);
                    var messageChunk = Encoding.UTF8.GetString(messageBuffer);
                    messageBuilder.Append(messageChunk);
                    messageBuffer = new byte[messageBuffer.Length];
                }
                while (!namedPipeClientStream.IsMessageComplete);

                var p = JsonConvert.DeserializeObject<Person>(messageBuilder.ToString());

                Text = p.FirstName;
            }
        }

        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }

        public string Status
        {
            get => status;
            set => SetProperty(ref status, value);
        }

        public ICommand SendBack { get; set; }
    }
}
