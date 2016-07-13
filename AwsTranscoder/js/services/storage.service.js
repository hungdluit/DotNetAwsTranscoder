// Handles all the storage requests to the web api
function StorageService() {
  var self = this;

  self.upload = function(file) {

    var data = new FormData();
    data.append('file', file);

    return $.ajax({
      url: '/api/storage',
      type: 'post',
      data: data,
      processData: false,
      contentType: false
    });
  };
}