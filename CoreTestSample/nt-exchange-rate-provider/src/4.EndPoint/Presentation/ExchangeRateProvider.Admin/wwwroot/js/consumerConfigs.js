function generateClientSecret() {

    var consumerId = $("#ConsumerId").val();
    var url = `/Consumer/GenerateClientSecret?consumerId=${consumerId}`;
    $.ajax({
        url: url,
        type: 'Get',
  /*      data: JSON.stringify({ consumerId: consumerId }),*/
        contentType: 'application/json',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            $('#clientSecret').val(response.clientSecret);
            showSuccessMessage("New client secret generated successfully");
            $('#saveWarning').show();
        },
        error: function (xhr, status, error) {
            showErrorMessage("Failed to generate new secret: " + (xhr.responseJSON?.message || error));
        },
        complete: function () {
        }
    });
}

function copyToClipboard(inputId) {
    var trageInput = $(`#${inputId}`);
    navigator.clipboard.writeText(trageInput.val()).then(() => {

        const notification = $('#notification');
        notification.addClass('show');
        setTimeout(() => {
            notification.removeClass('show');
        }, 2000);

    });
};


function handleSave() {
    showSuccessMessage("New Credentials Saved successfully");
}

function showSaveWarning() {
    $('#saveWarning').show();
}


$(document).on('ready', () => {
    var isEditMode = $("#isEditMode").val();
    if (isEditMode != 'false')
        $('#saveWarning').show();
});