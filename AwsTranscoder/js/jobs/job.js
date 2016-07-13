/// <reference path="../common/modal.js" />
/// <reference path="../services/job.service.js" />
/// <reference path="../services/storage.service.js" />
/// <reference path="../services/preset.service.js" />
/// <reference path="~/Scripts/jquery-2.2.4.intellisense.js" />
/// <reference path="~/Scripts/knockout-3.4.0.debug.js" />
function Job(initialData) {
  var self = this;

  self.modalViewModel = new Modal();
  var jobService = new JobService();
  var storageService = new StorageService();
  var presetService = new PresetService();

  //////////// Public fields
  self.Id = ko.observable(initialData.Id);
  self.Status = ko.observable(initialData.Status);
  self.StatusDetail = ko.observable(initialData.StatusDetail);
  self.FileName = ko.observable(initialData.FileName);
  self.PresetId = ko.observable(initialData.PresetId);
  self.PresetName = ko.observable(initialData.PresetName);
  self.Thumbnails = ko.observable(initialData.Thumbnails);
  self.Rotate = ko.observable(initialData.Rotate);
  self.SubmittedDate = ko.observable(initialData.SubmittedDate);

  // new job request object
  var requestData = {
    Id: self.Id,
    FileName: self.FileName,
    FileNameKey: null,
    PresetId: self.PresetId,
    Thumbnails: self.Thumbnails,
    Rotate: self.Rotate
  }


  //////////// Public functions and read-only fields

  self.Processing = ko.observable(false);

  // get the right icon to display based on status
  self.statusClass = ko.pureComputed(function() {

    switch (self.Status()) {
    case 'Submitted':
      return 'glyphicon-time text-info';
    case 'Progressing':
      return 'glyphicon-refresh glyphicon-spin text-info';
    case 'Complete':
      return 'glyphicon-ok text-success';
    case 'Error':
      return 'glyphicon-warning-sign text-danger';
    default:
      return 'glyphicon-ban-circle text-muted';
    }
  });

  self.IsSubmitted = ko.computed(function() {
    return self.Status() === 'Submitted';
  }, self);

  self.IsCompleted = ko.computed(function() {
    return self.Status() === 'Complete';
  }, self);

  self.DownloadLink = ko.pureComputed(function() {
    return '/Home/Download?id=' + self.Id();
  }, self);


  self.rotateOptions = jobService.rotateOptions;
  self.presetOptions = ko.observableArray([]);

  self.getPresets = function() {
    presetService.presets().done(function(presetData) {
      $.each(presetData, function(key, value) {
        self.presetOptions.push(value);
      });
    });
  };



  //////////// Public functions

  // Create job on form submit
  self.createJob = function() {

    self.Processing(true);

    // Handle user validation
    var file = $('form.frm-create input[type=file]')[0].files[0];
    if (!file) {
      self.modalViewModel.open('Unable to Create Job', 'Did you forget the video?  You must supply a valid video before creating a new transcode job.');
      self.Processing(false);
      return;
    }

    if (!self.PresetId()) {
      self.modalViewModel.open('Unable to Create Job', 'You need to select a preset to transcode the video to.');
      self.Processing(false);
      return;
    }

    if (!isVideo(file.name)) {
      self.modalViewModel.open('Unable to Create Job', 'The file supplied isn\'t a valid video type.');
      self.Processing(false);
      return;
    }

    // To work with ajax and asp.net API, need to submit the file separately
    var uploadFile = function() {

      return storageService.upload(file)
        .fail(function(err) {
          self.modalViewModel.open('Unable to create the job, an unexpected occurred');
          self.Processing(false);
        });
    };

    var submitJob = function(fileKey) {
      self.FileName(file.name);
      requestData.FileNameKey = fileKey;

      return jobService.createJob(requestData)
        .done(function () {
          if (self.refreshJobs)
            self.refreshJobs();

          var fileInput = $('form.frm-create input[type=file]');
          fileInput.replaceWith(fileInput = fileInput.clone(true));

          self.Id('');
          self.FileName('');
          requestData.FileNameKey = null;
          self.PresetId(null);
          self.Thumbnails(false);
          self.Rotate('');
        })
        .fail(function(err) {
          self.modalViewModel.open('Unable to create the job, an unexpected occurred');
        })
        .always(function() {
          self.Processing(false);
        });
    };

    // Chain two together, upload file should be done first
    uploadFile()
      .done(submitJob);


    ///////// functions

    function getExtension(filename) {
      var parts = filename.split('.');
      return parts[parts.length - 1];
    }

    function isVideo(filename) {
      var ext = getExtension(filename);
      switch (ext.toLowerCase()) {
      case 'm4v':
      case 'avi':
      case 'mpg':
      case 'mp4':
        // etc
        return true;
      }
      return false;
    }

  };

  // Opens a pipeline between the job list vm and add job vm
  self.refreshJobs = null;
}