using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DevopsTicketHelper
{
    public class Functions
    {
        private readonly IHttpClientFactory httpFactory;
        private readonly ILogger logger;
        private readonly HttpClient client;
        private readonly string devopsBaseUrl;
        private readonly string tester;
        private int CreatedTestCount { get; set; }
        public Functions(IHttpClientFactory httpFactory, ILogger<Functions> logger)
        {
            this.httpFactory = httpFactory;
            this.logger = logger;
            devopsBaseUrl = "http://projecturl";
            tester = "username";
            client = this.httpFactory.CreateClient("devops");
        }

        public async Task CreateTestItemsFromBuildId(int buildId)
        {
            var currentBuild = await GetTriggerBuildFromCurrentBuild(buildId);
            if (currentBuild.triggeredByBuild == null)
            {
                logger.LogWarning("Triggered build not found");
                return;
            }
            var triggeredBuildId = currentBuild.triggeredByBuild.id;
            var buildWorkItems = await GetWorkItemsFromBuildIdAsync(triggeredBuildId.ToString());
            logger.LogInformation("Triggered BuildId: " + triggeredBuildId);
            if (buildWorkItems != null && buildWorkItems.value != null && buildWorkItems.value.Any())
            {
                foreach (var item in buildWorkItems.value)
                {
                    await CreateTestItems(item.id);
                }
                logger.LogInformation(string.Format("Total Work Item Count:{0}\n" +
                    "Total Created Task Count:{1}", buildWorkItems.value.Count, CreatedTestCount));
            }
            else
            {
                logger.LogWarning("Build work item(s) not found");
            }
        }

        private async Task AddLink(string parentId, string childId)
        {
            var value = new WorkItemPostOpForLink
            {
                rel = "System.LinkTypes.Hierarchy-Reverse",
                url = $"{devopsBaseUrl}/_apis/wit/workItems/{parentId}",
                attributes = new Attributes { comment = "From Ticket Helper" }
            };
            var operationList = new List<WorkItemPostOpLinked> { new WorkItemPostOpLinked { op = "add", path = "/relations/-", value = value } };
            var httpContent = new StringContent(JsonSerializer.Serialize(operationList), Encoding.UTF8, "application/json-patch+json");
            var response = await client.PatchAsync(new Uri($"{devopsBaseUrl}_apis/wit/workitems/{childId}?api-version=5.0"), httpContent);
            await PrintResponse(response);
        }

        private async Task AssignUser(int id)
        {
            var operationList = new List<WorkItemPostOp>
            {
                new WorkItemPostOp
                {
                    op = "add",
                    path = "/fields/System.AssignedTo",
                    value = tester
                }};
            var httpContent = new StringContent(JsonSerializer.Serialize(operationList), Encoding.UTF8, "application/json-patch+json");
            var response = await client.PatchAsync(new Uri($"{devopsBaseUrl}_apis/wit/workitems/{id}?bypassRules=true&api-version=5.0"), httpContent);
            await PrintResponse(response);
        }

        private async Task CreateTestItems(string workId)
        {
            var ticket = await GetWorkItemAsync(workId);
            if (ticket != null)
            {
                bool testTaskIsExists = false;
                bool createTestTask = false;
                if (ticket.fields == null || ticket.fields.Tags == null)
                {
                    createTestTask = true;
                }
                else
                {
                    var tags = ticket.fields?.Tags?.ToLower();
                    if (tags.Contains("notest"))
                    {
                        logger.LogWarning("Children didn't created because notest tag not found.");
                        return;
                    }
                    createTestTask = tags.Contains("test");
                }
                if (ticket.relations != null && ticket.relations.Any(c => c.rel != "ArtifactLink"))
                {
                    foreach (var item in ticket.relations.Where(c => c.rel != "ArtifactLink" && c.rel != "AttachedFile"))
                    {
                        var child = await GetChildItemAsync(item.url);
                        var title = child.fields.Title.ToLower();
                        testTaskIsExists = title.Contains("test");
                    }
                }
                createTestTask = !(ticket.fields?.State == "Done");
                if (!testTaskIsExists && createTestTask)
                {
                    var testTask = await CreateChildItem("Test", ticket.fields.IterationPath);
                    if (testTask != null)
                    {
                        await AddLink(workId, testTask.id.ToString());
                        await AssignUser(testTask.id);
                    }
                    CreatedTestCount++;
                }
            }
        }

        private async Task<BuildDto> GetTriggerBuildFromCurrentBuild(int currentBuildId)
        {
            var response = await client.GetAsync(
                $"{devopsBaseUrl}_apis/build/builds/{currentBuildId}?api-version=5.0");
            return await CheckResponse<BuildDto>(response);
        }

        private async Task<BuildWorkItems> GetWorkItemsFromBuildIdAsync(string buildId)
        {
            var response = await client.GetAsync(
                        $"{devopsBaseUrl}_apis/build/builds/{buildId}/workitems?api-version=5.0");
            return await CheckResponse<BuildWorkItems>(response);
        }

        private async Task<WorkItemDto> GetWorkItemAsync(string id)
        {
            var response = await client.GetAsync(
                        $"{devopsBaseUrl}_apis/wit/workitems/{id}?$expand=Relations&api-version=5.0");
            return await CheckResponse<WorkItemDto>(response);
        }

        private async Task<WorkItemDto> GetChildItemAsync(string url)
        {
            var response = await client.GetAsync(url);
            return await CheckResponse<WorkItemDto>(response);
        }

        private async Task<WorkItemDto> CreateChildItem(string title, string iterationPath, string type = "Task")
        {
            var operationList = new List<WorkItemPostOp>{
                new WorkItemPostOp
                {
                    op="add",
                    path="/fields/System.Title",
                    value=title,
                    from=null
                },
                new WorkItemPostOp
                {
                    op="add",
                    path="/fields/System.IterationPath",
                    value=iterationPath
                }};
            var request = new StringContent(JsonSerializer.Serialize(operationList), Encoding.UTF8, "application/json-patch+json");
            var response = await client.PostAsync(new Uri($"{devopsBaseUrl}_apis/wit/workitems/${type}?api-version=5.0"), request);
            return await CheckResponse<WorkItemDto>(response);
        }

        #region Unused Methods
        private async Task AddTag(int id, string tags)
        {
            var operationList = new List<WorkItemPostOp>{ new WorkItemPostOp
            {
                op="add",
                path="/fields/System.Tags",
                value=tags
            }};
            var httpContent = new StringContent(JsonSerializer.Serialize(operationList), Encoding.UTF8, "application/json-patch+json");
            var response = await client.PatchAsync(new Uri($"{devopsBaseUrl}_apis/wit/workitems/{id}?api-version=5.0"), httpContent);
            await PrintResponse(response);
        }
        #endregion

        private async Task<T> CheckResponse<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                logger.LogError(response.StatusCode.ToString());
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream);
        }

        private async Task PrintResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                logger.LogError(response.StatusCode.ToString());
            logger.LogInformation(await response.Content.ReadAsStringAsync());
        }
    }
}