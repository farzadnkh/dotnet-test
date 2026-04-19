$(document).ready(function () {
    const $ipTagContainer = $('#ipTagContainer');
    const $ipInput = $('#ipWhitelistInputCustom');
    const $ipHiddenInput = $('#ipWhitelistHiddenInput');
    const $ipValidationFeedback = $('#ipValidationFeedback');
    // Removed 'selectedIps' array as it was redundant with direct DOM querying

    const ipPattern = /^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/;

    function updateHiddenInput() {
        const ips = [];
        $ipTagContainer.find('.tag span').each(function () {
            ips.push($(this).text());
        });
        $ipHiddenInput.val(ips.join(','));
        validateInputState();
    }

    // Function to add a tag
    // Added a parameter 'skipClearInput' to control input clearing for initial population
    function addTag(ip, skipClearInput = false) {
        ip = ip.trim();
        if (ip === '') {
            validateInputState();
            return;
        }

        // Check for duplicates
        const existingIps = $ipTagContainer.find('.tag span').map(function () {
            return $(this).text();
        }).get();

        if (existingIps.includes(ip)) {
            if (!skipClearInput) { // Only clear if not in initial population
                $ipInput.val('');
            }
            validateInputState();
            return;
        }

        if (ipPattern.test(ip)) {
            const $tag = $(`
                        <span class="tag">
                            <span>${ip}</span>
                            <button type="button" class="tag-remove">&times;</button>
                        </span>
                    `);
            $ipTagContainer.prepend($tag); // Prepend to keep input at the end

            if (!skipClearInput) { // Only clear if not in initial population
                $ipInput.val(''); // Clear the input field
            }
            updateHiddenInput();
            validateInputState(); // Clear any input error
        } else {
            // Apply visual error state to the input wrapper
            $ipTagContainer.addClass('is-invalid-input');
        }
    }

    // Function to validate the current input field's content and apply/remove error class
    function validateInputState() {
        const currentInputValue = $ipInput.val().trim();
        if (currentInputValue !== '' && !ipPattern.test(currentInputValue)) {
            $ipTagContainer.addClass('is-invalid-input');
        } else {
            $ipTagContainer.removeClass('is-invalid-input');
        }
    }

    // Handle input key presses
    $ipInput.on('keydown', function (e) {
        // Add tag on Enter or Spacebar
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault(); // Prevent form submission or space from being typed in input
            addTag($(this).val());
        }
    });

    // Handle input blur (when user clicks outside)
    $ipInput.on('blur', function () {
        addTag($(this).val());
    });

    // Handle tag removal
    $ipTagContainer.on('click', '.tag-remove', function () {
        $(this).closest('.tag').remove();
        updateHiddenInput();
    });

    // Initial population from hidden input value (if any, e.g., from server)
    function populateTagsFromHidden() {
        const initialValue = $ipHiddenInput.val();
        if (initialValue) { // Check if there's an initial value
            const initialIps = initialValue.split(',').filter(ip => ip.trim() !== '');
            initialIps.forEach(ip => addTag(ip, true)); // Pass 'true' to skip clearing input
        }
        // AFTER all tags are added, then clear the input field once
        $ipInput.val('');
        updateHiddenInput(); // Ensure hidden input is consistent after initial population
    }

    // Live validation as user types in the current input
    $ipInput.on('input', validateInputState);

    // Run initial population on load
    populateTagsFromHidden();
    validateInputState(); // Ensure initial state is correct for empty or pre-filled
});