using Communicator.Shared;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Communicator.Server
{
    public sealed class MainWindowViewModel : BindableBase, IDisposable
    {
        private readonly NamedPipeServerStream namedPipeServerStream;
        private string text;
        private string output = string.Empty;

        public MainWindowViewModel()
        {
            namedPipeServerStream = new NamedPipeServerStream("Communicator", PipeDirection.InOut, 1,
                PipeTransmissionMode.Message, PipeOptions.Asynchronous);

            Task.Run(Connect);

            Send = new DelegateCommand(() =>
            {
                Task.Run(async () =>
                {
                    var person = new Person
                    {
                        FirstName = Text
                    };

                    var serialized = JsonConvert.SerializeObject(person);
                    var messageBytes = Encoding.UTF8.GetBytes(serialized);
                    await namedPipeServerStream.WriteAsync(messageBytes, 0, messageBytes.Length);

                    Text = string.Empty;
                });
            });
        }

        private async void Connect()
        {
            namedPipeServerStream.WaitForConnection();

            while (true)
            {
                StringBuilder messageBuilder = new StringBuilder();
                byte[] messageBuffer = new byte[5];
                do
                {
                    await namedPipeServerStream.ReadAsync(messageBuffer, 0, messageBuffer.Length);
                    var messageChunk = Encoding.UTF8.GetString(messageBuffer);
                    messageBuilder.Append(messageChunk);
                    messageBuffer = new byte[messageBuffer.Length];
                }
                while (!namedPipeServerStream.IsMessageComplete);

                var p = JsonConvert.DeserializeObject<Person>(messageBuilder.ToString());

                
                Output += p.FirstName + "\n";
            }
        }

        public ICommand Send { get; set; }

        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }
        
        public string Output { get => output; set => SetProperty(ref output, value); }

        public void Dispose()
        {
            namedPipeServerStream.Disconnect();
            namedPipeServerStream.Dispose();
        }
    }
}
