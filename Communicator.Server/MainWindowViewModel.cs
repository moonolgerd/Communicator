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

namespace Communicator.Server
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly NamedPipeServerStream namedPipeServerStream;
        private string status;
        private string text;
        
        public MainWindowViewModel()
        {
            namedPipeServerStream = new NamedPipeServerStream("Communicator", PipeDirection.InOut, 2,
                PipeTransmissionMode.Message);

            Status = "Waiting For Connection";

            Task.Run(Connect);
            
            Send = new DelegateCommand(() =>
            {
                Task.Run(() =>
                {
                    var person = new Person
                    {
                        FirstName = "Server is responding"
                    };
                    string serialised = JsonConvert.SerializeObject(person);
                    byte[] messageBytes = Encoding.UTF8.GetBytes(serialised);
                    namedPipeServerStream.Write(messageBytes, 0, messageBytes.Length);
                });
            });

        }

        private void Connect()
        {
            namedPipeServerStream.WaitForConnection();

            var p = new Person
            {
                FirstName = "Client has connected"
            };
            string serialised = JsonConvert.SerializeObject(p);
            byte[] messageBytes = Encoding.UTF8.GetBytes(serialised);
            namedPipeServerStream.Write(messageBytes, 0, messageBytes.Length);

            while (true)
            {
                StringBuilder messageBuilder = new StringBuilder();
                byte[] messageBuffer = new byte[5];
                do
                {
                    namedPipeServerStream.Read(messageBuffer, 0, messageBuffer.Length);
                    var messageChunk = Encoding.UTF8.GetString(messageBuffer);
                    messageBuilder.Append(messageChunk);
                    messageBuffer = new byte[messageBuffer.Length];
                }
                while (!namedPipeServerStream.IsMessageComplete);

                var person = JsonConvert.DeserializeObject<Person>(messageBuilder.ToString());

                Text = person.FirstName;
            }
        }

        public ICommand Send { get; set; }

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
    }
}
