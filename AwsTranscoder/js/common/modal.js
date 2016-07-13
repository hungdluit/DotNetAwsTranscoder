// Used for displaying a popup message to the user.
function Modal() {
  var self = this;

  //////////// Public fields
  self.ShowModal = ko.observable(false);
  self.ModalTitle = ko.observable('');
  self.ModalMessage = ko.observable('');
  self.ConfirmCallBack = ko.observable(false);

  //////////// Public functions
  var confirmCb = null;
  self.confirm = function () {
    self.ShowModal(false);
    confirmCb();
  };


  self.open = function (title, message, cb) {
    self.ModalTitle(title);
    self.ModalMessage(message);

    if (cb) {
      self.ConfirmCallBack(true);
      confirmCb = cb;
    }

    self.ShowModal(true);
  }

  self.close = function () {
    self.ShowModal(false);
  }

  // custom event handler for showing / hiding modal
  ko.bindingHandlers.ToggleModal = {
    init: function (element, valueAccessor) {
      var value = valueAccessor();
      $(element).addClass('modal').addClass('fade').modal({ keyboard: false, show: ko.unwrap(value) });;
    },
    update: function (element, valueAccessor) {
      var value = valueAccessor();
      ko.unwrap(value) ? $(element).modal('show') : $(element).modal('hide');
    }
  };
}