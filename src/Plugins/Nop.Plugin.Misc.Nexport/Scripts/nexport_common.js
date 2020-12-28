function navigateToNexport(url) {
  var nexportWindow = window.open("about:blank", "_blank");

  $.ajax({
    url: url,
    type: "GET",
    success: function (result) {
      if (result && result.RedirectUrl) {
        nexportWindow.location.href = result.RedirectUrl;
      }
    },
    error: function (xhr, textStatus, errorThrown) {
      var response = xhr.responseJSON;
      if (response && response.Error) {
        nexportWindow.close();
        displayBarNotification(response.Error, "error");
      }
    }
  });
}