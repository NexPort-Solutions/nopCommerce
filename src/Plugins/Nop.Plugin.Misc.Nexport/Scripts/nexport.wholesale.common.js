function CardToggle() {
  var card = $(this).parent(".card.card-secondary");
  card.CardWidget('toggle');
}

//collapse search block
$(function () {
  $(".row.search-row").on('click', function (e) {
    ToggleSearchBlockAndSavePreferences();
  });
});

function ToggleSearchBlockAndSavePreferences() {
  $(this).parents(".card-search").find(".search-body").slideToggle();
  var icon = $(this).find(".icon-collapse i");
  var svgIcon = false;
  if (!icon.length) {
    icon = $(this).find(".icon-collapse svg");
    svgIcon = true;
  }

  if ($(this).hasClass("opened")) {
    if (svgIcon) {
      icon.attr("data-icon", "angle-down");
    } else {
      icon.removeClass("fa-angle-up");
      icon.addClass("fa-angle-down");
    }
  } else {
    if (svgIcon) {
      icon.attr("data-icon", "angle-up");
    } else {
      icon.addClass("fa-angle-up");
      icon.removeClass("fa-angle-down");
    }
  }

  $(this).toggleClass("opened");
}

function ensureDataTablesRendered() {
  $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
}

function reloadAllDataTables(itemCount) {
  //depending on the number of elements, the time for animation of opening the menu should increase
  var timePause = 300;
  if (itemCount) {
    timePause = itemCount * 100;
  }
  $('table[class^="table"]').each(function () {
    setTimeout(function () {
      ensureDataTablesRendered();
    }, timePause);
  });
}

/**
 * @param {string} alertId Unique identifier of alert
 * @param {any} text Message text
 */
function showAlert(alertId, text) {
  $('#' + alertId + '-info').text(text);
  $('#' + alertId).trigger("click");
}

//scrolling and hidden DataTables issue workaround
//More info - https://datatables.net/examples/api/tabs_and_scrolling.html
$(function () {
  $('button[data-card-widget="collapse"]').on('click', function (e) {
    //hack with waiting animation.
    //when page is loaded, a box that should be collapsed have style 'display: none;'.that's why a table is not updated
    setTimeout(function () {
      ensureDataTablesRendered();
    }, 1);
  });

  // when tab item click
  $('.nav-tabs .nav-item').on('click', function (e) {
    setTimeout(function () {
      ensureDataTablesRendered();
    }, 1);
  });

  $('ul li a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
    ensureDataTablesRendered();
  });

  $('#advanced-settings-mode').on('click', function (e) {
    ensureDataTablesRendered();
  });

  //when sidebar-toggle click
  $('#nopSideBarPusher').on('click', function (e) {
    reloadAllDataTables();
  });
});

/**
 * @param {string} masterCheckbox Master checkbox selector
 * @param {string} childCheckbox Child checkbox selector
 */
function prepareTableCheckboxes(masterCheckbox, childCheckbox) {
  //Handling the event of clicking on the master checkbox
  $(masterCheckbox).on('click', function (e) {
    $(childCheckbox).prop('checked', $(this).prop('checked'));
  });

  //Handling the event of clicking on a child checkbox
  $(childCheckbox).on('change', function (e) {
    $(masterCheckbox).prop('checked', $(childCheckbox + ':not(:checked)').length === 0 ? true : false);
  });

  //Determining the state of the master checkbox by the state of its children
  $(masterCheckbox).prop('checked', $(childCheckbox).length == $(childCheckbox + ':checked').length && $(childCheckbox).length > 0);
}