function showErrorMessage(errors) {
    if (!Array.isArray(errors) || errors.length === 0) return;

    const errorList = errors.map(e => `<p style="margin: 4px 0;">${e}</p>`).join("");

    Swal.fire({
        icon: 'error',
        title: 'Errors',
        html: `<div>${errorList}</div>`,
        confirmButtonText: 'Ok',
        customClass: {
            popup: 'swal2-popup-ar'
        }
    });
}

function showSuccessMessage(strMessage) {
    
    Swal.fire({
        icon: 'success',
        title: 'Success',
        html: `<div>${strMessage}</div>`,
        confirmButtonText: 'Ok',
        customClass: {
            popup: 'swal2-popup-ar'
        }
    });
}


