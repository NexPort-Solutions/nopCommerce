(function ($) {
  "use strict";

  var initializedDataKey = "nexportPasswordRecoveryInitialized";

  function prepareValidationSummary($form, $button) {
    var $summary = $form.find(
      '[data-valmsg-summary="true"], .nexport-password-recovery-error, .validation-summary-errors, .message-error')
      .first();
    if (!$summary.length) {
      $summary = $(
        '<div class="message-error validation-summary-valid nexport-password-recovery-error" ' +
        'data-nexport-password-recovery-summary="true" role="alert" aria-live="polite"></div>');
    }

    var $buttons = $form.find(".buttons").first();
    if (!$buttons.length) {
      $buttons = $button;
    }

    if ($buttons.parent().length && !$summary.is($buttons) &&
      !$.contains($buttons[0], $summary[0]) && !$.contains($summary[0], $buttons[0])) {
      $summary.insertBefore($buttons);
    }

    return $summary;
  }

  function createElementLookup($form, selector, attribute) {
    var $elements = $form.find(selector);
    var byKey = Object.create(null);
    $elements.each(function () {
      var key = $(this).attr(attribute);
      if (key && !byKey[key]) {
        byKey[key] = $(this);
      }
    });

    return { elements: $elements, byKey: byKey };
  }

  function updateValidationErrors($form, $summary, validationMessages, formControls, errors) {
    validationMessages.elements.text("")
      .removeClass("field-validation-error")
      .addClass("field-validation-valid");
    $form.find('[aria-invalid="true"]').removeAttr("aria-invalid");

    var summaryMessages = [];
    var hasErrors = false;
    $.each(errors || {}, function (key, value) {
      var values = $.isArray(value) ? value : [value];
      var messages = $.grep(values, $.trim);
      if (messages.length === 0) {
        return;
      }

      hasErrors = true;

      if (!key) {
        $.merge(summaryMessages, messages);
        return;
      }

      var $validationMessage = validationMessages.byKey[key];
      var $formControl = formControls.byKey[key];
      if ($validationMessage && $validationMessage.length) {
        $validationMessage.text(messages.join(" "))
          .removeClass("field-validation-valid")
          .addClass("field-validation-error");
      } else {
        $.merge(summaryMessages, messages);
      }

      if ($formControl && $formControl.length) {
        $formControl.attr("aria-invalid", "true");
      }
    });

    $summary.empty();
    if (summaryMessages.length === 0) {
      $summary.removeClass("validation-summary-errors").addClass("validation-summary-valid");
      return hasErrors;
    }

    var $list = $("<ul>");
    $.each(summaryMessages, function (index, message) {
      $("<li>").text(message).appendTo($list);
    });
    $summary.append($list)
      .removeClass("validation-summary-valid")
      .addClass("validation-summary-errors");
    return hasErrors;
  }

  function initialize() {
    var config = window.nexportPasswordRecoveryCooldown;
    if (!config) {
      return;
    }

    var $form = $(".password-recovery-page form").first();
    if (!$form.length) {
      $form = $("form").filter(function () {
        var action = $(this).attr("action");
        return action && action.toLowerCase().indexOf("passwordrecovery") >= 0;
      }).first();
    }

    if (!$form.length) {
      return;
    }

    var $button = $form.find(
      '[name="send-email"], .password-recovery-button, button[type="submit"], input[type="submit"]')
      .first();
    if (!$button.length || $form.data(initializedDataKey)) {
      return;
    }

    $form.data(initializedDataKey, true);
    var $summary = prepareValidationSummary($form, $button);
    var validationMessages = createElementLookup($form, "[data-valmsg-for]", "data-valmsg-for");
    var formControls = createElementLookup($form, "[name]", "name");
    var isInput = $button.is("input");
    var originalContent = isInput ? $button.val() : $button.html();
    var originalAttributes = {
      "aria-label": $button.attr("aria-label"),
      title: $button.attr("title"),
      "aria-disabled": $button.attr("aria-disabled"),
      "aria-busy": $button.attr("aria-busy")
    };
    var originalDisabled = $button.prop("disabled");
    var timerId = null;
    var requestPending = false;
    var clearValidationOnCooldownExpiry = false;

    function setButtonLabel(label, busy) {
      if (isInput) {
        $button.val(label);
      } else {
        $button.text(label);
      }
      $button.attr({
        "aria-label": label,
        title: label,
        "aria-busy": busy ? "true" : "false"
      });
    }

    function renderCountdown(seconds) {
      var template = config.tryAgainIn || "Try again in {0}";
      template = template.replace(/\s*\.$/, "");
      var minutes = Math.floor(seconds / 60);
      var duration = minutes + ":" + String(seconds % 60).padStart(2, "0");
      var label = template.indexOf("{0}") >= 0
        ? template.replace("{0}", duration)
        : template + " " + duration;

      setButtonLabel(label, false);
    }

    function stopCountdown() {
      if (timerId !== null) {
        window.clearInterval(timerId);
        timerId = null;
      }
    }

    function restoreButton() {
      stopCountdown();
      $button.prop("disabled", originalDisabled).removeClass("nexport-password-recovery-cooldown");
      if (isInput) {
        $button.val(originalContent);
      } else {
        $button.html(originalContent);
      }

      $.each(originalAttributes, function (attribute, value) {
        if (value === undefined) {
          $button.removeAttr(attribute);
        } else {
          $button.attr(attribute, value);
        }
      });
    }

    function setCooldownState() {
      $button.prop("disabled", true)
        .addClass("nexport-password-recovery-cooldown")
        .attr("aria-disabled", "true");
    }

    function finishCountdown() {
      stopCountdown();
      if (requestPending) {
        setCooldownState();
        setButtonLabel(config.sending || "Sending...", true);
        return;
      }

      if (clearValidationOnCooldownExpiry) {
        updateValidationErrors($form, $summary, validationMessages, formControls, {});
        clearValidationOnCooldownExpiry = false;
      }

      restoreButton();
    }

    function startCountdown(seconds) {
      var remaining = Math.max(0, Math.ceil(Number(seconds) || 0));
      stopCountdown();
      if (remaining <= 0) {
        finishCountdown();
        return;
      }

      var expiresAt = Date.now() + remaining * 1000;
      setCooldownState();
      renderCountdown(remaining);
      timerId = window.setInterval(function () {
        var nextRemaining = Math.max(0, Math.ceil((expiresAt - Date.now()) / 1000));
        if (nextRemaining >= remaining) {
          return;
        }

        remaining = nextRemaining;
        if (remaining <= 0) {
          finishCountdown();
          return;
        }

        renderCountdown(remaining);
      }, 1000);
    }

    function showNotification(message, messageType) {
      if (message) {
        window.displayBarNotification([message], messageType, 0);
      }
    }

    function applyResponse(result) {
      requestPending = false;
      var hasErrors = updateValidationErrors($form, $summary, validationMessages, formControls, result.errors);
      showNotification(result.message, result.messageType || (result.success ? "success" : "error"));

      var cooldown = result.cooldown || {};
      var remainingSeconds = Math.max(0, Math.ceil(Number(cooldown.remainingSeconds) || 0));
      clearValidationOnCooldownExpiry = remainingSeconds > 0 && hasErrors;
      config.remainingSeconds = remainingSeconds;
      startCountdown(remainingSeconds);
    }

    function handleRequestFailure() {
      requestPending = false;
      restoreButton();
      showNotification(config.requestFailed || "We couldn't process your request. Please try again.", "error");
    }

    function submitWithAjax() {
      var submitName = $button.attr("name") || "send-email";
      var submitValue = $button.val() || $button.text() || submitName;
      var requestData = $form.serialize();
      requestData += (requestData ? "&" : "") + $.param([{ name: submitName, value: submitValue }]);
      requestPending = true;
      startCountdown(config.submitSafetyTimeoutSeconds || 30);
      $.ajax({
        url: $form.attr("action") || window.location.href,
        type: "POST",
        data: requestData,
        dataType: "json",
        headers: {
          "X-Nexport-Password-Recovery-Ajax": "true"
        }
      }).done(function (result) {
        if (!result || !result.cooldown) {
          handleRequestFailure();
          return;
        }

        applyResponse(result);
      }).fail(handleRequestFailure);
    }

    $form.on("submit.nexportPasswordRecovery", function (event) {
      if (requestPending || $button.prop("disabled")) {
        event.preventDefault();
        return;
      }

      if (!$form.valid()) {
        return;
      }

      event.preventDefault();
      submitWithAjax();
    });

    startCountdown(config.remainingSeconds);
  }

  $(initialize);
}(jQuery));
