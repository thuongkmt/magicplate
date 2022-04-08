$(document).ready(function () {
  var title = $("title");
  title.text("Konbini Vending Automation - Cashless API");
  var link = $("a.link");
  var img = link.children(0);
  console.log(img);
  img.attr("src", "/swagger-ui/logo.png");
  img.attr("width", "100");
  link.attr("alt", "Konbini Vending Automation");
  link.attr("href", "http://konbinisg.com");

  var text = link.children(1);
  text.text("");

  //$(document).ready(function () {
  konbini.swagger.hwstatus(function (status) {
    var hgroup = $("hgroup.main");
    hgroup.append(status);
  });

  $("#select").change(function (e) {
    $(document).ready(function () {
      konbini.swagger.hwstatus(function (status) {
        var hgroup = $("hgroup.main");
        hgroup.append(status);
      });
    });

  });
  //});
});

