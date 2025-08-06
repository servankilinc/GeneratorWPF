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
        title = 'Sil',
        requestUrl = '',
        requestData = undefined,
        pageTable = undefined,
        disable = false
    }) {
        return DatatableManager.RowButton({
            kind: DatatableManager.buttonKinds.delete,
            onClick: async () => {
                ModalManager.CreateModal({
                    title: title,
                    innerHtml: `<div class="d-flex flex-column justify-content-center"><i class="fa-solid fa-triangle-exclamation text-warning opacity-50" style="font-size: 2.5rem;"></i><h4 class="text-center fw-normal">Silmek İstediğinize Emin misiniz?</h4></div>`,
                    modalSize: 'sm',
                    btnCancelSize: 'sm',
                    showHeader: title == null || title.length == 0 ? false : true,
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