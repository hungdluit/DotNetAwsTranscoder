using System;
using System.Linq;
using System.Web.Http;
using AwsTranscoder.Helpers;
using AwsTranscoder.Models;
using AwsTranscoder.Services;

namespace AwsTranscoder.Controllers
{
    public class JobsController : ApiController
    {
        private readonly TranscoderService _transcoderService;
        private readonly StorageService _storageService;

        public JobsController()
        {
            _transcoderService = new TranscoderService();
            _storageService = new StorageService();
        }

        // GET api/jobs
        public IHttpActionResult Get()
        {
            try
            {
                var jobsList = _transcoderService.GetJobsList();

                return Ok(jobsList.ToArray());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST api/jobs
        public IHttpActionResult Post([FromBody]Job job)
        {
            try
            {
                // filename and preset is required
                if (string.IsNullOrEmpty(job.FileName))
                    return BadRequest("FileName is required");

                if (string.IsNullOrEmpty(job.FileNameKey))
                    return BadRequest("FileNameKey is required");

                if (string.IsNullOrEmpty(job.PresetId))
                    return BadRequest("Preset is required");


                // Housekeeping: when a new one is added, delete the oldest one based on the limit set
                var oldestJob = _transcoderService.GetOldestJob();
                if (oldestJob != null)
                {
                    // get the pipeline so we can get the in / out bucket names
                    var pipeLine = _transcoderService.Pipeline().Pipeline;

                    // AWS doesn't have an endpoint for removing a job, 
                    // only canceling and thats for "Submitted" status only
                    if (oldestJob.Status == "Submitted")
                        _transcoderService.CancelJob(oldestJob.Id);

                    // get the bucket for in / out files so they can be deleted.
                    _storageService.RemoveFilesStartWith(oldestJob.Input.Key.WithoutExtension(), pipeLine.InputBucket);

                    _storageService.RemoveFilesStartWith(oldestJob.Output.Key.WithoutExtension(), pipeLine.OutputBucket);
                }

                _transcoderService.CreateJob(job.FileName, job.FileNameKey, job.Rotate, job.Thumbnails, job.PresetId);

                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE api/jobs/5
        public IHttpActionResult Delete(string id)
        {
            try
            {
                // get the job first
                var job = _transcoderService.JobById(id).Job;

                // get the pipeline so we can get the in / out bucket names
                var pipeLine = _transcoderService.Pipeline().Pipeline;


                if (job.Status != "Submitted")
                     return Ok("The job is currently processing or completed and not allowed to be canceled.");


                _transcoderService.CancelJob(id); 
                
                // from the original job response, get the bucket for in / out files.
                _storageService.RemoveFilesStartWith(job.Input.Key.WithoutExtension(), pipeLine.InputBucket);

                _storageService.RemoveFilesStartWith(job.Output.Key.WithoutExtension(), pipeLine.OutputBucket);

                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}
