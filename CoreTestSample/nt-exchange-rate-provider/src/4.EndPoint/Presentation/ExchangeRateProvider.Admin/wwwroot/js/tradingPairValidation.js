$(document).ready(function () {

    var isValid = true;

    $("form").submit(function (e) {
        if (!isValid) {
            e.preventDefault();
            showErrorMessage("Market and Currency cannot be the same");
        }
    });

    $('#Response_MarketId, #Response_CurrencyId').change(function () {

        isValid = true;
        $(this).removeClass('is-invalid');
        $('.market-currency-error').remove();

        var marketId = $("#Response_MarketId option:selected").text();
        var currencyId = $("#Response_CurrencyId option:selected").text();

        $('#Response_MarketId').removeClass('is-invalid');
        $('#Response_CurrencyId').removeClass('is-invalid');
        $('.market-currency-error').remove();

        if (marketId && currencyId && marketId === currencyId) {

            $('#Response_MarketId').addClass('is-invalid');
            $('#Response_CurrencyId').addClass('is-invalid');

            $('<span class="text-danger market-currency-error">Market and Currency cannot be the same</span>')
                .insertAfter('#Response_CurrencyId');

            isValid = false;
        }
    });
});