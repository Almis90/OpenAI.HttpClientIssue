using System.Diagnostics;
using System.Windows.Input;
using OpenAI.Interfaces;
using OpenAI.ObjectModels.RequestModels;

namespace OpenAI.HttpClientIssue.SampleApp
{
	public class MainViewModel : BaseViewModel
    {
        private readonly IOpenAIService _openAIService;
        private string _message = "Who was Alexander the Great?";
        private string _responseMessage = string.Empty;

        public MainViewModel(IOpenAIService openAIService)
        {
            _openAIService = openAIService;
            SendCommand = new Command(() => ExecuteSend());
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public string ResponseMessage
        {
            get => _responseMessage;
            set
            {
                _responseMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand SendCommand { get; }

        private void ExecuteSend()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var response = _openAIService.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
                    {
                        Messages = new List<ChatMessage>
                        {
                            new ChatMessage("user", Message)
                        },
                        Model = OpenAI.ObjectModels.Models.ChatGpt3_5Turbo,
                        Stream = true,
                        MaxTokens = 4000,
                    });

                    await foreach (var completion in response)
                    {
                        if (completion.Successful)
                        {
                            var partialMessage = completion.Choices.FirstOrDefault()?.Message.Content;

                            ResponseMessage += partialMessage;
                        }
                        else
                        {
                            Debug.WriteLine($"{completion.Error.Code}: {completion.Error.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            });
        }
    }
}

