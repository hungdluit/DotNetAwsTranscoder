using System;
using System.Linq;
using System.Web.Http;
using AwsTranscoder.Services;

namespace AwsTranscoder.Controllers
{
    public class PresetsController : ApiController
    {
        private readonly TranscoderService _transcoderService;

        public PresetsController()
        {
            _transcoderService = new TranscoderService();
        }

        // GET api/presets
        public IHttpActionResult Get()
        {
            try
            {
                var response = _transcoderService.GetPresets();
                return Ok(response.Presets.Select(x => new { x.Id, x.Name }).ToArray());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}
