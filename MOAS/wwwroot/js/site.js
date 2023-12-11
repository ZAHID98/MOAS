/// <reference path="../plugins/jquery/jquery.js" />
// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//-------all functions Account/Index----------
function ResetPassword(UserID) {
    $.post({
        url: "/Account/ResetPassword",
        dataType: "json",
        data: { UserID: UserID },
        success: function (result) {
            if (result.flag == 'y') {
                toastr.success(result.msg);
            }
            else {
                toastr.error(result.msg);
            }
        }
    }
    );
}

function SetPermission(UserID) {
    $.get({
        url: "/Account/SetPermission",
        dataType: "html",
        data: { UserID: UserID },
        success: function (result) {
            $("#divPermission").html(result);
            $("#mdPermission").modal();
        }
    }
    );
}

function SubmitPermission() {
    var FormData = $("#frmPermission").serialize();
    $.post({
        url: "/Account/SetPermission",
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: FormData,
        success: function (result) {
            if (result.flag == 'y') {
                toastr.success(result.msg);
            }
            else {
                toastr.error(result.msg);
            }
        },
        error: function () {
            toastr.error(result.msg);
        }
    }
    );
}
//-------all functions Account/Register----------
function Register() {
    $("#frmRegister").validate();

    if ($("#frmRegister").valid() === true) {
        var FormData = $("#frmRegister").serialize();
        $.post({
            url: "/Account/Register",
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
            data: FormData,
            success: function (result) {
                if (result.flag == 'y') {
                    toastr.success(result.msg);
                }
                else {
                    toastr.error(result.msg);
                }
            },
            error: function () {
                toastr.error(result.msg);
            }
        }
        );
    }
}
//-------all functions Account/Edit----------
function UpdateUser() {
    $("#frmUpdate").validate();

    if ($("#frmUpdate").valid() === true) {
        var FormData = $("#frmUpdate").serialize();
        $.post({
            url: "/Account/Edit",
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
            data: FormData,
            success: function (result) {
                if (result.flag == 'y') {
                    toastr.success(result.msg);
                }
                else {
                    toastr.error(result.msg);
                }
            },
            error: function () {
                toastr.error(result.msg);
            }
        }
        );
    }
}
//-------all functions Common----------
function FormReset(frm) {
    $("#" + frm)[0].reset();
}


