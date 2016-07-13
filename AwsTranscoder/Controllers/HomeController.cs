using System.IO;
using System.Net.Mime;
using System.Web.Configuration;
using System.Web.Mvc;
using AwsTranscoder.Helpers;
using AwsTranscoder.Services;
using Ionic.Zip;

namespace AwsTranscoder.Controllers
{
    public class HomeController : Controller
    {
        private readonly TranscoderService _transcoderService;
        private readonly StorageService _storageService;

        public HomeController()
        {
            _transcoderService = new TranscoderService();
            _storageService = new StorageService();
        }

        public ActionResult Index()
        {
            ViewBag.JobKeepLimit = WebConfigurationManager.AppSettings["AWSJobKeepLimit"];
            return View();
        }

        public ActionResult Download(string id)
        {

            var job = _transcoderService.JobById(id).Job;
            var pipeLineResponse = _transcoderService.Pipeline();

            var files = _storageService.FilesStartWith(job.Output.Key.WithoutExtension(), pipeLineResponse.Pipeline.OutputBucket);
            if (files.Length == 0)
                // Redirect them back to the list if the files don't exist
                return RedirectToAction("Index"); 

            // Create a zip file based on the output files
            var zipFile = new ZipFile();
            var stream = new MemoryStream();

            // Extract the original vanity name used for the file.
            var vanityName =
                (job.UserMetadata.ContainsKey("name")
                    ? job.UserMetadata["name"]
                    : job.Output.Key).WithoutExtension();

            foreach (var file in files)
                // replace S3 keyed file names back with the original vanity name
                zipFile.AddEntry(file.Name.Replace(job.Output.Key.WithoutExtension(), vanityName), file.OpenRead());


            zipFile.Save(stream);
            stream.Position = 0;

            return File(stream, MediaTypeNames.Application.Octet, $"{vanityName}.zip");
            
        }
    }
}
