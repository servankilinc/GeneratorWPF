const UIManager = {
    InsertModal: function ({
        title = 'Ekle',
        formGetterUrl = '',
        e_btn = undefined,
        pageTable = undefined,
        disable = false
    }) {
        return RequestManager.Get({
            path: formGetterUrl,
            dataType: 'text',
            showToastrSuccess: false,
            buttonElement: e_btn,
            onSuccess: (formHtml) => {
                ModalManager.CreateModal({
                    title: title,
                    innerHtml: formHtml,
                    modalSize: "xl",
                    buttons: [
                        ModalManager.Button({
                            kind: ModalManager.buttonKinds.save,
                            disable: disable,
                            onClick: (e_btn_modal, e_modal, e_form) => {
                                RequestManager.HandleRequest({
                                    type: e_form.attr("method"),
                                    path: e_form.attr("action"),
                                    requestData: e_form.serializeArray(),
                                    buttonElement: e_btn_modal,
                                    onSuccess: () => {
                                        $(e_modal).modal("hide");
                                        if (pageTable != undefined) pageTable.reload();
                                    }
                                })
                            }
                        })
                    ]
                }).show();
            }
        })
    },

    UpdateButtonTable: function ({
        title = 'Düzenle',
        formGetterUrl = '',
        requestData = undefined,
        pageTable = undefined,
        disable = false
    }) {
        return DatatableManager.RowButton({
            kind: DatatableManager.buttonKinds.update,
            onClick: async (e_btn) => {
                await RequestManager.Get({
                    path: formGetterUrl,
                    requestData: requestData,
                    dataType: 'text',
                    showToastrSuccess: false,
                    buttonElement: e_btn.currentTarget,
                    onSuccess: (formHtml) => {
                        ModalManager.CreateModal({
                            title: title,
                            innerHtml: formHtml,
                            modalSize: "xl",
                            buttons: [
                                ModalManager.Button({
                                    kind: ModalManager.buttonKinds.update,
                                    disable: disable,
                                    onClick: (e_btn_modal, e_modal, e_form) => {
                                        RequestManager.HandleRequest({
                                            type: e_form.attr("method"),
                                            path: e_form.attr("action"),
                                            requestData: e_form.serializeArray(),
                                            buttonElement: e_btn_modal,
                                            onSuccess: () => {
                                                $(e_modal).modal("hide");
                                                if (pageTable != undefined) pageTable.reload();
                                            }
                                        })
                                    }
                                })
                            ]
                        }).show();
                    }
                })
            }
        });
    },
    DeleteButtonTable: function ({
        text = 'Are you sure you want to delete?',
        requestUrl = '',
        requestData = undefined,
        pageTable = undefined,
        disable = false
    }) {
        return DatatableManager.RowButton({
            kind: DatatableManager.buttonKinds.delete,
            disable: disable,
            onClick: async () => {
                ModalManager.CreateModal({
                    innerHtml: `<div class="d-flex flex-column justify-content-center align-items-center"><i class="fa-solid fa-triangle-exclamation text-warning opacity-50" style="font-size: 2.5rem;"></i><h5 class="text-center fw-normal mt-4">${text}</h4></div>`,
                    modalSize: 'sm',
                    btnCancelSize: 'sm',
                    showHeader: false,
                    buttons: [
                        ModalManager.Button(
                            {
                                kind: ModalManager.buttonKinds.delete,
                                disable: disable,
                                onClick: (e_btn, e_mdl) => {
                                    RequestManager.Delete({
                                        path: requestUrl,
                                        requestData: requestData,
                                        buttonElement: e_btn,
                                        onSuccess: () => {
                                            $(e_mdl).modal("hide");
                                            if (pageTable != undefined) pageTable.reload();
                                        }
                                    })
                                },
                                size: 'sm'
                            }
                        )
                    ],
                }).show();
            }
        });
    }
};