// Write your Javascript code.
$('#form_login_submit').click(function (e) {
    e.preventDefault();
    updateViaApi($(this));
});
$('#form_logoff_submit').click(function (e) {
    e.preventDefault();
    updateViaApi($(this));
});
$('#form_updatesession_submit').click(function (e) {
    e.preventDefault();
    updateViaApi($(this));
});

function updateViaApi(referrer) {

    var $button = referrer,
        $form = $(referrer).closest('form');
    var action = $($form).attr('action'),
        data = $($form).serialize(),
        dataType = 'application/x-www-form-urlencoded; charset=utf-8',
        messagebox = $($form).find('.update-warning');

    $(messagebox).removeClass('hidden');

    $.ajax({
        type: 'POST',
        url: action,
        dataType: 'json',
        contentType: dataType,
        data: data,
        success: function (result) {
            $(messagebox).text(result.message);
        }
    }).fail(function (err) {
        $(messagebox).text('failed! see console');
        console.log(err);
    });
};

$('option').each(function (i, elem) {
    //console.log(elem);
    var text = '(' + $(elem).val() + ') ' + $(elem).text();
    $(elem).text(text);
});

$('.invoke_session_sync').click(function (e) {
    e.preventDefault();
    var $btn = this;
    var zone = $($btn).data('zone');
    var url = '/invoke-sessionsync/' + zone;
    var statusSelector = '#session_sync_result';
    var $statusObj = $(statusSelector);

    $.ajax({
        type: 'GET',
        url: url,
        dataType: 'json',
        success: function (result) {
            var status = "Update success: " + result.success + ", Message: " + result.message;
            $statusObj.text(status);

            if (result.success) {
                $statusObj.removeClass('alert-danger').removeClass('alert-warning').addClass('alert-success');
            } else {
                $statusObj.removeClass('alert-danger').removeClass('alert-success').addClass('alert-warning');
            }

            if ($statusObj.is(':hidden')) {
                $statusObj.removeClass('hidden');
                $statusObj.slideDown();
            }
        }
    }).fail(function (err) {
        var status = "Could not send sync request";
        $statusObj.text(status);
        $statusObj.removeClass('alert-success').addClass('alert-error');
        if ($statusObj.is(":hidden")) {
            $statusObj.removeClass('hidden');
            $statusObj.slideDown();
        }
    });
});

$('.invoke_serverstatuscheck').click(function (e) {
    e.preventDefault();
    var $btn = this;
    var url = '/invoke-serverstatuscheck/';
    var statusSelector = '#status_check_result';
    var $statusObj = $(statusSelector);

    $.ajax({
        type: 'GET',
        url: url,
        dataType: 'json',
        success: function (result) {
            var status = "Status check success: " + result.success + ", Message: " + result.message;
            $statusObj.text(status);

            if (result.success) {
                $statusObj.addClass('alert-success').removeClass('alert-warning').removeClass('alert-warning');
            } else {
                $statusObj.addClass('alert-warning').removeClass('alert-success');
            }


            if ($statusObj.is(':hidden')) {
                $statusObj.removeClass('hidden');
                $statusObj.slideDown();
            }
        }
    }).fail(function (err) {
        var status = "Could not send status check request";
        $statusObj.text(status);
        $statusObj.removeClass('alert-success').addClass('alert-error');
        if ($statusObj.is(":hidden")) {
            $statusObj.removeClass('hidden');
            $statusObj.slideDown();
        }
    });
});

$('.invoke_autologout').click(function (e) {
    e.preventDefault();
    var $btn = this;
    var url = '/invoke-autologout/';
    var statusSelector = '#autologout_result';
    var $statusObj = $(statusSelector);

    $.ajax({
        type: 'GET',
        url: url,
        dataType: 'json',
        success: function (result) {
            var status = "Autologout success: " + result.success + ", Message: " + result.message;
            $statusObj.text(status);

            if (result.success) {
                $statusObj.addClass('alert-success').removeClass('alert-warning');
            } else {
                $statusObj.addClass('alert-warning').removeClass('alert-success');
            }


            if ($statusObj.is(':hidden')) {
                $statusObj.removeClass('hidden');
                $statusObj.slideDown();
            }
        }
    }).fail(function (err) {
        var status = "Could not send auto logout request.";
        $statusObj.text(status);
        $statusObj.removeClass('alert-success').addClass('alert-error');
        if ($statusObj.is(":hidden")) {
            $statusObj.removeClass('hidden');
            $statusObj.slideDown();
        } 
    });
});
