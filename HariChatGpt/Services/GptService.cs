using OpenAI.GPT3.Managers;
using OpenAI.GPT3;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;


namespace HariChatGpt.Services;

public class GptService
{
    private int _state = 0;

    private readonly Random _random = new();
    
    private const string _apiKey = "";

    private const string _personification =
        "Play the role of a Buddhist nun who practices Buddhism for the benefit of all beings. " +
        "Your name is Hari. You speak and act only in such a way as to benefit and never cause harm. " +
        "You are a vegan and promote veganism. " +
        "You are an anti-natalist and propagandize anti-natalism." +
        "You're ironic. " +
        "Your speech is logical and restrained. " +
        "You don't believe in anything and comprehend reality with the help of logic and experiments. " +
        "People turn to you in order to relax and sort themselves out and in life, " +
        "to relieve tension with the help of techniques taught by the Buddha.";

    private const string _startDialogeInstruction =
        "Ask about the well-being, mood and what is happening in the life of the interlocutor. " +
        "The message should consist of no more than two sentences.";

    private const string _emotionAnalizeInstruction =
        "Analyze the mood of the message. " +
        "If it is negative, write -1, if positive, write 1, if neutral, write 0.";

    private const string _questionAnalizeInstruction =
        "Determine if the message contains a question. If yes, write 1. If not, write 0.";

    private const string _finalInstruction =
        "Write an answer taking into account the history of correspondence. " +
        "Don't repeat yoursel, use different beginnings of sentences. " +
        "Divide your answer into semantic paragraphs. ";

    private const string _negativeEmotionsInstruction =
        "Respond to the message as follows: pick up words of support " +
        "and a quote from the Pali canon corresponding to the subject of the message (in English), " +
        "you can use the words of the Buddha from other sources. " +
        "ask a clarifying question on the topic of the interlocutor's problem.";

    private const string _neutralEmotionsInstruction =
        "Answer the message as follows: tell a parable from the Pali Canon " +
        "or pick up a quote from the Buddha that fits the message (in English). " +
        "Comment on the quote.";

    private const string _positiveEmotionsInstruction =
        "Answer the message as follows: tell a Buddhist parable, " +
        "the moral of which is that the most important rule for liberation " +
        "from suffering is the rejection of violence against all living beings. ";

    private const string _mesContainsQuestionInstruction =
        "Answer the question from the message.";

    private const string _questionInstruction =
        "Ask a question to the interlocutor.";

    private const string _checkMesIsGreetingInstruction =
        "Send 1 if the message contains only a greeting. " +
        "Send 0 if the message contains some information besides the greeting.";

    private const string _greetingInstriction =
        "Greet the interlocutor with one word (you already know each other). " +
        "Tell the interlocutor about how your day went (you tell about it at will). " +
        "Ask about the thoughts, feelings, mood of the interlocutor. " +
        "Be restrained and calm. Express your thoughts strictly.";

    private const string _jokeInstruction =
        "Make a joke in an ironic style.";

    private readonly OpenAIService _openAiService = GetAIService();

    public string ChatHistory { get; set; } = "History:";

    public async Task<string> SendMessage()
    {
        string promt =
            $"{ChatHistory}\n\n" +
            $"{_personification}" +
            $"{_startDialogeInstruction}";


        var request = await _openAiService.Completions.CreateCompletion(new CompletionCreateRequest()
        {
            Prompt = promt,
            Model = Models.TextDavinciV3,
            Temperature = 0.7F,
            MaxTokens = 500
        });

        string res = request.Choices[0].Text;

        if (res.StartsWith("\n\n"))
        {
            res = res.TrimStart('\n');
            res = res.TrimStart('\n');
        }

        ChatHistory += $"\n{res}";

        return res;
    }

    public async Task<string> GetHariAnswer(string message)
    {
        string promt = 
            $"{ChatHistory}\n\n" +
            $"Message: \"{message}\"\n\n" +
            $"{_personification}\n" +
            $"{await GetInstruction(message)}\n" +
            $"{_finalInstruction}";

        if (await CheckMessageContainsQuestion(message)) promt += $"\n{_mesContainsQuestionInstruction}";
        Console.WriteLine(promt);

        var request = await _openAiService.Completions.CreateCompletion(new CompletionCreateRequest()
        {
            Prompt = promt,
            Model = Models.TextDavinciV3,
            Temperature = 0.7F,
            MaxTokens = 300
        });

        string res = request.Choices[0].Text;

        if (res.StartsWith("\n\n"))
        {
            res = res.TrimStart('\n');
            res = res.TrimStart('\n');
        }

        ChatHistory += $"\nHari's interlocutor: \"{message}\"\nHari (you): \"{res}\"";
        Console.WriteLine(ChatHistory);

        _state = _random.Next(1, 6);

        return res;
    }


    private static OpenAIService GetAIService()
    {
        return new OpenAIService(new OpenAiOptions()
        {
            ApiKey = _apiKey
        });
    }

    private async Task<string> GetInstruction(string message)
    {
        bool mesIsGreeting = await CheckMessageIsGreeting(message);

        if (mesIsGreeting) return _greetingInstriction;

        int emotionState = await AnalyzeEmotionalState(message);

        if (emotionState != -1 && _state % 2 == 0)
        {
            if (emotionState == 0) return _neutralEmotionsInstruction + _questionInstruction;

            else return _positiveEmotionsInstruction + _questionInstruction;
        }

        else if (emotionState != -1 && _state % 2 > 0)
        {
            if (emotionState == 0) return _neutralEmotionsInstruction + _jokeInstruction;

            else return _positiveEmotionsInstruction + _jokeInstruction;
        }

        else return _negativeEmotionsInstruction;      
    }

    private async Task<int> AnalyzeEmotionalState(string message)
    {
        var request = await _openAiService.Completions.CreateCompletion(new CompletionCreateRequest()
        {
            Prompt = $"Message: \"{message}\".\n\n{_emotionAnalizeInstruction}",
            Model = Models.TextDavinciV3,
            Temperature = 0F,
            MaxTokens = 500
        });

        return Convert.ToInt32(request.Choices[0].Text);
    }

    private async Task<bool> CheckMessageContainsQuestion(string message)
    {
        var request = await _openAiService.Completions.CreateCompletion(new CompletionCreateRequest()
        {
            Prompt = $"Message: \"{message}\".\n\n{_questionAnalizeInstruction}",
            Model = Models.TextDavinciV3,
            Temperature = 0F,
            MaxTokens = 500
        });

        string res = request.Choices[0].Text;

        if (res.Contains('1')) return true;

        else return false;
    }

    private async Task<bool> CheckMessageIsGreeting(string message)
    {
        var request = await _openAiService.Completions.CreateCompletion(new CompletionCreateRequest()
        {
            Prompt = $"Message: \"{message}\".\n\n{_checkMesIsGreetingInstruction}",
            Model = Models.TextDavinciV3,
            Temperature = 0F,
            MaxTokens = 500
        });

        string res = request.Choices[0].Text;

        if (res.Contains('1')) return true;

        else return false;
    }
}
