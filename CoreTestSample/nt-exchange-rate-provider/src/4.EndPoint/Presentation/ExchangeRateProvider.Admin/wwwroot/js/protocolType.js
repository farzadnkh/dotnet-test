$(document).ready(function () {
    const $providerType = $('select[name="Response.Type"]');
    const $protocolTypeContainer = $('select[name="Response.ProtocolType"]').closest('.mb-3');
    const $protocolTypeSelect = $('select[name="Response.ProtocolType"]');
    const $credentialsSection = $('#credentials-section');
    const initialProtocolType = '@((int)Model.Response.ProtocolType)';
    const initialProviderType = '@((int)Model.Response.Type)';

    updateProtocolTypeField(initialProviderType, initialProtocolType);

    $providerType.change(function () {
        updateProtocolTypeField($(this).val());
    });

    function updateProtocolTypeField(providerType, selectedProtocol = null) {
        $protocolTypeContainer.find('input').remove();
        $protocolTypeSelect.show().prop('disabled', false);
        $credentialsSection.hide();
        $('.provider-credentials').hide();

        if (providerType === "3") {
            loadProtocolTypes(selectedProtocol || initialProtocolType);
            $credentialsSection.show();
            $('#CryptoCompare').show();
        }
        else if (providerType === "4") {
            const protocolName = getProtocolTypeName(2);
            $protocolTypeSelect.hide();
            $protocolTypeContainer.html(`
                <label class="form-label">Protocol Type:</label>
                <input type="text" class="form-control" 
                       value="${protocolName}" disabled readonly />
                <input type="hidden" name="Response.ProtocolType" value="2" />
            `);
            $credentialsSection.show();
            $('#Xe').show();
        }
    }

    function loadProtocolTypes(selectedValue) {
        $.ajax({
            url: 'GetProtocolTypes',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                renderProtocolOptions(data, selectedValue);
            },
            error: function () {
                $protocolTypeContainer.html(`
                    <label class="form-label">Protocol Type:</label>
                    <select name="Response.ProtocolType" class="form-control">
                        <option value="">Failed to load protocols</option>
                    </select>
                `);
            }
        });
    }

    function renderProtocolOptions(protocolTypes, selectedValue) {
        let options = '<label class="form-label">Protocol Type:</label><select name="Response.ProtocolType" class="form-control">';

        protocolTypes.forEach(function (protocol) {
            const isSelected = protocol.value === selectedValue ? 'selected' : '';
            options += `<option value="${protocol.value}" ${isSelected}>${protocol.text}</option>`;
        });

        options += '</select>';
        $protocolTypeContainer.html(options);
    }

    function getProtocolTypeName(value) {
        const protocols = {
            "2": "Api call",
            "3": "Web Socket"
        };
        return protocols[value] || '';
    }
});