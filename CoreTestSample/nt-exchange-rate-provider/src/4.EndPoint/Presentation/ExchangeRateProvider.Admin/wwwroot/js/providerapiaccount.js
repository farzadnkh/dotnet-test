$(document).on("ready", function () {

	var selectedProvider = $("#Response_Type");

	if (selectedProvider.val() == "3" || selectedProvider.val().toUpperCase() == 'CRYPTOCOMPARE') {
		$("#credentials-section").show();
		$("#CryptoCompare").show();
	}
	
	if (selectedProvider.val() == "4" || selectedProvider.val().toUpperCase() == "XE") {
		$("#credentials-section").show();
		$("#Xe").show();
	}
	
	selectedProvider.on("change", (function () {
		$("#credentials-section").hide();
		$("#CryptoCompare").hide();
		$("#Xe").hide();

		var provider = $(this).val();
		if (provider == "3") {
			$("#credentials-section").show();
			$("#CryptoCompare").show();
		}
		else if (provider == "4") {
			$("#credentials-section").show();
			$("#Xe").show();
		}
		else {
			$("#CryptoCompare").hide();
			$("#credentials-section").hide();
		}
	}));
});