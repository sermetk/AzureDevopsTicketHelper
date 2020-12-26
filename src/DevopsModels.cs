using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DevopsTicketHelper
{
    public class Avatar
    {
        public string href { get; set; }
    }
    public class Links
    {
        public Avatar avatar { get; set; }
    }
    public class SystemCreatedBy
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public Links _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }
    public class SystemChangedBy
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public Links _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }
    public class Fields
    {
        [JsonPropertyName("System.AreaPath")]
        public string AreaPath { get; set; }
        [JsonPropertyName("System.TeamProject")]
        public string TeamProject { get; set; }
        [JsonPropertyName("System.IterationPath")]
        public string IterationPath { get; set; }
        [JsonPropertyName("System.WorkItemType")]
        public string WorkItemType { get; set; }
        [JsonPropertyName("System.State")]
        public string State { get; set; }
        [JsonPropertyName("System.Reason")]
        public string Reason { get; set; }
        [JsonPropertyName("System.AssignedTo")]
        public SystemAssignedTo AssignedTo { get; set; }
        [JsonPropertyName("System.CreatedDate")]
        public DateTime CreatedDate { get; set; }
        [JsonPropertyName("System.CreatedBy")]
        public SystemCreatedBy CreatedBy { get; set; }
        [JsonPropertyName("System.ChangedDate")]
        public DateTime ChangedDate { get; set; }
        [JsonPropertyName("System.ChangedBy")]
        public SystemChangedBy ChangedBy { get; set; }
        [JsonPropertyName("System.CommentCount")]
        public int CommentCount { get; set; }
        [JsonPropertyName("System.Title")]
        public string Title { get; set; }
        [JsonPropertyName("System.BoardColumn")]
        public string BoardColumn { get; set; }
        [JsonPropertyName("System.BoardColumnDone")]
        public bool BoardColumnDone { get; set; }
        [JsonPropertyName("System.Tags")]
        public string Tags { get; set; }
    }
    public class SystemAssignedTo
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public Links _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }
    public class WorkItemDto
    {
        public int id { get; set; }
        public int rev { get; set; }
        public Fields fields { get; set; }
        public Links _links { get; set; }
        public string url { get; set; }
        public List<Relations> relations { get; set; }
    }
    public class Relations
    {
        public string rel { get; set; }
        public string url { get; set; }
    }
    public class WorkItemPostOp
    {
        public string op { get; set; }
        public string path { get; set; }
        public string value { get; set; }
        public string from { get; set; }
    }
    public class WorkItemPostOpLinked
    {
        public string op { get; set; }
        public string path { get; set; }
        public WorkItemPostOpForLink value { get; set; }
    }
    public class WorkItemPostOpForLink
    {
        public string rel { get; set; }
        public string url { get; set; }
        public Attributes attributes { get; set; }
    }
    public class Attributes
    { public string comment { get; set; } }
    public class BuildWorkItem
    {
        public string id { get; set; }
        public string url { get; set; }
    }
    public class BuildWorkItems
    {
        public int count { get; set; }
        public IList<BuildWorkItem> value { get; set; }
    }
    public class BuildDto
    {
        public int id { get; set; }
        public string buildNumber { get; set; }
        public string status { get; set; }
        public TriggeredByBuild triggeredByBuild { get; set; }
    }
    public class TriggeredByBuild
    {
        public int id { get; set; }
        public string buildNumber { get; set; }
    }
}