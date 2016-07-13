using System;
using System.Collections.Generic;
using System.Web.Configuration;
using Amazon.ElasticTranscoder;
using Amazon.ElasticTranscoder.Model;
using AwsTranscoder.Helpers;
using System.Linq;

namespace AwsTranscoder.Services
{
    /// <summary>
    /// Service that wraps the Amazon Elastic Transcoder library and any other "Transcoder" related tasks.
    /// </summary>
    public class TranscoderService
    {
        private readonly AmazonElasticTranscoderClient _transcoderClient;
        private readonly string _pipelineId;
        private readonly int _jobKeepLimit;

        public TranscoderService()
        {
            _pipelineId = WebConfigurationManager.AppSettings["AWSPipelineId"];
            _jobKeepLimit = WebConfigurationManager.AppSettings["AWSJobKeepLimit"].ToInt();

            _transcoderClient = new AmazonElasticTranscoderClient();
        }

        /// <summary>
        /// Gets the pipeline to use for all the jobs.
        /// Note: Pipeline to use is set in app settings under AWSPipelineId
        /// </summary>
        /// <returns>AWS pipeline response</returns>
        public ReadPipelineResponse Pipeline()
        {
            return _transcoderClient.ReadPipeline(new ReadPipelineRequest() { Id = _pipelineId });
        }

        /// <summary>
        /// Gets a list of the jobs currently in a pipeline.
        /// </summary>
        /// <returns>AWS jobs by pipeline response</returns>
        public ListJobsByPipelineResponse Jobs()
        {
            return _transcoderClient.ListJobsByPipeline(new ListJobsByPipelineRequest() { PipelineId = _pipelineId });
        }

        /// <summary>
        /// Gets the jobs to list
        /// </summary>
        /// <returns>List of jobs</returns>
        public IEnumerable<Models.Job> GetJobsList()
        {
            var jobsResponse = Jobs();
            var presetsReponse = GetPresets();

            // Convert all utc times as EST
            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            return (from job in jobsResponse.Jobs
                    join preset in presetsReponse.Presets on job.Output.PresetId equals preset.Id
                    orderby job.Timing.SubmitTimeMillis descending
                    select new Models.Job(job, preset, easternZone)).Take(_jobKeepLimit);
        }

        /// <summary>
        /// Get the oldest job in the list based on the limit set in the app settings under AWSJobKeepLimit
        /// </summary>
        /// <returns></returns>
        public Job GetOldestJob()
        {
            return Jobs()?.Jobs.OrderByDescending(x => x.Timing.SubmitTimeMillis).Skip(_jobKeepLimit).FirstOrDefault();
        }

        /// <summary>
        /// Cancels a submitted job.  
        /// Note: Only jobs in submitted status are allowed to be canceled.
        /// </summary>
        /// <param name="jobId">job id</param>
        /// <returns>AWS cancel job response</returns>
        public CancelJobResponse CancelJob(string jobId)
        {
           return _transcoderClient.CancelJob(new CancelJobRequest() { Id = jobId });
        }

        /// <summary>
        /// Get the job by job id
        /// </summary>
        /// <param name="jobId">job id</param>
        /// <returns>AWS get job response</returns>
        public ReadJobResponse JobById(string jobId)
        {
            return _transcoderClient.ReadJob(new ReadJobRequest() {Id = jobId});
        }

        /// <summary>
        /// Gets all the current presets available to transcode with
        /// </summary>
        /// <returns>AWS preset response</returns>
        public ListPresetsResponse GetPresets()
        {
            return _transcoderClient.ListPresets(new ListPresetsRequest());
        }

        /// <summary>
        /// Creates and adds a new job to the pipeline queue
        /// </summary>
        /// <param name="name">Vanity name of the file</param>
        /// <param name="fileName">Unique file name for storage</param>
        /// <param name="rotate">How much to rotate the file (in degrees)</param>
        /// <param name="makeThumbs">Whether or not to create thumbnails</param>
        /// <param name="presetId">The preset used for transcoding</param>
        /// <returns>AWS create job response</returns>
        public CreateJobResponse CreateJob(string name, string fileName, string rotate, bool makeThumbs, string presetId)
        {
            // Get the preset so we know what file type is going to be created in the output
            var preset = _transcoderClient.ReadPreset(new ReadPresetRequest() { Id = presetId });

            var jobOutput = new CreateJobOutput()
            {
                PresetId = presetId,
                Key = fileName.WithoutExtension() + "." + preset.Preset.Container, // set filename of the output 
                Rotate = rotate,
                // ThumbnailPattern is what aws keys on to determine whether or not to create thumbs
                ThumbnailPattern = makeThumbs ? fileName.WithoutExtension() + "-thumb-{count}" : null
            };

            var jobRequest = new CreateJobRequest()
            {
               PipelineId = _pipelineId,
               UserMetadata = new Dictionary<string,string>() { { "name", name } }, //store the original vanity file name
               Input = new JobInput() { Key = fileName },
               Output = jobOutput
            };

            return _transcoderClient.CreateJob(jobRequest);
        }
    }
}