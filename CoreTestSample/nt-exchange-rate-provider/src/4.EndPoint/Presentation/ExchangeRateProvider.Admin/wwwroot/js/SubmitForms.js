$(document).ready(() => {
    $('form').on('submit', (e) => {
        e.preventDefault();

        const form = $(e.currentTarget);
        const url = form.attr('action');

        $.ajax({
            url: url,
            type: 'POST',
            data: new FormData(form.get(0)),
            processData: false,
            contentType: false,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (data) {
                if (data.isSuccess) {
                    showSuccessMessage("Successfully Done.");
                    if (data.data && data.data.redirectUrl) {
                        window.location.href = data.data.redirectUrl;
                    }
                } else {
                    showErrorMessage(data.errors);
                }
            },
            error: function (xhr) {
                let errorText = xhr.responseText || "An unexpected error occurred.";
                showErrorMessage(errorText);
            }
        });
    });
});