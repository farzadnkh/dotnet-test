$(document).ready(function () {
    $('.save-row-btn').click(function (e) {
        const url = '/ManualRating/Save';
        const loaderOverlay = $("#loaderOverlay");
        loaderOverlay.css("display", "flex");
        var button = $(this);
        var tradingPair = button.data('trading-pair');
        var tradingPairId = button.data('trading-pair-id');
        var index = button.data('index');
        var form = $('#ratingForm');
        var data = {
            pairId: tradingPairId,
            tradingPair: tradingPair,
            newPrice: $(`#ManualRatingResponses_${index}__Price`).val(),
            oldPrice: $(`.oldPrice_${index}`).val()
        };

        $.ajax({
            url: url,
            type: 'POST',
            data: JSON.stringify(data),
            contentType: "application/json",
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (data) {
                loaderOverlay.hide();
                if (data.isSuccess) {
                    showSuccessMessage("Successfully Done.");
                } else {
                    showErrorMessage(data.errors);
                }
            },
            error: function (xhr) {
                loaderOverlay.hide();
                let errorText = xhr.responseText || "An unexpected error occurred.";
                showErrorMessage(errorText);
            }
        });
    });


    $('input[id*="ManualRatingResponses_"][id$="__Price"]').on('input', function () {
        const input = $(this);
        const id = input.attr('id');
        const index = id.match(/\d+/)[0];

        const price = parseFloat(input.val());
        if (isNaN(price) || price <= 0) return;

        const upperPct = parseFloat($(`#ManualRatingResponses_${index}__UpperLimitPercentage`).val()) || 0;
        const lowerPct = parseFloat($(`#ManualRatingResponses_${index}__LowerLimitPercentage`).val()) || 0;

        const upperLimit = price + (price * upperPct / 100);
        const lowerLimit = price - (price * lowerPct / 100);

        $(`#ManualRatingResponses_${index}__UpperLimit`).val(upperLimit.toFixed(4));
        $(`#ManualRatingResponses_${index}__LowerLimit`).val(lowerLimit.toFixed(4));
    });
});