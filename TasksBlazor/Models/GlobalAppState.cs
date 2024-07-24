using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using TasksBlazor.Components;
using TasksBlazor.Components.Pages;
using TasksBlazor.Models.Requests;
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
        public List<GetTaskResponse> tickets { get; set; } = new List<GetTaskResponse>();
        public List<GetTaskResponse> todoTickets { get; set; } = new List<GetTaskResponse>();
        public List<GetTaskResponse> progressTickets { get; set; } = new List<GetTaskResponse>();
        public List<GetTaskResponse> doneTickets { get; set; } = new List<GetTaskResponse>();

        public void SaveToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);

            Username = jwtSecurityToken.Claims.FirstOrDefault(p => p.Type == Constants.UserIdClaim)?.Value ?? "no user claim";
            UserId = int.Parse(jwtSecurityToken.Claims.FirstOrDefault(p => p.Type == Constants.UserIdClaim)?.Value ?? "-1");
            if (jwtSecurityToken.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Role)?.Value == "admin")
            {
                IsAdmin = true;
            }
            Token = token;
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

        public async Task FetchTasksAsync()
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync("Tasks");
                if (!response.IsSuccessStatusCode)
                {
                    throw new BadHttpRequestException("FetchTasksAsync failed in GlobalAppState");
                }

                var result = await response.Content.ReadFromJsonAsync<List<GetTaskResponse>>();

                tickets = result!;

                todoTickets = tickets.Where(x => x.Status == "ToDo").ToList();
                progressTickets = tickets.Where(x => x.Status == "InProgress").ToList();
                doneTickets = tickets.Where(x => x.Status == "Done").ToList();
            }
            catch (BadHttpRequestException ex)
            {
                Console.WriteLine(ex);
            }
            catch (System.Text.Json.JsonException ex)
            {
                Console.WriteLine("Serialization failed in FetchTasksAsync in GlobalAppState\n" + ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task FetchTasksAsync(string username)
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync($"Tasks/GetUserTasks/{username}");
                if (!response.IsSuccessStatusCode)
                {
                    throw new BadHttpRequestException($"Fetch tasks for {username} is not successful in GlobalAppState");
                }

                var result = await response.Content.ReadFromJsonAsync<List<GetTaskResponse>>();

                tickets = result!;

                todoTickets = tickets.Where(x => x.Status == "ToDo").ToList();
                progressTickets = tickets.Where(x => x.Status == "InProgress").ToList();
                doneTickets = tickets.Where(x => x.Status == "Done").ToList();
            }
            catch (BadHttpRequestException ex)
            {
                Console.WriteLine(ex);
            }
            catch (System.Text.Json.JsonException ex)
            {
                Console.WriteLine("Serialization failed in FetchTasksAsync in GlobalAppState\n" + ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex);
            }
        }

        public (List<GetTaskResponse>, List<GetTaskResponse>, List<GetTaskResponse>) GetTasks(string filter = "")
        {
            if (filter.IsNullOrEmpty())
            {
                return (todoTickets, progressTickets, doneTickets);
            }
            var todoList = todoTickets.Where(x => x.Categories.Any(x => x.Name.Contains(filter))).ToList();
            var progressList = progressTickets.Where(x => x.Categories.Any(x => x.Name.Contains(filter))).ToList();
            var doneList = doneTickets.Where(x => x.Categories.Any(x => x.Name.Contains(filter))).ToList();

            return (todoList, progressList, doneList);
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

                todoTickets = tickets.Where(x => x.Status == "ToDo").ToList();
                progressTickets = tickets.Where(x => x.Status == "InProgress").ToList();
                doneTickets = tickets.Where(x => x.Status == "Done").ToList();
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex);
            }
        }

        public UpdateTaskRequest GetTaskModel(int Id)
        {
            try
            {
                var ticket = tickets.FirstOrDefault(x => x.Id == Id);
                if (ticket == null)
                {
                    throw new NullReferenceException("Ticket is null in GetTaskModel in GlobalAppState");
                }

                var model = new UpdateTaskRequest
                {
                    AssignedTo = ticket.AssignedTo,
                    CreatedBy = ticket.CreatedBy,
                    Description = ticket.Description,
                    Id = ticket.Id,
                    ModifiedBy = ticket.ModifiedBy,
                    Priority = ticket.Priority,
                    Status = ticket.Status,
                    Subject = ticket.Subject,
                    Categories = ticket.Categories
                };

                return model;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex);
            }

            return new UpdateTaskRequest { Subject = "Ticket not found"};
        }

        public void AddCategory(int ticketId, GetCategoryResponse category)
        {
            try
            {
                var ticket = tickets.FirstOrDefault(x => x.Id == ticketId);
                if (ticket == null)
                {
                    throw new NullReferenceException("Ticket is null in GlobalAppState AddCategory");
                }

                if (ticket.Categories.Any(x => x.Name == category.Name))
                {
                    return;
                }

                ticket.Categories.Add(category);

                var model = new UpdateTaskRequest
                {
                    AssignedTo = ticket.AssignedTo,
                    CreatedBy = ticket.CreatedBy,
                    Description = ticket.Description,
                    Id = ticket.Id,
                    ModifiedBy = ticket.ModifiedBy,
                    Priority = ticket.Priority,
                    Status = ticket.Status,
                    Subject = ticket.Subject,
                    Categories = ticket.Categories
                };
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex);
            }
        }
        
        public void RemoveCategory(int ticketId, int categoryId)
        {
            try
            {
                var ticket = tickets.FirstOrDefault(x => x.Id == ticketId);
                if (ticket == null)
                {
                    throw new NullReferenceException("Ticket is null in GlobalAppState in RemoveCategory");
                }
                var category = ticket.Categories.Where(x => x.Id == categoryId).FirstOrDefault();
                ticket.Categories.Remove(category!);
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void RemoveTicket(int ticketId)
        {
            try
            {
                var ticket = tickets.FirstOrDefault(x => x.Id == ticketId);
                if (ticket == null)
                {
                    throw new NullReferenceException("Ticket is null in GlobalAppState in RemoveTicket");
                }

                tickets.Remove(ticket);

                todoTickets = tickets.Where(x => x.Status == "ToDo").ToList();
                progressTickets = tickets.Where(x => x.Status == "InProgress").ToList();
                doneTickets = tickets.Where(x => x.Status == "Done").ToList();
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void AddTicket(GetTaskResponse ticket)
        {
            tickets.Add(ticket);

            todoTickets = tickets.Where(x => x.Status == "ToDo").ToList();
            progressTickets = tickets.Where(x => x.Status == "InProgress").ToList();
            doneTickets = tickets.Where(x => x.Status == "Done").ToList();
        }

        public void Logout()
        {
            tickets = new List<GetTaskResponse>();
            todoTickets = new List<GetTaskResponse>();
            progressTickets = new List<GetTaskResponse>();
            doneTickets = new List<GetTaskResponse>();

            Token = "";
            IsAdmin = false;
        }
    }
}
