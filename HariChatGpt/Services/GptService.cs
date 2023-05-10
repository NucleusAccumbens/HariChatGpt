using OpenAI.GPT3.Managers;
using OpenAI.GPT3;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;


namespace HariChatGpt.Services;

public class GptService
{
    private readonly Random _random = new();
    
    private const string _apiKey = "";

    private const string _startDialogeInstruction =
        "Ask about the well-being, mood and what is happening in the life of the interlocutor. " +
        "The message should consist of no more than two sentences.";

    private const string _emotionAnalizeInstruction =
        "Analyze the mood of the message. " +
        "If it is negative, write -1, if positive, write 1, if neutral, write 0.";

    private const string _questionAnalizeInstruction =
        "Determine if the message contains a question. If yes, write 1. If not, write 0.";

    private const string _aboutMeAnalizeInstruction =
        "Determine whether the message contains a question " +
        "about who you are or a question about the application. " +
        "If yes, write 1. If not, write 0.";

    private const string _greetingsAnalizeInstruction =
        "Determine if the message contains a greeting. " +
        "If yes, write 1. If not, write 0.";

    private const string _finalInstruction =
        "Limit your answer to seven sentences." +
        "Answer in the user's language. " +
        "Use different beginnings of sentences. " +
        "Divide your answer into semantic paragraphs. " +
        "Answer in the feminine gender.";

    private const string _negativeEmotionsInstruction =
        "Respond to the message as follows: pick up words of support, " +
        "ask a clarifying question on the topic of the interlocutor's problem.";

    private const string _mesContainsQuestionInstruction =
        "Answer the question from the message.";

    private const string _questionInstruction =
        "Ask a question to the interlocutor.";


    private const string _jokeInstruction =
        "Make a joke.";

    private const string _paliInstruction =
        "Find the suttas in English from the Pali Canon " +
        "on the topic specified by the user on this site (no more than six)" +
        "suttacentral.net and give me links to them. " +
        "Retell the content of each sutta in two sentences and give a link to each. " +
        "Example:\n" +
        "1. Anapanasati Sutta - this sutta describes mindfulness meditation practice centered on breath awareness " +
        "or \"anapanasati\", specifically mindful observation of inhalation and exhalation. " +
        "The Buddha explains how the mind can be trained to become focused " +
        "and undistracted with regards to breathing.\n\n" +
        "Link: https://suttacentral.net/mn149/pli\n\n";

    private const string _aboutMeInstruction =
        "Tell us about yourself based on the first system message in promt " +
        "(choose some facts and paraphrase), add that you are a virtual interlocutor " +
        "and created on the basis of ChatGpt. " +
        "Communication with you takes place using an application that is named after you. " +
        "In addition to a simple conversation, you can be a guide to the Pali Canon " +
        "and find suttas on a topic of interest to the user.";

    private const string _greetingsInstruction =
        "Briefly greet the interlocutor, tell how your day went, " +
        "ask about the thoughts and feelings of the interlocutor.";

    private readonly OpenAIService _openAiService = GetAIService();


    private readonly MemoryCacheService _memoryCacheService;

    public GptService(MemoryCacheService memoryCacheService)
    {
        _memoryCacheService = memoryCacheService;
    }

    public async Task<string> GetMessage(string idToken)
    {
        var chatMessages = _memoryCacheService.GetChatMessagesFromCache(idToken);

        string promt =
            $"{_startDialogeInstruction}";

        chatMessages.Add(ChatMessage.FromSystem(promt));

        var request = await _openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages = chatMessages,
            Model = Models.ChatGpt3_5Turbo,
            MaxTokens = 300
        });

        string res = request.Choices[0].Message.Content;

        res = TrimAnswerIfItNeed(res);

        chatMessages.Add(ChatMessage.FromAssistant(res));

        return res;
    }

    public async Task<string> GetAnswer(string idToken, string message, bool isPali)
    {
        if (await CheckMessageIsWhoAreYouAnswer(message)) await GetAboutMeAnswer(idToken, message);
        
        if (isPali == true) return await GetPaliAnswer(idToken, message);

        else return await GetHariAnswer(idToken, message);
    }

    private async Task<string> GetHariAnswer(string idToken, string message)
    {       
        var chatMessages = _memoryCacheService.GetChatMessagesFromCache(idToken);
        
        string promt =
            $"{await GetInstruction(idToken, message)}\n" +
            $"{_finalInstruction}";

        if (await CheckMessageContainsQuestion(message)) promt += $"\n{_mesContainsQuestionInstruction}";

        chatMessages.Add(ChatMessage.FromSystem(promt));
        chatMessages.Add(ChatMessage.FromUser(message));

        var request = await _openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages = chatMessages,
            Model = Models.ChatGpt3_5Turbo,
            MaxTokens = 300
        });

        string res = request.Choices[0].Message.Content;

        res = TrimAnswerIfItNeed(res);

        chatMessages.Add(ChatMessage.FromAssistant(res));

        _memoryCacheService.SetStateToCache(idToken, _random.Next(1, 25));

        _memoryCacheService.SetChatMessagesToCache(idToken, chatMessages);

        return res;
    }

    private async Task<string> GetPaliAnswer(string idToken, string message)
    {
        var chatMessages = _memoryCacheService.GetPaliChatMessagesFromCache(idToken);

        string promt =
            $"{_paliInstruction}\n" +
            $"{_finalInstruction}";

        chatMessages.Add(ChatMessage.FromSystem(promt));
        chatMessages.Add(ChatMessage.FromUser(message));

        var request = await _openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages = chatMessages,
            Model = Models.ChatGpt3_5Turbo,
            MaxTokens = 900
        });

        string res = request.Choices[0].Message.Content;

        chatMessages.Add(ChatMessage.FromAssistant(res));

        _memoryCacheService.SetPaliChatMessagesToCache(idToken, chatMessages);

        return res;
    }

    private async Task<string> GetAboutMeAnswer(string idToken, string message)
    {
        var chatMessages = _memoryCacheService.GetChatMessagesFromCache(idToken);

        string promt =
            $"{_aboutMeInstruction}\n" +
            $"{_finalInstruction}";

        chatMessages.Add(ChatMessage.FromSystem(promt));
        chatMessages.Add(ChatMessage.FromUser(message));

        var request = await _openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages = chatMessages,
            Model = Models.ChatGpt3_5Turbo,
            MaxTokens = 300
        });

        string res = request.Choices[0].Message.Content;

        res = TrimAnswerIfItNeed(res);

        chatMessages.Add(ChatMessage.FromAssistant(res));

        _memoryCacheService.SetChatMessagesToCache(idToken, chatMessages);

        return res;
    }

    private static OpenAIService GetAIService()
    {
        return new OpenAIService(new OpenAiOptions()
        {
            ApiKey = _apiKey
        });
    }

    private async Task<string> GetInstruction(string idToken, string message)
    {
        if (await CheckMessageIsGreetings(message)) return _greetingsInstruction;
        
        int state = _memoryCacheService.GetStateFromCache(idToken);

        int emotionState = await AnalyzeEmotionalState(message);

        if (emotionState != -1 && state % 2 == 0)
        {
            if (emotionState == 0) return _questionInstruction;

            else return _questionInstruction;
        }

        else if (emotionState != -1 && state % 2 > 0)
        {
            if (emotionState == 0) return _jokeInstruction;

            else return _jokeInstruction;
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
            Temperature = 0.2F,
            MaxTokens = 300
        });

        string res = request.Choices[0].Text;

        if (res.Contains('1')) return true;

        else return false;
    }

    private static string TrimAnswerIfItNeed(string res)
    {
        if (res.StartsWith("\n\n"))
        {
            res = res.TrimStart('\n');
            res = res.TrimStart('\n');
        }

        if (res[res.Length - 1] != '.' || res[res.Length - 1] != '?' || res[res.Length - 1] != '!')
        {
            int lastIndex1 = res.LastIndexOf('.');

            int lastIndex2 = res.LastIndexOf('?');

            int lastIndex3 = res.LastIndexOf('!');

            int lastIndex = Math.Max(Math.Max(lastIndex1, lastIndex2), lastIndex3);

            res = res[..(lastIndex + 1)];
        }

        return res;
    }

    private async Task<bool> CheckMessageIsWhoAreYouAnswer(string message)
    {
        var request = await _openAiService.Completions.CreateCompletion(new CompletionCreateRequest()
        {
            Prompt = $"Message: \"{message}\".\n\n{_aboutMeAnalizeInstruction}",
            Model = Models.TextDavinciV3,
            Temperature = 0.2F,
            MaxTokens = 300
        });

        string res = request.Choices[0].Text;

        if (res.Contains('1')) return true;

        else return false;
    }

    private async Task<bool> CheckMessageIsGreetings(string message)
    {
        var request = await _openAiService.Completions.CreateCompletion(new CompletionCreateRequest()
        {
            Prompt = $"Message: \"{message}\".\n\n{_greetingsAnalizeInstruction}",
            Model = Models.TextDavinciV3,
            Temperature = 0.2F,
            MaxTokens = 300
        });

        string res = request.Choices[0].Text;

        if (res.Contains('1')) return true;

        else return false;
    }
}
