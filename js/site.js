window.showLogoutPopup = function (name) {
    if (confirm("Logout, " + name + "?")) {
        window.location.href = "/Account/Logout";
    }
};

$(document).ready(function () {
    $("#login-form").submit(function (e) {
        e.preventDefault();
        if ($(this).valid()) {
            var formData = $(this).serialize();
            $.ajax({
                url: loginUrl,
                type: "POST",
                data: formData,
                success: function (response) {
                    $("#login-message").text(response.Message)
                        .css("color", response.Success ? "green" : "red")
                        .show();
                    if (response.Success) {
                        setTimeout(function () {
                            window.location.href = dashboardUrl;
                        }, 1000);
                    }
                },
                error: function (xhr) {
                    $("#login-message").text(xhr.responseText || "Login failed.")
                        .css("color", "red")
                        .show();
                }
            });
        }
    });

    $("#register-form").submit(function (e) {
        e.preventDefault();
        if ($(this).valid()) {
            var formData = new FormData(this);
            $.ajax({
                url: registerUrl,
                type: "POST",
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    $("#register-message").text(response.Message)
                        .css("color", response.Success ? "green" : "red")
                        .show();
                    if (response.Success) {
                        setTimeout(function () {
                            window.location.href = dashboardUrl;
                        }, 1000);
                    }
                },
                error: function (xhr) {
                    $("#register-message").text(xhr.responseText || "Registration failed.")
                        .css("color", "red")
                        .show();
                }
            });
        }
    });

  
});


