// Write your JavaScript code.
//for sending ajax requests to the controllers
$(document).on({
    ajaxStart: function () { $('body').addClass("loading"); },
    ajaxStop: function () { $('body').removeClass("loading"); }
});


function viewTouch() {
    $.ajax({
        type: "POST",
        url: "/AccessionCodes/ViewTouchCodes",
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (data) {
            $("#myModal").modal('show');
            $("#topic").text("Touch Codes");
            $("#listOfCodes").html(data.trim().split(" ").join());
        },
        failure: function (response) {
            alert('error');
        }
    });
}

function viewAllCodes() {
    $.ajax({
        type: "POST",
        url: "/AccessionCodes/ViewALLCodes",
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (data) {
            $("#myModal").modal('show');
            $("#topic").text("Accession Codes");
            var a = data.trim().split(" ");
            $("#listOfCodes").html(a.join());
        },
        failure: function (response) {
            alert('error');
        }
    });

}

function searchCode(code) {
    if (code == "") {
        alert("Please enter a code");
        return false;
    }
    $.ajax({

        type: "POST",
        url: "/AccessionCodes/SearchCode?code=" + code.trim(),
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (data) {
            $('#label1').show();
            $('#txtarea1').show();
            $("#txtarea1").html(data);
        },
        failure: function (response) {
            alert('error');
        }
    });

}

function publishSelectedOrALL(a) {
    if (a == "single") {
        if ($('#code').val() == "") {
            alert("Please select a site");
            return false;
        }
        var site = $('#sites').val();
        var hiturl = "/Location/PublishSelectedOrALL?path=" + $('#path').val().trim() + "&code=" + site;
    }
    else {
        var hiturl = "/Location/PublishSelectedOrALL";
    }
    $.ajax({

        type: "POST",
        url: hiturl,
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (data) {
            $("#myModal").modal('show');
            $("#topic").text("Publish Result");
            $('#listOfCodes').html(data);
        },
        failure: function (response) {
            alert('error');
        }
    });
}

function verifyAll() {

    $.ajax({

        type: "POST",
        url: "/Location/VerifyAll",
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (data) {
            $("#myModal").modal('show');
            $("#topic").text("Verification Result");
            $('#listOfCodes').html(data);
        },
        failure: function (response) {
            alert('error');
        }
    });
}


function verifySelected() {

    if ($('#code').val() == "") {
        alert("Please select a site");
        return false;
    }
    var site = $('#sites').val();
    var hiturl = "/Location/VerifySelected?" + "&code=" + site;
    $.ajax({

        type: "POST",
        url: hiturl,
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (data) {
            $("#myModal").modal('show');
            $("#topic").text("Verification Result");
            $('#listOfCodes').html(data);
        },
        failure: function (response) {
            alert('error');
        }
    });
}

function ftp() {

    //if ($('#path').text() == "") {
    //    alert("Please select a site");
    //    return false;
    //}
    //var site = $('#sites').val();
    //var hiturl = "/Location/VerifySelected?" + "&code=" + site;
    $.ajax({

        type: "POST",
        url: "/Location/Ftp",
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (data) {
            $("#myModal").modal('show');
            $("#topic").text("Verification Result");
            $('#listOfCodes').html(data);
        },
        failure: function (response) {
            alert('error');
        }
    });
}



function removeLocation(a) {
    if ($('#sites').val() == undefined) {
        alert("Please select a site");
        return false;
    }
    var site = $('#sites').val();
    a.href += "?site=" + site;
    return confirm("Are you Sure You Want to Delete This Location?");

}




function convertToFTP() {
   // var id = $(this).data('assigned-id');
   // var route = '@Url.Action("ViewTest", "Home")?id=' + id;
    var url = "/Location/ConvertToFTPForm?code="+$('#code').val().trim();
    $('#partial').load(url);
    $('#modalTopic').text("Convert to FTP Site");
}


function recoverDBForm() {
    // var id = $(this).data('assigned-id');
    // var route = '@Url.Action("ViewTest", "Home")?id=' + id;
    var url = "/Location/RecoverDB"
    $('#partial').load(url);
    $('#modalTopic').text("Recover Database");
}


function recoverDB(path) {
    // var id = $(this).data('assigned-id');
    // var route = '@Url.Action("ViewTest", "Home")?id=' + id;
    if (!confirm("Are you Sure ? You cannot undo it later. "))
        return false;
    $('#closebutton').click();
    $.ajax({

        type: "POST",
        url: "/Location/RecoverDBFinal?path="+path.trim(),
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (data) {
            $("#myModal").modal('show');
            $("#topic").text("Restoration Results");
            $('#listOfCodes').html(data);
        },
        failure: function (response) {
            alert('error');
        }
    });
    //var url = "/Location/RecoverDB"
    //$('#partial').load(url);
    //$('#modalTopic').text("Recover Database");
}



function pingAllSites() {

    $.ajax({

        type: "POST",
        url: "/Location/PingAllSites",
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (data) {
            $("#myModal").modal('show');
            $("#topic").text("Ping Results");
            $('#listOfCodes').html(data);
        },
        failure: function (response) {
            alert('error');
        }
    });

}

function pingSelectedSite(server, host) {
    if (server == "" && host == "") {
        alert("Please select a site");
        return false;
    }
    if (server == "")
        var hiturl = "/Location/PingSelectedSite?server=" + host.trim();
    else
        var hiturl = "/Location/PingSelectedSite?server=" + server.trim();
    $.ajax({

        type: "POST",
        url: hiturl,
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (data) {
            $("#myModal").modal('show');
            $("#topic").text("Ping Result");
            $('#listOfCodes').html(data);
        },
        failure: function (response) {
            alert('error');
        }
    });

}

function viewSelectedSite(path, a) {
    if (path == "") {
        alert("Please select a site");
        return false;
    }
    a.href = a.href.split("?")[0] + "?path=" + path;

    return true;
    //$.ajax({

    //    type: "post",
    //    url: "/location/viewselectedsite?path=" + path.trim(),
    //    contenttype: "application/json; charset=utf-8",
    //    datatype: "text",
    //    success: function (data) {
    //        if (data != "") {
    //            $("#mymodal").modal('show');
    //            $("#topic").text("error");
    //            $('#listofcodes').html(data);
    //        }

    //    },
    //    failure: function (response) {
    //        alert('error');
    //    }
    //});

}
function eidtLocation(a) {
    if ($('#sites').val() == undefined) {
        alert("Please select a site");
        return false;
    }
    var code = $('#code').val();
    a.href += "?code=" + code;
    return true;

}

function viewSiteDetails(a) {
   
    $.ajax({
        type: "POST",

        url: "/Location/ViewSiteDetails?expression=" + a.trim(),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
           
            var a = data;
            $('#name').val(a[0].name);
            $('#code').val(a[0].code);
            if (a[0].isFTP == "N") {
                $('#username').val("");
                $('#usernamediv').hide();
                $('#password').val("");
                $('#passworddiv').hide();
                $('#host').val("");
                $('#hostdiv').hide();
                $('#directory').val("");
                $('#directorydiv').hide();
                $('#port').val("");
                $('#portdiv').hide();
                $('#statusdiv').show();
                $('#serverdiv').show();
                $('#pathdiv').show();
                $('#retentiondiv').show();
                $('#diagnosticdiv').show();
                $('#accessiondiv').show();
                $('#status').val(a[0].status);
                $('#server').val(a[0].server);
                $("#path").val(a[0].path);
                $("#rp").val(a[0].retentionPeriod);
                $("#dgl").val(a[0].diagnosticLevel);

                var x = [];

                a[1].forEach(function (arrayItem) {
                    if (arrayItem.isTouch.trim() == "Y")
                        x.push("<span style='color:blue;font-weight:bold;'>" + arrayItem.code + "</span>");
                    else
                        x.push(arrayItem.code);

                });
                // $("#ac").text(x.join());
                var s = x.join();
                tinymce.get("ac").setContent(s);
            }
            else {
                $('#status').val("");
                $('#server').val("");
                $("#path").val("");
                $("#rp").val("");
                $("#dgl").val("");
                $('#statusdiv').hide();
                $('#serverdiv').hide();
                $('#pathdiv').hide();
                $('#retentiondiv').hide();
                $('#diagnosticdiv').hide();
                $('#accessiondiv').hide();

                $('#usernamediv').show();
                $('#passworddiv').show();
                $('#hostdiv').show();
                $('#directorydiv').show();
                $('#portdiv').show();

                $('#host').val(a[0].host);
                $('#username').val(a[0].username);
                $("#directory").val(a[0].directoryPath);
                $("#password").val(a[0].passwrd);
                $("#port").val(a[0].port);

            }
            if ($('#host').val() == "") {

                $('#ftprow').show();

            }
            else {

                $('#ftprow').hide();
            }


        },
        failure: function (response) {
            alert('error');
        }
    });
}


