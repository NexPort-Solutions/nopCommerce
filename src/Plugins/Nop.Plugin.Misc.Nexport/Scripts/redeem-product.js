var Assign = {
  loadWaiting: false,
  failureUrl: false,

  init: function (failureUrl) {
    this.loadWaiting = false;
    this.failureUrl = failureUrl;

    Accordion.disallowAccessToNextSections = true;
  },

  ajaxFailure: function () {
    location.href = Assign.failureUrl;
  },

  _disableEnableAll: function (element, isDisabled) {
    var descendants = element.find('*');
    $(descendants).each(function () {
      if (isDisabled) {
        $(this).prop("disabled", true);
      } else {
        $(this).prop("disabled", false);
      }
    });

    if (isDisabled) {
      element.prop("disabled", true);
    } else {
      $(this).prop("disabled", false);
    }
  },

  setLoadWaiting: function (step, keepDisabled) {
    var container;
    if (step) {
      if (this.loadWaiting) {
        this.setLoadWaiting(false);
      }

      container = $("#" + step + "-buttons-container");
      container.addClass("disabled");
      container.css("opacity", ".5");

      this._disableEnableAll(container, true);

      $("#" + step + "-please-wait").show();
    } else {
      if (this.loadWaiting) {
        container = $("#" + this.loadWaiting + "-buttons-container");

        var isDisabled = keepDisabled ? true : false;
        if (!isDisabled) {
          container.removeClass("disabled");
          container.css("opacity", "1");
        }

        this._disableEnableAll(container, isDisabled);

        $("#" + this.loadWaiting + "-please-wait").hide();
      }
    }

    this.loadWaiting = step;
  },

  gotoSection: function (section) {
    section = $('#assign-' + section);
    section.addClass('allow');
    Accordion.openSection(section);
  },

  back: function () {
    // if (this.loadWaiting) return;
    Accordion.openPrevSection(true, true);
  },

  setStepResponse: function (response) {
    if (response.update_section) {
      $("#assignment-" + response.update_section.name + "-load").html(response.update_section.html);
    }

    if (response.goto_section) {
      Assign.gotoSection(response.goto_section);

      return true;
    }

    if (response.redirect) {
      location.href = response.redirect;

      return true;
    }
    return false;
  }
};

var Customer = {
  form: false,
  saveUrl: false,

  init: function (form, saveUrl) {
    this.form = form;
    this.saveUrl = saveUrl;
  },

  continue: function () {
    $("#CustomerStepSendViaEmail").prop("checked", true);

    this.save();
  },

  save: function () {
    if (Assign.loadWaiting !== false) return;

    Assign.setLoadWaiting('customer');

    $.ajax({
      cache: false,
      url: this.saveUrl,
      data: $(this.form).serialize(),
      type: "POST",
      success: this.nextStep,
      complete: this.resetLoadWaiting,
      error: Assign.ajaxFailure
    });
  },

  resetLoadWaiting: function () {
    Assign.setLoadWaiting(false);
  },

  nextStep: function (response) {
    if (response.error) {
      if (typeof response.message === "string") {
        alert(response.message);
      } else {
        alert(response.message.join("\n"));
      }

      return false;
    }

    $("#CustomerStepSendViaEmail").prop("checked", false);

    Assign.setStepResponse(response);
  }
};

var Product = {
  form: false,
  saveUrl: false,
  productId: false,

  init: function (form, saveUrl) {
    this.form = form;
    this.saveUrl = saveUrl;
  },

  save: function () {
    if (Assign.loadWaiting !== false) return;

    Assign.setLoadWaiting("product");

    $.ajax({
      cache: false,
      url: this.saveUrl,
      data: $(this.form).serialize(),
      type: "POST",
      success: this.nextStep,
      complete: this.resetLoadWaiting,
      error: Assign.ajaxFailure
    });
  },

  resetLoadWaiting: function () {
    Assign.setLoadWaiting(false);
  },

  nextStep: function (response) {

    if (response.error) {
      if (typeof response.message === "string") {
        alert(response.message);
      } else {
        alert(response.message.join("\n"));
      }

      return false;
    }

    Assign.setStepResponse(response);
  }
};

var ProductOptions = {
  form: false,
  saveUrl: false,
  productId: false,

  init: function (form, saveUrl) {
    this.form = form;
    this.saveUrl = saveUrl;
  },

  save: function () {
    if (Assign.loadWaiting !== false) return;

    Assign.setLoadWaiting("productOptions");

    $.ajax({
      cache: false,
      url: this.saveUrl,
      data: $(this.form).serialize(),
      type: "POST",
      success: this.nextStep,
      complete: this.resetLoadWaiting,
      error: Assign.ajaxFailure
    });
  },

  resetLoadWaiting: function () {
    Assign.setLoadWaiting(false);
  },

  nextStep: function (response) {
    if (response.error) {
      if (typeof response.message === "string") {
        alert(response.message);
      } else {
        alert(response.message.join("\n"));
      }

      return false;
    }

    Assign.setStepResponse(response);
  }
};

var Confirm = {
  form: false,
  saveUrl: false,
  successUrl: false,
  isSuccess: false,

  init: function (form, saveUrl, successUrl) {
    this.form = form;
    this.saveUrl = saveUrl;
    this.successUrl = successUrl;
  },

  save: function () {
    if (Assign.loadWaiting !== false) return;

    Assign.setLoadWaiting("confirm");

    //var postData = {};

    //addAntiForgeryToken(postData);
    $.ajax({
      cache: false,
      url: this.saveUrl,
      data: $(this.form).serialize(),
      type: "POST",
      success: this.nextStep,
      complete: this.resetLoadWaiting,
      error: Assign.ajaxFailure
    });
  },

  resetLoadWaiting: function (transport) {
    Assign.setLoadWaiting(false, Confirm.isSuccess);
  },

  nextStep: function (response) {
    if (response.error) {
      if (typeof response.message === "string") {
        alert(response.message);
      } else {
        alert(response.message.join("\n"));
      }

      return false;
    }

    if (response.redirect) {
      Confirm.isSuccess = true;
      location.href = response.redirect;
      return;
    }

    if (response.success) {
      Confirm.isSuccess = true;
      window.location = Confirm.successUrl;
    }

    Assign.setStepResponse(response);
  }
};

/*
** redeem product custom accordion
** it is the same as nopcommerce custom accordion except removed this line:
** location.hash = section.attr('id');
** removed so that page won't scroll when filling out form since it is a smaller form
*/

var Accordion = {
  checkAllow: false,
  disallowAccessToNextSections: false,
  sections: new Array(),
  currentSection: false,
  headers: new Array(),

  init: function (elem, clickableEntity, checkAllow) {
    this.checkAllow = checkAllow || false;
    this.disallowAccessToNextSections = false;
    this.sections = $("#" + elem + " .tab-section");
    this.currentSectionId = false;

    var headers = $("#" + elem + " .tab-section " + clickableEntity);
    headers.on("click", function () {
      Accordion.headerClicked($(this));
    });
  },

  headerClicked: function (section) {
    Accordion.openSection(section.parent(".tab-section"));
  },

  openSection: function (section) {
    var section = $(section);

    if (this.checkAllow && !section.hasClass("allow")) {
      return;
    }
    if (section.attr("id") != this.currentSectionId) {
      var previousSectionId = this.currentSectionId;

      this.closeExistingSection();
      this.currentSectionId = section.attr("id");

      $("#" + this.currentSectionId).addClass("active");

      var contents = section.children(".a-item");
      $(contents[0]).show();

      $(document).trigger({ type: "accordion_section_opened", previousSectionId: previousSectionId, currentSectionId: this.currentSectionId });

      if (this.disallowAccessToNextSections) {
        var pastCurrentSection = false;
        for (var i = 0; i < this.sections.length; i++) {
          if (pastCurrentSection) {
            $(this.sections[i]).removeClass("allow");
          }

          if ($(this.sections[i]).attr("id") == section.attr("id")) {
            pastCurrentSection = true;
          }
        }
      }
    }
  },

  closeSection: function (section) {
    var section = $(section);
    section.removeClass("active");
    var contents = section.children(".a-item");
    $(contents[0]).hide();

    $(document).trigger({ type: "accordion_section_closed", sectionId: section.attr("id") });
  },
  hideSection: function (section) {
    var section = $(section);
    section.hide();

    $(document).trigger({ type: "accordion_section_hidden", sectionId: section.attr("id") });
  },

  showSection: function (section) {
    var section = $(section);
    section.show();
    location.hash = section.attr("id");

    $(document).trigger({ type: "accordion_section_shown", sectionId: section.attr("id") });
  },

  openNextSection: function (setAllow) {
    for (section in this.sections) {
      var nextIndex = parseInt(section) + 1;
      if (this.sections[section].id == this.currentSectionId && this.sections[nextIndex]) {
        if (setAllow) {
          $(this.sections[nextIndex]).addClass("allow");
        }

        this.openSection(this.sections[nextIndex]);

        return;
      }
    }
  },

  openPrevSection: function (setAllow, onlyAllowed) {
    var prevIndex = 0;
    for (section in this.sections) {
      if (onlyAllowed) {
        //ensure that the section is allowed
        var tmp = parseInt(section) - 1;
        if (!isNaN(tmp) && $(this.sections[tmp]).hasClass("allow")) {
          prevIndex = tmp;
        }
      } else {
        prevIndex = parseInt(section) - 1;
      }
      if (this.sections[section].id == this.currentSectionId && this.sections[prevIndex]) {
        if (setAllow) {
          $(this.sections[prevIndex]).addClass("allow");
        }

        this.openSection(this.sections[prevIndex]);

        return;
      }
    }
  },

  closeExistingSection: function () {
    if (this.currentSectionId) {
      this.closeSection($("#" + this.currentSectionId));
    }
  }
};