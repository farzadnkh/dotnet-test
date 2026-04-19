export class DropdownComponent {
    constructor(config) {
        this.config = {
            items: [],
            selectedItems: [],
            toggleBtnId: '',
            listId: '',
            arrowId: '',
            containerId: '',
            displayId: '',
            noTextId: '',
            loadingTextId: '',
            hiddenInputId: '',
            selectAllId: '',
            label: '',
            ...config
        };
        this.init();
    }

    init() {
        try {
            const hiddenInput = document.querySelector(`#${this.config.hiddenInputId}`);
            if (hiddenInput) {
                const initialItems = JSON.parse(hiddenInput.dataset.initial || '[]');
                if (Array.isArray(initialItems)) {
                    this.config.selectedItems = initialItems.map(item => {
                        if (typeof item === 'object' && item !== null && 'value' in item) {
                            return item.value;
                        }
                        return item;
                    });
                }
            }
        } catch (e) {
            console.error(`Error parsing initial data for ${this.config.label}:`, e);
        }
        this.renderDropdownItems();
        this.renderSelectedItemsDisplay();
        this.updateHiddenInput();
        this.bindEvents();
    }

    renderDropdownItems() {
        const list = document.querySelector(`#${this.config.listId}`);
        if (!list) return;
        list.innerHTML = '';

        if (!this.config.items.length) {
            list.innerHTML = `<p class="p-4 text-gray-500 font-semibold">No ${this.config.label}s available.</p>`;
            return;
        }

        const selectAllLabel = document.createElement('label');
        selectAllLabel.className = 'flex items-center px-4 py-2 hover:bg-gray-100 cursor-pointer transition-colors duration-200 border-b border-gray-200';
        selectAllLabel.innerHTML = `
            <input type="checkbox" id="${this.config.selectAllId}" class="form-checkbox h-4 w-4 text-blue-600 rounded focus:ring-blue-500">
            <span class="ml-3 text-gray-900 font-semibold">Select All</span>
        `;
        list.appendChild(selectAllLabel);

        this.config.items.forEach(item => {
            const isChecked = this.config.selectedItems.includes(item.value);
            const label = document.createElement('label');
            label.className = 'flex items-center px-4 py-2 hover:bg-blue-50 cursor-pointer transition-colors duration-200 rounded';
            label.innerHTML = `
                <input type="checkbox" value="${encodeURIComponent(item.value)}" ${isChecked ? 'checked' : ''} class="form-checkbox h-4 w-4 text-blue-600 rounded focus:ring-blue-500">
                <span class="ml-3 text-gray-800 capitalize">${item.text}</span>
            `;
            list.appendChild(label);
        });

        const selectAll = document.querySelector(`#${this.config.selectAllId}`);
        if (selectAll) {
            selectAll.checked = this.config.items.every(item => this.config.selectedItems.includes(item.value));
        }
    }

    renderSelectedItemsDisplay() {
        const display = document.querySelector(`#${this.config.displayId}`);
        const noText = document.querySelector(`#${this.config.noTextId}`);
        if (!display || !noText) return;

        Array.from(display.children).forEach(child => {
            if (child.id !== this.config.noTextId) display.removeChild(child);
        });

        if (!this.config.selectedItems.length) {
            noText.classList.remove('hidden');
            display.appendChild(noText);
        } else {
            noText.classList.add('hidden');
            this.config.selectedItems.forEach(value => {
                const text = this.getTextByValue(value);
                const span = document.createElement('span');
                span.className = 'bg-blue-100 text-blue-800 text-sm font-medium px-3 py-1 rounded-full shadow-sm flex items-center';
                span.textContent = text;

                const closeBtn = document.createElement('span');
                closeBtn.className = 'selected-tag-close';
                closeBtn.innerHTML = '×';
                closeBtn.dataset.value = value;

                span.appendChild(closeBtn);
                display.appendChild(span);
            });
        }
    }

    getTextByValue(value) {
        const item = this.config.items.find(i => i.value === value);
        return item ? item.text : value;
    }

    updateHiddenInput() {
        const hiddenInput = document.querySelector(`#${this.config.hiddenInputId}`);
        if (hiddenInput) {
            hiddenInput.value = JSON.stringify(this.config.selectedItems);
        }
    }

    toggleDropdown() {
        const list = document.querySelector(`#${this.config.listId}`);
        const arrow = document.querySelector(`#${this.config.arrowId}`);
        if (!list || !arrow) return;

        list.classList.toggle('hidden');
        arrow.classList.toggle('rotate-180');
    }

    bindEvents() {
        const toggleBtn = document.querySelector(`#${this.config.toggleBtnId}`);
        const list = document.querySelector(`#${this.config.listId}`);
        const selectAll = document.querySelector(`#${this.config.selectAllId}`);
        const display = document.querySelector(`#${this.config.displayId}`);

        if (toggleBtn) {
            toggleBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                this.toggleDropdown();
            });
        }

        if (list) {
            list.addEventListener('change', (e) => {
                const checkbox = e.target;
                if (checkbox.type !== 'checkbox') return;

                const value = decodeURIComponent(checkbox.value);
                if (checkbox.id === this.config.selectAllId) {
                    this.config.selectedItems = checkbox.checked ? this.config.items.map(i => i.value) : [];
                } else {
                    if (checkbox.checked) {
                        if (!this.config.selectedItems.includes(value)) {
                            this.config.selectedItems.push(value);
                        }
                    } else {
                        this.config.selectedItems = this.config.selectedItems.filter(i => i !== value);
                    }
                }

                this.renderDropdownItems();
                this.renderSelectedItemsDisplay();
                this.updateHiddenInput();
            });
        }

        if (display) {
            display.addEventListener('click', (e) => {
                if (e.target.classList.contains('selected-tag-close')) {
                    const value = e.target.dataset.value;
                    this.config.selectedItems = this.config.selectedItems.filter(i => i !== value);
                    this.renderDropdownItems();
                    this.renderSelectedItemsDisplay();
                    this.updateHiddenInput();
                }
            });
        }
    }
}
