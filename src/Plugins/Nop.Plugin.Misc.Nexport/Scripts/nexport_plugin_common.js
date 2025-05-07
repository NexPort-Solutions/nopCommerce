function renderCustomButton(buttonFunction, buttonIconClass, buttonName, renderDisabled) {
  if (!buttonName) {
    throw "Cannot render button without a name!";
  }

  var buttonElement = "<button class=\"btn btn-default\"";
  if (buttonFunction) {
    buttonElement += "onclick=\"" + buttonFunction + "\"";
  }

  if (renderDisabled) {
    buttonElement += " disabled";
  }

  buttonElement += ">";
  if (buttonIconClass) {
    buttonElement += "<i class=\"" + buttonIconClass + "\"></i>";
  }

  buttonElement += buttonName + "</button>";

  return buttonElement;
}

function renderCustomLinkTag(linkUrl, linkFunction, linkIconClass, linkName, renderDisabled) {
  if (!linkIconClass) {
    throw "Link icon class is missing!";
  }

  if (!linkName) {
    throw "Cannot render link without a name!";
  }

  var linkElement = "<a";
  var linkHref = "#";

  if (linkUrl) {
    linkHref = linkUrl;
  }

  linkElement = "<a href=\"" + linkHref + "\" onclick=\"" + linkFunction + "\"";

  var linkElementClass = "btn btn-default";

  if (renderDisabled) {
    linkElementClass += " disabled";
  }

  linkElement += " class=\"" + linkElementClass + "\"";
  linkElement += "><i class=\"" + linkIconClass + "\"></i>" + linkName + "</a>";

  return linkElement;
}

function renderLocalDateForTableEntry(data, type, row, meta) {
  return data ? moment.utc(data).local().format("MM/DD/YYYY HH:mm:ss") : null;
}