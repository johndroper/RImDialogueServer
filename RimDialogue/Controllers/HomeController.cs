﻿using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using Azure.AI.OpenAI;
using GroqSharp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OpenAI.Chat;
using RimDialogueLocal.Core;
using System.ClientModel;
using static System.Net.Mime.MediaTypeNames;

namespace RimDialogueLocal.Controllers
{
  public class HomeController(IConfiguration Configuration, IMemoryCache memoryCache) : Controller
  {
    public void Log(params string[] data)
    {
      //******Logging******
      try
      {
        if (Configuration["LoggingEnabled"]?.ToLower() == "true")
        {
          if (!Directory.Exists("Output"))
            Directory.CreateDirectory("Output");
          using (StreamWriter log = System.IO.File.CreateText($"Output\\data-log-{DateTime.Now.Ticks}.txt"))
          {
            foreach (var item in data)
              log.WriteLine(item);
          }
        }
      }
      catch (Exception ex)
      {
        Exception exception = new("Error logging data.", ex);
        throw exception;
      }
    }

    public IActionResult Index()
    {
      return View();
    }

    private async Task<string> GetFromAws(string prompt)
    {
      var awsKey = Configuration["AwsKey"];
      if (String.IsNullOrWhiteSpace(awsKey))
        throw new Exception("AWS Key is empty in appsettings.");
      var awsSecret = Configuration["AwsSecret"];
      if (String.IsNullOrWhiteSpace(awsSecret))
        throw new Exception("AWS Secret is empty in appsettings.");
      var awsRegion = Configuration["AwsRegion"];
      if (String.IsNullOrWhiteSpace(awsRegion))
        throw new Exception("AWS Region is empty in appsettings.");
      var modelId = Configuration["AwsModelId"];
      if (String.IsNullOrWhiteSpace(modelId))
        throw new Exception("AWS ModelId is empty in appsettings.");
      try
      {
        BasicAWSCredentials awsCredentials = new(awsKey, awsSecret);
        var region = Amazon.RegionEndpoint.GetBySystemName(awsRegion);
        AmazonBedrockRuntimeClient client = new AmazonBedrockRuntimeClient(awsCredentials, region);
        var message = new Amazon.BedrockRuntime.Model.Message();
        message.Content = new List<ContentBlock> { new ContentBlock { Text = prompt } };
        message.Role = ConversationRole.User;
        ConverseRequest request = new ConverseRequest
        {
          ModelId = modelId,
          Messages = new List<Amazon.BedrockRuntime.Model.Message> { message }
        };
        var converseResponse = await client.ConverseAsync(request);
        return converseResponse.Output.Message.Content.First().Text;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Bedrock.", ex);
        exception.Data.Add("awsRegion", awsRegion);
        exception.Data.Add("modelId", modelId);
        throw exception;
      }
    }

    private async Task<string> GetFromOpenAi(string prompt)
    {
      var openAiUri = Configuration["OpenAiUri"];
      if (String.IsNullOrWhiteSpace(openAiUri))
        throw new Exception("Provider is set to OpenAi but 'OpenAiUri' is empty in appsettings.");
      var openAiApiKey = Configuration["OpenAiApiKey"];
      if (String.IsNullOrWhiteSpace(openAiApiKey))
        throw new Exception("Provider is set to OpenAi but 'OpenAiApiKey' is empty in appsettings.");
      var openAiDeployment = Configuration["OpenAiDeployment"];
      if (String.IsNullOrWhiteSpace(openAiDeployment))
        throw new Exception("Provider is set to OpenAi but 'OpenAiDeployment' is empty in appsettings.");
      try
      {
        AzureOpenAIClient azureClient = new(
          new Uri(openAiUri),
          new ApiKeyCredential(openAiApiKey));
        ChatClient chatClient = azureClient.GetChatClient(openAiDeployment);
        var results = await chatClient.CompleteChatAsync(new OpenAI.Chat.UserChatMessage(prompt));
        return results.Value.Content.First().Text;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Azure.", ex);
        exception.Data.Add("openAiUri", openAiUri);
        exception.Data.Add("openAiDeployment", openAiDeployment);
        throw exception;
      }
    }

    private async Task<string> GetFromGroq(string prompt)
    {
      var apiKey = Configuration["GroqApiKey"];
      if (String.IsNullOrWhiteSpace(apiKey))
        throw new Exception("Provider is set to Groq but 'GroqApiKey' is empty in appsettings.");
      var groqModelId = Configuration["GroqModelId"];
      if (String.IsNullOrWhiteSpace(groqModelId))
        throw new Exception("Provider is set to Groq but 'GroqModelId' is empty in appsettings.");
      try
      {
        var groqClient = new GroqClient(apiKey, groqModelId);
        var text = await groqClient.CreateChatCompletionAsync(
          new GroqSharp.Models.Message { Role = MessageRoleType.User, Content = prompt });
        if (text == null)
          throw new Exception("Groq response is null.");
        return text;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Groq.", ex);
        exception.Data.Add("groqModelId", groqModelId);
        throw exception;
      }
    }

    private async Task<string> GetFromOllama(string prompt)
    {
      var ollamaUrl = Configuration["OllamaUrl"];
      if (String.IsNullOrWhiteSpace(ollamaUrl))
        throw new Exception("Provider is set to Ollama but 'OllamaUrl' is empty in appsettings.");
      var ollamaModelId = Configuration["OllamaModelId"];
      if (String.IsNullOrWhiteSpace(ollamaModelId))
        throw new Exception("Provider is set to Ollama but 'OllamaModelId' is empty in appsettings.");
      try
      {
        IChatClient client = new OllamaChatClient(new Uri(ollamaUrl), ollamaModelId);
        var result = await client.CompleteAsync(
          [
              new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, prompt)
          ]);
        var text = result.Message.Text;
        if (text == null)
          throw new Exception("Ollama response is null.");
        return text;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Ollama.", ex);
        exception.Data.Add("ollamaUrl", ollamaUrl);
        exception.Data.Add("ollamaModelId", ollamaModelId);
        throw exception;
      }
    }

    private async Task<string> GetResults(string prompt)
    {
      var provider = Configuration["provider"];
      try
      {
        switch (provider?.ToUpper())
        {
          case "AWS":
            return await GetFromAws(prompt);
          case "OLLAMA":
            return await GetFromOllama(prompt);
          case "OPENAI":
            return await GetFromOpenAi(prompt);
          case "GROQ":
            return await GetFromGroq(prompt);
          case null:
            throw new Exception($"Provider not set.");
          default:
            throw new Exception($"Unknown provider:'{provider}'");
        }
      }
      catch (Exception ex)
      {
        Exception exception = new("Error fetching results.", ex);
        exception.Data.Add("provider", provider);
        throw exception;
      }
    }

    public bool CheckRateLimit()
    {
      //******Rate Limiting******
      string? key = this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString();
      if (key == null)
        throw new Exception("Remote IP address is null.");
      try
      {
        if (memoryCache.TryGetValue(key, out RequestRate? requestRate) && requestRate != null)
        {
          int minRateLimitRequestCount = Configuration.GetValue<int>("MinRateLimitRequestCount", 5);
          if (requestRate.Count > minRateLimitRequestCount)
          {
            float rateLimit = Configuration.GetValue<float>("RateLimit", 0.1f);
            var rate = requestRate.GetRate();
            if (rate > rateLimit)
            {
              Console.WriteLine($"{key} was rate limited to {rateLimit} requests per second. Current rate is {rate} requests per second.");
              return true;
            }
          }
          requestRate.Increment();
        }
        else
        {
          int rateLimitCacheMinutes = Configuration.GetValue<int>("RateLimitCacheMinutes", 1);
          requestRate = new RequestRate(key);
          var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(rateLimitCacheMinutes));
          memoryCache.Set(key, requestRate, cacheEntryOptions);
        }
        return false;
      }
      catch (Exception ex)
      {
        Exception exception = new("An error occurred in rate limiting.", ex);
        exception.Data.Add("key", key);
        throw exception;
      }
    }

    public async Task<IActionResult> GetDialogue(string dialogueDataJSON)
    {
      DialogueData? dialogueData = null;
      if (dialogueDataJSON == null)
        throw new Exception("dialogueDataJSON is null.");

      if (CheckRateLimit())
        return new JsonResult(new DialogueResponse { RateLimited = true });

      //******Deserialization******
      try
      {
        dialogueData = JsonConvert.DeserializeObject<DialogueData>(dialogueDataJSON);
      }
      catch (Exception ex)
      {
        Exception exception = new("An error occurred deserializing JSON.", ex);
        exception.Data.Add("dialogueDataJSON", dialogueDataJSON);
        throw exception;
      }

      //******Prompt Generation******
      string? prompt = null;
      try
      {
        if (dialogueData == null)
          throw new Exception("dialogueData is null.");
        PromptTemplate promptTemplate = new(dialogueData, Configuration);
        prompt = promptTemplate.TransformText();
        int maxPromptLength = Configuration.GetValue<int>("MaxPromptLength", 1200);
        if (prompt.Length > maxPromptLength)
        {
          Console.WriteLine($"Prompt truncated to {maxPromptLength} characters. Original length was {prompt.Length} characters.");
          prompt = prompt.Substring(0, maxPromptLength);
        }
      }
      catch (Exception ex)
      {
        Exception exception = new("An error occurred generating prompt.", ex);
        exception.Data.Add("dialogueData", JsonConvert.SerializeObject(dialogueData));
        throw exception;
      }

      //******Response Generation******
      string? text = null;
      try
      {
        text = await GetResults(prompt);
      }
      catch (Exception ex)
      {
        Exception exception = new("An error occurred fetching results.", ex);
        exception.Data.Add("prompt", prompt);
        throw exception;
      }

      //******Metrics******
      ServerMetrics.AddRequest(
        this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString(),
        prompt.Length,
        text.Length);

      //******Response Serialization******
      DialogueResponse? dialogueResponse = null;
      try
      {
        dialogueResponse = new DialogueResponse();
        int maxResponseLength = Configuration.GetValue<int>("MaxResponseLength", 5000);
        if (text.Length > maxResponseLength)
        {
          Console.WriteLine($"Response truncated to {maxResponseLength} characters. Original length was {text.Length} characters.");
          text = text.Substring(0, maxResponseLength) + "...";
        }
        dialogueResponse.text = text;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error creating DialogueResponse.", ex);
        exception.Data.Add("text", text);
        throw exception;
      }

      Log(prompt, text);

      return new JsonResult(dialogueResponse);
    }
  }
}
