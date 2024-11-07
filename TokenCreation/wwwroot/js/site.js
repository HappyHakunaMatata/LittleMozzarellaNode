// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.



$(function () {
    $(".needs-validation")
        .find("input,select,textarea")
        .on("input", function () {
            if ($(this).val() === '') {
                $(this).removeClass("is-valid is-invalid");
                return;
            }
            $(this)
                .removeClass("is-valid is-invalid")
                .addClass(this.checkValidity() ? "is-valid" : "is-invalid");
        });
});