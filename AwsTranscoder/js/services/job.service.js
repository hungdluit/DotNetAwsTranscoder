// Handles all the job requests to the web api
function JobService() {
  var self = this;

  self.rotateOptions = ['90', '180', '270'];

  self.createJob = function(jobData) {

    var dataObject = ko.toJSON(jobData);

    return $.ajax({
      url: '/api/jobs',
      type: 'post',
      data: dataObject,
      processData: false,
      contentType: 'application/json'
    });
  };

  self.cancelJob = function (jobId) {
    return $.ajax({
      url: '/api/jobs/' + jobId,
      type: 'delete',
      contentType: 'application/json'
    });
  };

  self.getList = function() {
    return $.getJSON('/api/jobs');
  }
}