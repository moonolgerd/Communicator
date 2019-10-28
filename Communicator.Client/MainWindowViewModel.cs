using Communicator.Shared;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Communicator.Client
{
    public sealed class MainWindowViewModel : BindableBase, IDisposable
    {
        private readonly NamedPipeClientStream namedPipeClientStream;
        private string text;
        private string output = string.Empty;

        public MainWindowViewModel()
        {
            namedPipeClientStream = new NamedPipeClientStream(".", "Communicator", PipeDirection.InOut, PipeOptions.Asynchronous);
            
            Task.Run(Connect);

            Send = new DelegateCommand(async () =>
            {
                var person = new Person
                {
                    FirstName = Text
                };

                var serialized = JsonConvert.SerializeObject(person);
                var messageBytes = Encoding.UTF8.GetBytes(serialized);
                await namedPipeClientStream.WriteAsync(messageBytes, 0, messageBytes.Length);

                Text = string.Empty;
            });
            
        }

        private async void Connect()
        {
            namedPipeClientStream.Connect();
            namedPipeClientStream.ReadMode = PipeTransmissionMode.Message;

            while (true)
            {
                
                var messageBuilder = new StringBuilder();
                var messageBuffer = new byte[5];
                do
                {
                    await namedPipeClientStream.ReadAsync(messageBuffer, 0, messageBuffer.Length);
                    var messageChunk = Encoding.UTF8.GetString(messageBuffer);
                    messageBuilder.Append(messageChunk);
                    messageBuffer = new byte[messageBuffer.Length];
                }
                while (!namedPipeClientStream.IsMessageComplete);

                var p = JsonConvert.DeserializeObject<Person>(messageBuilder.ToString());

                Output += p.FirstName + "\n";
            }
        }

        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }

        public string Output { get => output; set => SetProperty(ref output, value); }

        public ICommand Send { get; set; }

        public void Dispose()
        {
            namedPipeClientStream.Dispose();
        }
    }
}
