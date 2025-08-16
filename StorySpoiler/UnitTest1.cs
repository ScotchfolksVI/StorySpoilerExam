
using NUnit.Framework;
using NUnit.Framework.Constraints;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoiler.Models;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace StorySpoiler
{
    [TestFixture]
    public class StorySpoilerTests
    {
        private RestClient client;
        private static string createdStoryId;
        private static string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("vili", "vili123");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            client = new RestClient(options);
        }
        
        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);

            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString();
        }


        [Test, Order(1)]
        public void CreateStorySpoiler_WithRequiredFields()
        {
            var story = new 
            {
                Title = "New Story Spoiler",
                Description = "Story Spoiler Descrption",
                Url = ""
            };
        

        var request = new RestRequest("/api/Story/Create", Method.Post);
        request.AddJsonBody(story);
        var response = client.Execute(request);
        var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
        createdStoryId = json.GetProperty("storyId").GetString();


            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(createdStoryId, Is.Not.Empty);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Successfully created!"));

        }

        [Test, Order(2)]

        public void EditStorySpoiler_ShouldReturnSuccess()
            
        {
            var editedRequest = new StoryDTO
            {
                Title = "Edited Story",
                Description = "This is an updated story",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);
            request.AddJsonBody(editedRequest);

            var response = this.client.Execute(request);

       
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json.Msg, Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]

        public void GetAllStorySpoilers()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = this.client.Execute(request);

            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseItems, Is.Not.Null);
            Assert.That(responseItems, Is.Not.Empty);
        }

        [Test, Order(4)]

        public void DeleteStorySpoilers()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);
          
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }
        
        [Test, Order(5)]

        public void CreateStorySpoiler_WithoutRequiredFields()
        {
            var storyRequest = new StoryDTO
            {
                Title = "",
                Description = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(storyRequest);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]

        public void EditNonExistingStorySpoiler_ShouldReturnNotFound()
        {
            string nonExistingStoryId = "123";
            var editRequest = new StoryDTO
            {
                Title = "Edited Non-Existing Story",
                Description = "This is an updated test story description for a non-existing story.",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{nonExistingStoryId}", Method.Put);
            request.AddJsonBody(editRequest);

            var response = this.client.Execute(request);

            var json = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);


            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No spoilers..."));
        }

        [Test, Order(7)]

        public void DeleteNonExistingStorySpoiler_ShouldReturnBadRequest()
        {
            string nonExistingStoryId = "123";
            var request = new RestRequest($"/api/Story/Delete/{nonExistingStoryId}", Method.Delete);

            var response = this.client.Execute(request);
 
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}