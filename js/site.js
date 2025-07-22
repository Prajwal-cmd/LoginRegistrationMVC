window.showLogoutPopup = function (name) {
    if (confirm("Logout, " + name + "?")) {
        window.location.href = "/Account/Logout";
    }
};

$.validator.addMethod("customEmail", function (value, element) {
    if (!value || value.trim() === "") return true;
    var regExp = /^([a-zA-Z0-9._%+-]+)@([a-zA-Z0-9.-]+\.[a-zA-Z]{2,})$/;
    return regExp.test(value);
}, "Email must be a valid address (e.g., user@domain.com).");

$.validator.addMethod("validDate", function (value, element) {
    if (!value || value.trim() === "") return false;
    var dob = new Date(value);
    return !isNaN(dob.getTime()) && dob < new Date();
}, "Date of Birth must be a valid past date.");

$(document).ready(function () {
    $("#loginButton").on("click", function () {
        var email = $("#email").val().trim();
        var password = $("#password").val().trim();
        var isValid = true;

        if (!email) {
            $("#email-error").text("Email is required.").show();
            isValid = false;
        } else if (!$.validator.methods.customEmail.call(this, email, $("#email")[0])) {
            $("#email-error").text("Email must be a valid address (e.g., user@domain.com).").show();
            isValid = false;
        } else {
            $("#email-error").hide();
        }

        if (!password) {
            $("#password-error").text("Password is required.").show();
            isValid = false;
        } else if (password.length < 6) {
            $("#password-error").text("Password must be at least 6 characters.").show();
            isValid = false;
        } else {
            $("#password-error").hide();
        }

        if (isValid) {
            var data = {
                Email: email,
                Password: password
            };

            $.ajax({
                url: loginUrl,
                type: "POST",
                data: JSON.stringify(data),
                contentType: "application/json",
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
                    var errorMsg = "Login failed due to an unexpected error.";
                    try {
                        var response = JSON.parse(xhr.responseText);
                        if (response && !response.Success && response.Message) {
                            errorMsg = response.Message;
                        }
                        $("#login-message").text(errorMsg)
                            .css("color", "red")
                            .show();
                        $("#email-error").hide();
                        $("#password-error").hide();
                    } catch (e) {
                        $("#login-message").text(errorMsg)
                            .css("color", "red")
                            .show();
                    }
                    console.log("AJAX error: ", xhr.responseText);
                }
            });
        }
    });

    $("#registerButton").on("click", function () {
        var name = $("#name").val().trim();
        var email = $("#email").val().trim();
        var password = $("#password").val().trim();
        var confirmPassword = $("#confirm-password").val().trim();
        var gender = $("#gender").val();
        var dateOfBirth = $("#date-of-birth").val();
        var imageFile = $("#image")[0].files[0];
        var isValid = true;

        if (!name) {
            $("#name-error").text("Name is required.").show();
            isValid = false;
        } else if (name.length > 50) {
            $("#name-error").text("Name cannot exceed 50 characters.").show();
            isValid = false;
        } else {
            $("#name-error").hide();
        }

        if (!email) {
            $("#email-error").text("Email is required.").show();
            isValid = false;
        } else if (!$.validator.methods.customEmail.call(this, email, $("#email")[0])) {
            $("#email-error").text("Email must be a valid address (e.g., user@domain.com).").show();
            isValid = false;
        } else {
            $("#email-error").hide();
        }

        if (!password) {
            $("#password-error").text("Password is required.").show();
            isValid = false;
        } else if (password.length < 6) {
            $("#password-error").text("Password must be at least 6 characters.").show();
            isValid = false;
        } else {
            $("#password-error").hide();
        }

        if (!confirmPassword) {
            $("#confirm-password-error").text("Confirm Password is required.").show();
            isValid = false;
        } else if (confirmPassword !== password) {
            $("#confirm-password-error").text("Confirm Password must match the password.").show();
            isValid = false;
        } else {
            $("#confirm-password-error").hide();
        }

        if (!gender) {
            $("#gender-error").text("Gender is required.").show();
            isValid = false;
        } else {
            $("#gender-error").hide();
        }

        if (!dateOfBirth) {
            $("#date-of-birth-error").text("Date of Birth is required.").show();
            isValid = false;
        } else if (!$.validator.methods.validDate.call(this, dateOfBirth, $("#date-of-birth")[0])) {
            $("#date-of-birth-error").text("Date of Birth must be a valid past date.").show();
            isValid = false;
        } else {
            $("#date-of-birth-error").hide();
        }

        if (!imageFile) {
            $("#image-error").text("Image is required.").show();
            isValid = false;
        } else {
            var validTypes = ["image/jpeg", "image/png", "image/gif"];
            var maxSize = 5 * 1024 * 1024; 
            if (!validTypes.includes(imageFile.type)) {
                $("#image-error").text("Image must be JPEG, PNG, or GIF.").show();
                isValid = false;
            } else if (imageFile.size > maxSize) {
                $("#image-error").text("Image size must not exceed 5MB.").show();
                isValid = false;
            } else {
                $("#image-error").hide();
            }
        }

        if (isValid) {
            var formData = new FormData();
            formData.append("Name", name);
            formData.append("Email", email);
            formData.append("Password", password);
            formData.append("ConfirmPassword", confirmPassword);
            formData.append("Gender", gender);
            formData.append("DateOfBirth", dateOfBirth);
            if (imageFile) formData.append("Image", imageFile);

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
                    var errorMsg = "Registration failed due to an unexpected error.";
                    try {
                        var response = JSON.parse(xhr.responseText);
                        if (response && !response.Success && response.Message) {
                            errorMsg = response.Message;
                        }
                        $("#register-message").text(errorMsg)
                            .css("color", "red")
                            .show();
                        $("#name-error").hide();
                        $("#email-error").hide();
                        $("#password-error").hide();
                        $("#confirm-password-error").hide();
                        $("#gender-error").hide();
                        $("#date-of-birth-error").hide();
                        $("#image-error").hide();
                    } catch (e) {
                        $("#register-message").text(errorMsg)
                            .css("color", "red")
                            .show();
                    }
                    console.log("AJAX error: ", xhr.responseText);
                }
            });
        }
    });
});