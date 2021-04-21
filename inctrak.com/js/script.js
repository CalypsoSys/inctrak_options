$(document).ready(function() {
  //for easy scrolling
  $.scrollIt({
    upKey: 20, // key code to navigate to the next section 38
    downKey: 40, // key code to navigate to the previous section
    easing: "swing", // the easing function for animation
    scrollTime: 100, // how long (in ms) the animation takes
    activeClass: "active", // class given to the active nav element
    onPageChange: null, // function(pageIndex) that is called when page is changed
    topOffset: -85 // offstet (in px) for fixed top navigation
  });

  //waypoint
  $(".aboutUs").waypoint(
    function(direction) {
      if (direction == "down") {
        $(".addsticky").addClass("sticky");
      } else {
        $(".addsticky").removeClass("sticky");
      }
    },
    {
      offset: "300px;"
    }
  );

    new WOW().init();

    $("#learn_more_id").submit(function (e) {

        e.preventDefault(); // avoid to execute the actual submit of the form.

        var formData = {
            "Created": "0001-01-01T00:00:00",
            "EmailAddress": $("#EMAIL_ADDRESS_ID").val(),
            "Name": $("#NAME_ID").val(),
            "Subject": $("#SUBJECT_ID").val(),
            "Message": $("#MESSAGE_ID").val(),
            "MessageTypeFk": "8",
            "FeedbackPk": 0,
            "ClientData": null,
            "MessageTypeFkNavigation": null
        };


        $.ajax({
            type: "POST",
            url: "https://shared.inctrak.com/api/feedback/save_message/",
            contentType: "application/json;charset=UTF-8",
            data: JSON.stringify(formData),
            success: function (data) {
                alert(data.message);
                $("#EMAIL_ADDRESS_ID").val("");
                $("#NAME_ID").val("");
                $("#SUBJECT_ID").val("");
                $("#MESSAGE_ID").val("")
            },
            error: function (data) {
                alert(data.message);
            }
        });
    });

  // $("a").click(function() {
  //   $(this)
  //     .addClass("active")
  //     .siblings()
  //     .removeClass("active");
  // });
});
