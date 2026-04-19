window.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('input').forEach(input => {
        input.setAttribute('autocomplete', 'off');
    });
});