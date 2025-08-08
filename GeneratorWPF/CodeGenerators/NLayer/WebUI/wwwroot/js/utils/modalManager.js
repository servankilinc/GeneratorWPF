const ModalManager =
{
    buttonKinds: {
        save: 'save',
        update: 'update',
        delete: 'delete',
        confirmation: 'confirmation',
        cancel: 'cancel',
    },
    defaultButtonsProps: {
        save: {
            text: 'Kaydet',
            name: 'btn-primary',
            icon: 'fa-regular fa-circle-check',
        },
        update: {
            text: 'Güncelle',
            name: 'btn-warning',
            icon: 'fa-regular fa-pen-to-square',
        },
        delete: {
            text: 'Sil',
            name: 'btn-danger',
            icon: 'fa-solid fa-trash-can',
        },
        confirmation: {
            text: 'Onay',
            name: 'btn-success',
            icon: 'fa-solid fa-check-circle',
        },
        cancel: {
            text: 'İptal',
            name: 'btn-secondary',
            icon: 'fa-solid fa-arrow-left',
        }
    },


    CreateModal: function ({
        id = null,
        title = null,
        showHeader = true,
        innerHtml = '',
        buttons = [],
        modalSize = 'md',
        btnCancelEnable = true,
        btnCancelSize = 'md',
        btnCancelText = 'Kapat',
        backdrop = true,
        tabindex = -1,
        onBeforeShow = null,
        onAfterShow = null,
        onBeforeClose = null,
        onAfterClose = null,
    }) {

        if (id == null) id = GenerateId();

        $(`#${id}`).remove();

        let modalElement = GenerateModalDOM(id, tabindex, backdrop, modalSize, showHeader, title, innerHtml, btnCancelEnable, btnCancelSize, btnCancelText, buttons);

        $('body').append(modalElement);

        let modal = $(`#${id}`);

        // event.preventDefault() davranış iptal edilebilir;
        modal.on('show.bs.modal', (event) => {
            if (onBeforeShow != null && typeof onBeforeShow === 'function') onBeforeShow(event);
        })

        modal.on('shown.bs.modal', (event) => {
            if (onAfterShow != null && typeof onAfterShow === 'function') onAfterShow(event);

        })

        modal.on('hide.bs.modal', (event) => {
            if (onBeforeClose != null && typeof onBeforeClose === 'function') onBeforeClose(event);
        })

        modal.on('hidden.bs.modal', (event) => {
            if (onAfterClose != null && typeof onAfterClose === 'function') onAfterClose(event);
            modal.remove();
        })

        // ****** select2 Congifrations If Exist ******
        AutoInitSelect2(modal); 
        // ****** FlatPicker Congifrations If Exist ******
        AutoInitFlatPicker(modal);
        // ****** FlatPicker Congifrations If Exist ******
        AutoInitDatePicker(modal);

        return {
            element: modal,
            show: () => $(modal).modal("show"),
            close: () => $(modal).modal("hide"),
            remove: () => $(modal).remove()
        };
    },

    DeleteModal: function ({ id = null, onClick = null, }) {
        return this.CreateModal({
            id: id,
            innerHtml: `<div class="d-flex flex-column justify-content-center">
                            <i class="fa-solid fa-triangle-exclamation text-warning opacity-50" style="font-size: 2.5rem;"></i>
                            <h4 class="text-center fw-normal">Silmek İstediğinize Emin misiniz?</h4>
                        </div>`,
            buttons: [
                ModalManager.Button(
                    {
                        kind: this.buttonKinds.delete,
                        text: 'Sil',
                        onClick: onClick,
                        size: 'sm'
                    }
                )
            ],
            modalSize: 'sm',
            btnCancelSize: 'sm',
            btnCancelText: 'İptal',
            showHeader: false
        })
    },

    Button: function ({
        kind = null,
        id = null,
        className = '',
        type = 'button',
        attributes = {},
        disable = false,
        size = 'md',
        onClick = null,
        text = null,
        icon = null,
    }) {
        let btnObject = {
            id: id,
            className: className,
            type: type,
            attributes: attributes,
            disable: disable,
            size: size,
            onClick: onClick,
            text: text,
            icon: icon,
        };

        let defaultProps = this.defaultButtonsProps[kind];

        btnObject.id = btnObject.id || GenerateId();
        btnObject.text = btnObject.text || defaultProps.text;
        btnObject.name = btnObject.name || defaultProps.name;
        btnObject.icon = btnObject.icon || defaultProps.icon;

        return btnObject;
    }
}

function GenerateModalButtonDOM(btnObject, modalReferance)
{
    // **** Generation Process **** 
    $(`#${btnObject.id}`).remove();

    // btn-init
    const btnElement = document.createElement('button');
    btnElement.type = btnObject.type;
    btnElement.id = btnObject.id;
    btnElement.className = `btn ${btnObject.name} btn-${btnObject.size} my-1 ${btnObject.className || ''}`;
    Object.entries(btnObject.attributes).forEach(([key, val]) => btnElement.setAttribute(key, val));
    btnElement.disabled = btnObject.disable;

    btnElement.modalElement = modalReferance;

    // btn-onclick
    if (btnObject.onClick != null && typeof btnObject.onClick === 'function') {
        btnElement.onclick = async function (e) {
            const button = this;
            const modal = this.modalElement;
            const form = $(modal).find("form");

            let original = '';
            let dynamicContent = $(button).find(".dynamic-content");

            if (dynamicContent.length == 1) {
                original = dynamicContent.html();
                dynamicContent.html('<i class="fa-solid fa-spinner me-2"></i> Bekleyiniz');
            }

            $(button).prop("disabled", true);

            try {
                await btnObject.onClick(e, modal, form); 
            }
            finally {
                $(button).prop("disabled", false);

                if (dynamicContent.length == 1) {
                    dynamicContent.html(original);
                }
            }
        };
    }

    // btn-DynamicContent
    const dynamicContentElement = document.createElement('span');
    dynamicContentElement.className = `dynamic-content`;

    // btn-icon
    if (btnObject.icon != null) {
        const iconElement = document.createElement('i');
        iconElement.className = `${btnObject.icon} ${btnObject.text.length > 0 ? 'me-2' : ''}`;
        dynamicContentElement.append(iconElement);
    }

    // btn-text
    const textNode = document.createTextNode(btnObject.text);
    dynamicContentElement.appendChild(textNode);

    btnElement.append(dynamicContentElement);

    return btnElement;
}

function GenerateModalDOM(id, tabindex, backdrop, modalSize, showHeader, title, innerHtml, btnCancelEnable, btnCancelSize, btnCancelText, buttons) {

    // modal
    const modal = document.createElement('div');
    modal.className = 'modal fade';
    modal.id = id;
    modal.tabIndex = tabindex;
    modal.setAttribute('aria-labelledby', `${id}Label`);
    modal.setAttribute('aria-hidden', 'true');
    if (backdrop) modal.setAttribute('data-bs-backdrop', 'static');

    // modal-dialog
    const dialog = document.createElement('div');
    dialog.className = `modal-dialog modal-dialog-scrollable modal-${modalSize}`;
    dialog.setAttribute('role', 'document');

    // modal-content
    const content = document.createElement('div');
    content.className = 'modal-content';

    // modal-header (opsiyonel)
    if (showHeader) {
        const header = document.createElement('div');
        header.className = 'modal-header';

        const h5 = document.createElement('h5');
        h5.className = 'modal-title me-3 text-weight-light';
        h5.id = `${id}Label`;
        h5.innerText = title || '';

        const closeBtn = document.createElement('button');
        closeBtn.type = 'button';
        closeBtn.className = 'btn-close';
        closeBtn.setAttribute('data-bs-dismiss', 'modal');
        closeBtn.setAttribute('aria-label', 'Close');

        header.appendChild(h5);
        header.appendChild(closeBtn);
        content.appendChild(header);
    }

    // modal-body
    const body = document.createElement('div');
    body.className = 'modal-body bg-lighter';

    // body-content
    const bodyContent = document.createElement('div');
    bodyContent.className = 'bg-lightest shadow-sm rounded-3 py-5';

    if (typeof innerHtml === 'string') {
        bodyContent.innerHTML = innerHtml;
    }
    else if (innerHtml instanceof HTMLElement) {
        bodyContent.appendChild(innerHtml);
    }

    body.appendChild(bodyContent);

    // modal-footer
    const footer = document.createElement('div');
    footer.className = 'modal-footer';

    if (btnCancelEnable) {
        const cancelBtn = document.createElement('button');
        cancelBtn.type = 'button';
        cancelBtn.className = `btn btn-light text-secondary btn-${btnCancelSize}`;
        cancelBtn.setAttribute('data-bs-dismiss', 'modal');
        cancelBtn.innerHTML = `<i class="fa-solid fa-xmark me-2"></i>${btnCancelText || 'Kapat'}`;
        footer.appendChild(cancelBtn);
    }

    // Ekstra buttons 
    if (buttons != null) {
        if (Array.isArray(buttons))
        {
            const generatedBtnList = buttons.map(btnObject => GenerateModalButtonDOM(btnObject, modal));
            if (Array.isArray(generatedBtnList)) {
                generatedBtnList.forEach(btn => {
                    if (btn instanceof HTMLElement) {
                        btn.setAttribute('modal-id', id);
                        footer.appendChild(btn);
                    }
                });
            }
        }
        else
        {
            const generatedBtn = buttons.map(btnObject => GenerateModalButtonDOM(btnObject, modal));

            if (generatedBtn instanceof HTMLElement) {
                generatedBtn.setAttribute('modal-id', id);
                footer.appendChild(generatedBtn);
            }
        }
    }

    content.appendChild(body);
    content.appendChild(footer);
    dialog.appendChild(content);
    modal.appendChild(dialog);

    return modal;
}
