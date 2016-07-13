// create app global with the viewmodels inside
var app = (function (app) {

  // the view modal for the job listing page
  app.JobListViewModel = function () {
    var self = this;

    self.addJobViewModel = new Job({});
    self.jobListViewModel = new JobList();

    self.addJobViewModel.refreshJobs = self.jobListViewModel.getJobs;
  };

  return app;
}(app || {}));