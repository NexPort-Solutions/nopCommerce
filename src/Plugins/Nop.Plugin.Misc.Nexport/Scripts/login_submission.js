(function ($) {
  "use strict";

  var formSelector = ".login-page form:has(.login-button)";
  var submittingKey = "nexportLoginSubmitting";
  var labelKey = "nexportLoginLabel";

  $(document).off("submit.nexportLoginSubmission", formSelector)
    .on("submit.nexportLoginSubmission", formSelector, function (event) {
      var $form = $(this);
      if ($form.data(submittingKey)) {
        event.preventDefault();
        event.stopImmediatePropagation();
        return;
      }
      if (event.isDefaultPrevented()) {
        return;
      }

      var validator = $form.data("validator");
      if (validator && (validator.pendingRequest > 0 || !$form.valid())) {
        event.preventDefault();
        return;
      }

      var $button = $form.find(".login-button").first();
      var waitingText = $form.closest(".login-page").find("[data-nexport-login-wait]").attr("data-nexport-login-wait");
      $form.data(submittingKey, true).attr("aria-busy", "true");
      $button.data(labelKey, $button.text()).prop("disabled", true);
      if (waitingText) {
        $button.text(waitingText);
      }
    });

  $(window).off("pageshow.nexportLoginSubmission").on("pageshow.nexportLoginSubmission", function (event) {
    if (!event.originalEvent || !event.originalEvent.persisted) {
      return;
    }
    $(formSelector).each(function () {
      var $form = $(this);
      if (!$form.data(submittingKey)) {
        return;
      }
      var $button = $form.find(".login-button").first();
      $button.prop("disabled", false).text($button.data(labelKey));
      $form.removeData(submittingKey).removeAttr("aria-busy");
    });
  });
}(jQuery));
