using Microsoft.AspNetCore.Mvc;
using HariChatGpt.Services;

namespace HariChatGpt.Controllers;

public class GptController : ControllerBase
{
    private readonly GptService _gptService;

    public GptController(GptService gptService)
    {
        _gptService = gptService;
    }
    
    
    [HttpGet]
    [Route("gpt/request")]
    public async Task<IResult> GetGptRequest(string message)
    {
        try
        {          
            var res = await _gptService.GetHariAnswer(message);

            Console.WriteLine(res);

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

    [HttpGet]
    [Route("newMessage")]
    public async Task<IResult> GetMessageFromHari()
    {
        try
        {
            var res = await _gptService.SendMessage();

            Console.WriteLine(res);

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
}
