using System;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using RestSharp;
using RestSharp.Authenticators;
using NUnit.Framework;
using MovieCatalogExam.Models;

namespace MovieCatalogTests
{
    [TestFixture]
    public class MovieTests
    {
        private RestClient client;
        private static string createdMovieId;

        private const string BaseUrl = "http://144.91.123.158:5000";

        private const string Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI4MTc3NTg4NC1jZWM4LTQzODQtYmI4OC01ODEzYzg4M2M0NWUiLCJpYXQiOiIwNC8xOC8yMDI2IDA2OjE4OjEyIiwiVXNlcklkIjoiMGRlMmFlZWItYjBlNy00ZmQxLTYyNDgtMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJ2ZXNlbGEucGF2bG92YUBleGFtcGxlLmNvbSIsIlVzZXJOYW1lIjoidmVzZWxhcGF2bG92YTEiLCJleHAiOjE3NzY1MTQ2OTIsImlzcyI6Ik1vdmllQ2F0YWxvZ19BcHBfU29mdFVuaSIsImF1ZCI6Ik1vdmllQ2F0YWxvZ19XZWJBUElfU29mdFVuaSJ9.7GZ2EJ9Rj91zYL10_hqFmo3TCfrK-S5XbztybZDVs78";

        private static readonly JsonSerializerOptions jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        [OneTimeSetUp]
        public void Setup()
        {
            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(Token)
            };

            client = new RestClient(options);
        }

        [Order(1)]
        [Test]
        public void CreateMovie_ShouldReturnSuccess()
        {
            var movie = new MovieDto
            {
                Title = "Test Movie",
                Description = "Test Description"
            };

            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movie);

            var response = client.Execute(request);
            var result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content, jsonOptions);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Msg, Is.EqualTo("Movie created successfully!"));

            createdMovieId = result.Movie.Id;
        }

        [Order(2)]
        [Test]
        public void EditMovie_ShouldReturnSuccess()
        {
            var movie = new MovieDto
            {
                Title = "Edited Movie",
                Description = "Edited Description"
            };

            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", createdMovieId);
            request.AddJsonBody(movie);

            var response = client.Execute(request);
            var result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content, jsonOptions);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Catalog/All", Method.Get);

            var response = client.Execute(request);
            var movies = JsonSerializer.Deserialize<List<MovieDto>>(response.Content, jsonOptions);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(movies, Is.Not.Null.And.Not.Empty);
        }

        [Order(4)]
        [Test]
        public void DeleteMovie_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", createdMovieId);

            var response = client.Execute(request);
            var result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content, jsonOptions);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateMovie_WithMissingFields_ShouldReturnBadRequest()
        {
            var movie = new MovieDto
            {
                Title = "",
                Description = ""
            };

            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movie);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void EditNonExistingMovie_ShouldReturnBadRequest()
        {
            var movie = new MovieDto
            {
                Title = "Fake",
                Description = "Fake"
            };

            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", "00000000-0000-0000-0000-000000000000");
            request.AddJsonBody(movie);

            var response = client.Execute(request);
            var result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content, jsonOptions);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(result.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingMovie_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", "00000000-0000-0000-0000-000000000000");

            var response = client.Execute(request);
            var result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content, jsonOptions);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(result.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            client?.Dispose();
        }
    }
}