using OpenAI.GPT3.Managers;
using OpenAI.GPT3;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;



var openAiService = new OpenAIService(new OpenAiOptions()
{
    ApiKey = "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"
});

Console.WriteLine("Введи инструкцию:");

string? inst = Console.ReadLine();

var completionResult = await openAiService.Completions.CreateCompletion(new CompletionCreateRequest()
{
    Prompt = inst,
    Model = Models.CodeDavinciV2,
    Temperature = 0.5F,
    MaxTokens = 455
});

Console.WriteLine(completionResult.Choices[0].Text);

Console.ReadKey();
    
