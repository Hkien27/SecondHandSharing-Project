// using System.Net.Http.Headers;
// using System.Text;
// using System.Text.Json;
// using OpenAI.Chat;
// using Microsoft.Extensions.Configuration;

// namespace SecondHandSharing.Services{
//    public class OpenAIService
// {
//     private readonly IConfiguration _config;
//     private readonly HttpClient _httpClient;

//     public OpenAIService(IConfiguration config)
//     {
//         _config = config;
//         _httpClient = new HttpClient();
//     }

//     public async Task<string> AskAsync(string userMessage, string context)
//     {
//         var apiKey = _config["OpenAI:ApiKey"];
//         if (string.IsNullOrEmpty(apiKey))
//             throw new Exception("❌ Chưa cấu hình OpenAI API Key");

//         _httpClient.DefaultRequestHeaders.Clear();
//         _httpClient.DefaultRequestHeaders.Authorization =
//             new AuthenticationHeaderValue("Bearer", apiKey);

//         var requestBody = new
//         {
//             model = "gpt-4o-mini", // ✅ ổn định & rẻ
//             messages = new[]
//             {
//                 new {
//                     role = "system",
//                     content = "Bạn là trợ lý bán đồ cũ. Chỉ trả lời dựa trên dữ liệu được cung cấp, không bịa."
//                 },
//                 new {
//                     role = "system",
//                     content = context
//                 },
//                 new {
//                     role = "user",
//                     content = userMessage
//                 }
//             },
//             temperature = 0.3
//         };

//         var json = JsonSerializer.Serialize(requestBody);
//         var content = new StringContent(json, Encoding.UTF8, "application/json");

//         var response = await _httpClient.PostAsync(
//             "https://api.openai.com/v1/chat/completions",
//             content
//         );

//         if (!response.IsSuccessStatusCode)
//         {
//             var err = await response.Content.ReadAsStringAsync();
//             throw new Exception($"❌ OpenAI API lỗi: {err}");
//         }

//         var responseJson = await response.Content.ReadAsStringAsync();
//         using var doc = JsonDocument.Parse(responseJson);

//         return doc
//             .RootElement
//             .GetProperty("choices")[0]
//             .GetProperty("message")
//             .GetProperty("content")
//             .GetString();
//     }
// }
// }