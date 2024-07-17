using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using TasksBlazor.Components.Pages;
using TasksBlazor.Models.Responses;

namespace TasksBlazor.Models
{
    public class GlobalAppState
    {
        public string? Token { get; set; }
        public string? Username { get; set; }
        public int? UserId { get; set; }
        public bool IsLoggedIn => Token != null && Token != "";
        public bool IsAdmin { get; set; } = false;
        public IEnumerable<GetTaskResponse> tickets { get; set; } = new List<GetTaskResponse>();
        public IEnumerable<GetTaskResponse> todoTickets { get; set; } = new List<GetTaskResponse>();
        public IEnumerable<GetTaskResponse> progressTickets { get; set; } = new List<GetTaskResponse>();
        public IEnumerable<GetTaskResponse> doneTickets { get; set; } = new List<GetTaskResponse>();

        public void SaveToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);

            //var claimsIdentity = new ClaimsIdentity(jwtSecurityToken.Claims,
            //    CookieAuthenticationDefaults.AuthenticationScheme);
            Username = jwtSecurityToken.Claims.FirstOrDefault(p => p.Type == Constants.UserIdClaim)?.Value ?? "no user claim";
            UserId = int.Parse(jwtSecurityToken.Claims.FirstOrDefault(p => p.Type == Constants.UserIdClaim)?.Value ?? "-1");
            if (jwtSecurityToken.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Role)?.Value == "Admin")
            {
                IsAdmin = true;
            }
            Token = token;
        }

        public void RemoveToken()
        {
            Token = "";
            IsAdmin = false;
        }

        public HttpClient CreateClient()
        {
            var client = new HttpClient();
            if (!Token.IsNullOrEmpty())
            {
                client.DefaultRequestHeaders.Authorization =
                          new AuthenticationHeaderValue("Bearer", Token);
            }
            client.BaseAddress = new Uri("https://localhost:7111/api/");

            return client;
        }

        public async Task GetTasksAsync()
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync("Tasks");
                if (!response.IsSuccessStatusCode)
                {
                    throw new BadHttpRequestException("GetTasksAsync failed in GlobalAppState");
                }

                var result = await response.Content.ReadFromJsonAsync<List<GetTaskResponse>>();
                if (result == null)
                {
                    throw new InvalidCastException("Serialization failed in GetTasksAsync in GlobalAppState");
                }

                tickets = result;

                todoTickets = result.Where(x => x.Status == "ToDo");
                progressTickets = result.Where(x => x.Status == "InProgress");
                doneTickets = result.Where(x => x.Status == "Done");
            }
            catch (BadHttpRequestException ex)
            {
                Console.WriteLine(ex);
            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine(ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void UpdateTicket(GetTaskResponse updatedTicket)
        {
            try
            {
                var ticket = tickets.Where(x => x.Id == updatedTicket.Id).FirstOrDefault();
                if (ticket == null)
                {
                    throw new NullReferenceException("ticket is null in UpdateTicket in GlobalAppState");
                }
                ticket = updatedTicket;

                todoTickets = tickets.Where(x => x.Status == "ToDo");
                progressTickets = tickets.Where(x => x.Status == "InProgress");
                doneTickets = tickets.Where(x => x.Status == "Done");
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex);
            }
        }

        public UpdateTaskModel GetTaskModel(int Id)
        {
            try
            {
                var ticket = tickets.FirstOrDefault(x => x.Id == Id);
                if (ticket == null)
                {
                    throw new NullReferenceException("Model is null in GetTaskModel in GlobalAppState");
                }

                var model = new UpdateTaskModel
                {
                    AssignedTo = ticket.AssignedTo,
                    CreatedBy = ticket.CreatedBy,
                    Description = ticket.Description,
                    Id = ticket.Id,
                    ModifiedBy = ticket.ModifiedBy,
                    Priority = ticket.Priority,
                    Status = ticket.Status,
                    Subject = ticket.Subject,
                    Categories = ticket.Categories.Select(x => new UpdateCategoryModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Color = x.Color,
                    }).ToList(),
                };

                return model;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex);
            }

            return new UpdateTaskModel { Subject = "Ticket not found"};
        }
    }
}
