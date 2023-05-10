using Microsoft.AspNetCore.Mvc;
using HariChatGpt.Services;
using Microsoft.AspNetCore.Cors;

namespace HariChatGpt.Controllers;

public class GptController : ControllerBase
{
    private readonly GptService _gptService;

    private readonly MemoryCacheService _memoryCacheService;

    public GptController(GptService gptService, MemoryCacheService memoryCacheService)
    {
        _gptService = gptService;
        _memoryCacheService = memoryCacheService;
    }

    [HttpGet]
    [Route("gpt/request")]
    public async Task<IResult> GetGptRequest(string idToken, string message, bool isPali)
    {
        try
        {
            LogMessage(idToken, message);
            
            var res = await _gptService.GetAnswer(idToken, message, isPali);

            LogMessage("hari", res);

            var result = Results.Json(
                res,
                new(System.Text.Json.JsonSerializerDefaults.Web),
                "application/json; charset=utf-8",
                200);

            return result;
        }
        catch (Exception ex)
        {
            _memoryCacheService.ResetChatMessageToCache(idToken, message);

            _memoryCacheService.ResetPaliChatMessageToCache(idToken, message);
            
            //string answer = "hari has gone into deep meditation and cannot answer, " +
            //    "but as soon as her intellect is rested and refreshed, " +
            //    "you will be able to communicate with her again";

            Console.ForegroundColor = ConsoleColor.Red; 
            LogMessage("hari", ex.Message);
            Console.ResetColor();

            //return Results.Json(answer,
            //    new(System.Text.Json.JsonSerializerDefaults.Web));

            return await GetGptRequest(idToken, message, isPali);
        }
    }

    [HttpGet]
    [Route("newMessage")]
    public async Task<IResult> GetMessageFromHari(string idToken)
    {
        try
        {
            var res = await _gptService.GetMessage(idToken);

            return Results.Json(
                res,
                new(System.Text.Json.JsonSerializerDefaults.Web),
                "application/json; charset=utf-8",
                200);
        }
        catch (Exception)
        {
            return Results.Json("Error", new(System.Text.Json.JsonSerializerDefaults.Web));
        }
    }

    private static void LogMessage(string token, string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n{token}: {message}\n");
        Console.ResetColor();
    }
}
