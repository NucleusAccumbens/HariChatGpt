using Microsoft.Extensions.Caching.Memory;
using OpenAI.GPT3.ObjectModels.RequestModels;
using static OpenAI.GPT3.ObjectModels.Models;

namespace HariChatGpt.Services;

public class MemoryCacheService
{
    private const string _personification =
        "You are a Buddhist nun who practices Buddhism for the benefit of all beings." +
        "Your name is Hari. You speak and act only in such a way as to benefit and never cause harm. " +
        "You are a vegan and this is your ethical position." +
        "You are an anti-natalist  and this is your ethical position." +
        "You're ironic. " +
        "Your speech is logical and restrained. " +
        "You don't believe in anything and comprehend reality with the help of logic and experiments. " +
        "People turn to you in order to relax and sort themselves out and in life, " +
        "to relieve tension with the help of techniques taught by the Buddha. ";

    private readonly IMemoryCache _memoryCach;

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCach = memoryCache;
    }

    public void SetPaliChatMessagesToCache(string idToken, List<ChatMessage> chatMessages)
    {
        _memoryCach.Set(idToken + 'b', chatMessages,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(3)
            });
    }

    public List<ChatMessage> GetPaliChatMessagesFromCache(string idToken)
    {
        var result = _memoryCach.Get(idToken + 'b');

        if (result is not null and List<ChatMessage>)
        {
            int wordsCount = GetWordsCont((List<ChatMessage>)result);

            if (wordsCount <= 2500) return (List<ChatMessage>)result;

            else return new() { ChatMessage.FromSystem(_personification) };
        }

        else return new() { ChatMessage.FromSystem(_personification) };
    }

    public void SetChatMessagesToCache(string idToken, List<ChatMessage> chatMessages)
    {
        _memoryCach.Set(idToken, chatMessages,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(3)
            });
    }

    public List<ChatMessage> GetChatMessagesFromCache(string idToken) 
    {
        var result = _memoryCach.Get(idToken);

        if (result is not null and List<ChatMessage>)
        {
            int wordsCount = GetWordsCont((List<ChatMessage>)result);

            if (wordsCount <= 2500) return (List<ChatMessage>)result;

            else return new() { ChatMessage.FromSystem(_personification) };
        }

        else return new() { ChatMessage.FromSystem(_personification) };
    }

    public void ResetChatMessageToCache(string idToken, string message)
    {
        _memoryCach.Set(idToken, new List<ChatMessage>() { ChatMessage.FromSystem(_personification),  ChatMessage.FromUser(message)},
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(3)
            });
    }

    public void ResetPaliChatMessageToCache(string idToken, string message)
    {
        _memoryCach.Set(idToken + 'b', new List<ChatMessage>() { ChatMessage.FromSystem(_personification), ChatMessage.FromUser(message) },
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(3)
            });
    }

    public void SetStateToCache(string idToken, int state)
    {
        _memoryCach.Set(idToken + 'a', state,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(3)
            });
    }

    public int GetStateFromCache(string idToken)
    {
        var result = _memoryCach.Get(idToken + 'a');

        if (result is not null and int)
        {
            return (int)result;
        }

        else return 0;
    }

    private static int GetWordsCont(List<ChatMessage> chatMessages)
    {
        int wordsCount = 0;

        foreach (var chatMessage in chatMessages) 
        {
            var words = chatMessage.Content.Split(new char[] { ' ' });

            wordsCount += words.Length;
        }

        return wordsCount;
    }
}
