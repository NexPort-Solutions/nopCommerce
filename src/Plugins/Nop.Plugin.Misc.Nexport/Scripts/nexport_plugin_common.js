function renderCustomButton(buttonFunction, buttonIconClass, buttonName, renderDisabled) {
  if (!buttonIconClass) {
    throw "Button icon class is missing!";
  }

  if (!buttonName) {
    throw "Cannot render button without a name!";
  }

  var buttonElement = "<button class=\"btn btn-default\" onclick=\"" + buttonFunction + "\"";

  if (renderDisabled) {
    buttonElement += " disabled";
  }

  buttonElement += "><i class=\"" + buttonIconClass + "\"></i>" + buttonName + "</button>";

  return buttonElement;
}

function renderLocalDateForTableEntry(data, type, row, meta) {
  return data ? moment.utc(data).local().format("MM/DD/YYYY HH:mm:ss") : null;
}