$(document).ready(function () {
	function validateSpreadLimits() {
		const lower = parseFloat($('#lowerLimitInput').val());
		const upper = parseFloat($('#upperLimitInput').val());
		const isEnabled = $('#spreadEnabledCheck').is(':checked');

		$('#lowerLimitError').text('');
		$('#upperLimitError').text('');

		if (isEnabled) {
			if (!isNaN(lower) && !isNaN(upper) && lower > upper) {
				$('#lowerLimitError').text('Lower limit must be less than or equal to upper limit.');
				$('#upperLimitError').text('Upper limit must be greater than or equal to lower limit.');
			}
			if (!isNaN(lower) && lower < 0) {
				$('#lowerLimitError').text('Lower limit must be greater than or equal to 0.');
			}
			if (!isNaN(upper) && upper < 0) {
				$('#upperLimitError').text('upper limit must be greater than or equal to 0.');
			}
		}
	};

	$('#spreadEnabledCheck').change(function () {
		const enabled = $(this).is(':checked');

		if (!enabled)
			$('#lowerLimitInput, #upperLimitInput').val('');

		$('#lowerLimitInput, #upperLimitInput').prop('disabled', !enabled);
		validateSpreadLimits();
	});

	$('#lowerLimitInput, #upperLimitInput').on('input', function () {
		validateSpreadLimits();
	});

	$('#spreadEnabledCheck').trigger('change');
});