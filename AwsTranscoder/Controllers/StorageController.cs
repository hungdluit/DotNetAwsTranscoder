using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AwsTranscoder.Services;

namespace AwsTranscoder.Controllers
{
    public class StorageController : ApiController
    {
        private readonly StorageService _storageService;
        private readonly TranscoderService _transcoderService;

        public StorageController()
        {
            _storageService = new StorageService();
            _transcoderService = new TranscoderService();
        }

        // GET api/storage
        [HttpPost]
        public async Task<IHttpActionResult> Post()
        {
            try
            {
                // The work-around in handling files being uploaded via ajax post (thanks Microsoft)
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // extract file name and file contents
                var fileNameParam = provider.Contents[0].Headers.ContentDisposition.Parameters
                    .FirstOrDefault(p => p.Name.ToLower() == "filename");

                var fileName = fileNameParam?.Value.Trim('"') ?? "";
                var file = await provider.Contents[0].ReadAsStreamAsync();

                if(file.Length == 0 || string.IsNullOrEmpty(fileName))
                    return BadRequest("No file was provided");

                //Get the input S3 bucket name set on the pipeline config
                var piplineResponse = _transcoderService.Pipeline();

                //Create unique key and use that for the file being uploaded
                var key = Guid.NewGuid().ToString();
                var fileNameKey = key + Path.GetExtension(fileName);
                _storageService.UploadFile(file, fileNameKey, piplineResponse.Pipeline.InputBucket);

                return Ok(fileNameKey); //return unique so it can be used for the job. 
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}
