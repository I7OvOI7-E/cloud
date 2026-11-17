// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.querySelectorAll(".auth-preview-form").forEach(function (form) {
    form.addEventListener("submit", function (event) {
        event.preventDefault();
        form.querySelector(".preview-message").hidden = false;
    });
});
