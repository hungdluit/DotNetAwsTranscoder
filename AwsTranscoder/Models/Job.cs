using System;
using Amazon.ElasticTranscoder.Model;
using AwsTranscoder.Helpers;
using AwsJob = Amazon.ElasticTranscoder.Model.Job;

namespace AwsTranscoder.Models
{
    /// <summary>
    /// DTO for request / response via API
    /// </summary>
    public class Job
    {
        public Job()
        { }

        /// <summary>
        /// Job constructor with a aws job.
        /// </summary>
        /// <param name="job">The aws job</param>
        /// <param name="preset">The job preset</param>
        /// <param name="timeZone">The timezone to use</param>
        public Job(AwsJob job, Preset preset, TimeZoneInfo timeZone)
        {
            Id = job.Id;
            FileName = job.UserMetadata.ContainsKey("name") ? job.UserMetadata["name"] : job.Output.Key;
            PresetName = preset.Name;
            Thumbnails = !string.IsNullOrEmpty(job.Output.ThumbnailPattern);
            Rotate = string.IsNullOrEmpty(job.Output.Rotate) ? "none" : job.Output.Rotate;
            Status = job.Status;
            StatusDetail = job.Output.StatusDetail;
            SubmittedDate = //Adjust for Unix time stamp, start after 1/1/1970 then convert to EST
                TimeZoneInfo.ConvertTimeFromUtc(job.Timing.SubmitTimeMillis.DateFromUnixMilli(), timeZone).ToString("g"); 
        }

        public string Id { get; set; }
        public string Status { get; set; }
        public string StatusDetail { get; set; }
        public string FileName { get; set; }
        public string FileNameKey { get; set; }
        public string PresetId { get; set; }
        public string PresetName { get; set; }
        public bool Thumbnails { get; set; }
        public string Rotate { get; set; }
        public string SubmittedDate { get; set; }
    }
}