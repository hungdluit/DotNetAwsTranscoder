// Handles all the preset requests to the web api
function PresetService() {
  var self = this;

  self.presets = function () {
    return $.getJSON('/api/presets');
  };
}