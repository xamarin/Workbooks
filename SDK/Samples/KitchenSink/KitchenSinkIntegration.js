console.log("hello from some third party code => %O", xamarin.interactive)

var PersonRenderer = (function () {
  function PersonRenderer () {
  }

  PersonRenderer.prototype.cssClass = "renderer-third-party-person";

  PersonRenderer.prototype.getRepresentations = function () {
    return [
      { shortDisplayName: "Person" }
    ]
  };

  PersonRenderer.prototype.bind = function (renderState) {
    console.log("PersonRenderer: bind: %O", renderState)
    this.renderState = renderState;
  };

  PersonRenderer.prototype.render = function (target) {
    console.log("PersonRenderer: render %O to %O", this.renderState, target)
    var elem = document.createElement("div");
    elem.innerHTML = "<strong>Person: <em>" + this.renderState.source.Name + "</em></strong>";
    target.inlineTarget.appendChild(elem);
  }

  return PersonRenderer;
})();

xamarin.interactive.RendererRegistry.registerRenderer(
  function (source) {
    if (source.$type === "KitchenSinkIntegration.Person")
      return new PersonRenderer;
  }
);