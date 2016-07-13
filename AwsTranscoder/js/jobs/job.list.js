/// <reference path="../common/modal.js" />
/// <reference path="~/Scripts/knockout-3.4.0.debug.js" />
/// <reference path="~/js/jobs/job.js" />
function JobList() {
  var self = this;

  self.modalViewModel = new Modal();
  var jobService = new JobService();


  //////////// Public fields

  self.jobs = ko.observableArray([]);


  //////////// Public functions

  // get all the jobs available
  self.getJobs = function () {

    jobService.getList()
      .done(function (data) {
        self.jobs.removeAll();
        $.each(data, function (key, value) {
          self.jobs.push(new Job(value));
        });
      });
  };

  // cancel the job
  self.cancelJob = function (job) {

    var confirmCallBack = function () {
      jobService.cancelJob(job.Id())
        .done(function() {
          self.jobs.remove(job);
        });
    }

    self.modalViewModel.open('Confirmation',
      'Are you sure you want to cancel this job?',
      confirmCallBack);

  };

  //Automatically refresh the list every 10 seconds
  setInterval(self.getJobs, 10000);
}