using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace WhisperTranscript
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string pythonExe = @"C:\Users\codeclouds-Argha\AppData\Local\Programs\Python\Python313\python.exe";
            string scriptPath = @"C:\Users\codeclouds-Argha\Desktop\Projects\WhisperTranscript\WhisperTranscript\Python\whisper_wrapper.py";
            string videoPath = @"C:\Users\codeclouds-Argha\Downloads\daily-scrum.mp4";


            var psi = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = $"\"{scriptPath}\" \"{videoPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();
            process.WaitForExit();


            #region Req in OpenAiApi
            //var client = new HttpClient();
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "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");

            //var requestBody = new
            //{
            //    model = "gpt-4.1-mini",
            //    messages = new[]
            //    {
            //        new { role = "system", content = "You are a helpful assistant." },
            //        new { role = "user", content = $"Summarize this video transcript:\n\n{output}" }
            //    }
            //};
            //var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            //var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            //var responseString = await response.Content.ReadAsStringAsync();
            //Console.WriteLine("responseString");
            //Console.WriteLine(responseString); 
            #endregion


            #region using Ollama

            Console.WriteLine("\n\n\nEnter your question:");
            string question = Console.ReadLine();

            string response = await AskOllama(output, question);

            Console.WriteLine("\nResponse:");
            Console.WriteLine(response);


            #endregion





            //Console.WriteLine("Transcription:\n" + output);
            //if (!string.IsNullOrWhiteSpace(errors))
            //    Console.WriteLine("Errors:\n" + errors);
        }

        static async Task<string> AskOllama(string transcript, string question)
        {
            using var client = new HttpClient()
            {
                //Timeout = TimeSpan.FromMinutes(10)
                Timeout = Timeout.InfiniteTimeSpan 
            };

            var requestBody = new
            {
                model = "llama3",  
                prompt = $"Here is the video transcript:\n\n{transcript}\n\nQuestion: {question}",
                stream = true
            };

            //var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            //var response = await client.PostAsync("http://localhost:11434/api/generate", content);
            //var responseString = await response.Content.ReadAsStringAsync();

            //using var jsonDoc = JsonDocument.Parse(responseString);
            //var completion = jsonDoc.RootElement
            //    .GetProperty("response")
            //    .GetString();

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/generate")
            {
                Content = content
            };

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            Console.WriteLine("\nAI Response:\n");

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var jsonDoc = JsonDocument.Parse(line);
                    var token = jsonDoc.RootElement.GetProperty("response").GetString();
                    Console.Write(token); 
                }
                catch (JsonException ex)
                {
                    //Exception
                }
            }


            return "completion";
        }
    }
}
