using GeneratorWPF.Extensions;
using GeneratorWPF.Models;
using GeneratorWPF.Models.Enums;
using GeneratorWPF.Repository;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GeneratorWPF.CodeGenerators.NLayer.WebUI;

public class ViewGenerator
{
    private readonly RelationRepository _relationRepository;
    private readonly DtoRepository _dtoRepository;
    private readonly FieldRepository _fieldRepository;
    public ViewGenerator(AppSetting appSetting)
    {
        _relationRepository = new();
        _dtoRepository = new();
        _fieldRepository = new();
    }

    public string GenerateIndexPage(Entity entity)
    {
        List<Field> fieldList = _fieldRepository.GetAll(filter: f => f.EntityId == entity.Id, include: i => i.Include(x => x.FieldType));

        List<Field> filterableFields = fieldList.Where(f => f.Filterable).ToList();

        Dto? reportDto = entity.ReportDtoId != default ? _dtoRepository.Get(f => f.Id == entity.ReportDtoId, include: i => i.Include(x => x.DtoFields).ThenInclude(x => x.SourceField)) : default;
        bool isThereReportDto = reportDto != default;

        List<ModelFieldInput> modelFilterFields = new List<ModelFieldInput>();
        if (filterableFields.Any()) modelFilterFields = CreateFilterInputsModel(entity, filterableFields);

        StringBuilder sb = new StringBuilder();

        sb.Append($@"
@using WebUI.Models.ViewModels.{entity.Name}_
@model {entity.Name}ViewModel
@{{
    ViewData[""Title""] = ""{entity.Name.Pluralize()}"";
}}");

        // ########## HTML ##########
        sb.AppendLine("\r<div class=\"card\">");

        // ******** HEADER ********
        sb.AppendLine("\t<div class=\"card-header\">");
        if (filterableFields.Any())
        {
            sb.Append(CreateFilterForm(entity, modelFilterFields));
        }
        sb.AppendLine("\t</div>");


        // ******** BODY ********
        sb.AppendLine("\t<div class=\"card-body\">");

        if (isThereReportDto)
        {
            sb.Append(CreateTableByDto(entity, reportDto!));
        }
        else
        {
            sb.Append(CreateTableByEntity(entity, fieldList));
        }
        sb.AppendLine("\t</div>");


        sb.AppendLine("</div>"); 

         
        // ########## SCRIPT ##########
        string datatableMethod = isThereReportDto ?
            CreateTableInitMethodByDto(entity, reportDto!, modelFilterFields) :
            CreateTableInitMethodByEntity(entity, fieldList, modelFilterFields);

        sb.Append($@"
@section Scripts {{
    <script>

        let PageTable;

        $(document).ready(function(){{
            InitilazeTable();
        }})

        function InitilazeTable(e) {{
             {datatableMethod}
        }}
    </script>
}}");



        return sb.ToString();
    }

    public string GenerateCreatePage(Entity entity)
    {
        List<Field> fieldList = _fieldRepository.GetAll(filter: f => f.EntityId == entity.Id, include: i => i.Include(x => x.FieldType));

        Dto? createDto = entity.CreateDtoId != default ? _dtoRepository.Get(f => f.Id == entity.CreateDtoId, include: i => i.Include(x => x.DtoFields).ThenInclude(x => x.SourceField)) : default;
        bool isThereCreateDto = createDto != default;
        
        List<ModelFieldInput> modelInputs = new List<ModelFieldInput>();
        if (isThereCreateDto)
        {
            modelInputs = CreateFormInputsModelByDto(entity, createDto!);
        }
        else
        {
            modelInputs = CreateFormInputsModelByEntity(entity, fieldList);
        }
            
        StringBuilder sb = new StringBuilder();

        sb.Append($@"
@using WebUI.Models.ViewModels.{entity.Name}_
@model {entity.Name}CreateViewModel
@{{
    ViewData[""Title""] = ""Create {entity.Name}"";
}}");

        // ########## HTML ##########
        sb.AppendLine("\r<div class=\"card\">");

        // ******** HEADER ********
        sb.AppendLine("\t<div class=\"card-header\">");
        sb.AppendLine($"\t\tCreate {entity.Name}");
        sb.AppendLine("\t</div>");


        // ******** BODY ********
        sb.AppendLine("\t<div class=\"card-body\">");

        sb.Append(CreateForm(entity, fieldList, modelInputs!, "Create", true, createDto));
        
        sb.AppendLine("\t</div>");
        sb.AppendLine("</div>");

        return sb.ToString();
    }

    public string GenerateCreateFormPage(Entity entity)
    {
        List<Field> fieldList = _fieldRepository.GetAll(filter: f => f.EntityId == entity.Id, include: i => i.Include(x => x.FieldType));

        Dto? createDto = entity.CreateDtoId != default ? _dtoRepository.Get(f => f.Id == entity.CreateDtoId, include: i => i.Include(x => x.DtoFields).ThenInclude(x => x.SourceField)) : default;
        bool isThereCreateDto = createDto != default;

        List<ModelFieldInput> modelInputs = new List<ModelFieldInput>();
        if (isThereCreateDto)
        {
            modelInputs = CreateFormInputsModelByDto(entity, createDto!);
        }
        else
        {
            modelInputs = CreateFormInputsModelByEntity(entity, fieldList);
        }

        StringBuilder sb = new StringBuilder();

        sb.Append($@"
@using WebUI.Models.ViewModels.{entity.Name}_
@model {entity.Name}CreateViewModel
@{{
    Layout = null;
}}"); 

        sb.Append(CreateForm(entity, fieldList, modelInputs!, "Create", false, createDto));
         
        return sb.ToString();
    }

    public string GenerateUpdateFormPage(Entity entity)
    {
        List<Field> fieldList = _fieldRepository.GetAll(filter: f => f.EntityId == entity.Id, include: i => i.Include(x => x.FieldType));

        Dto? updateDto = entity.UpdateDtoId != default ? _dtoRepository.Get(f => f.Id == entity.UpdateDtoId, include: i => i.Include(x => x.DtoFields).ThenInclude(x => x.SourceField)) : default;
        bool isThereUpdateDto = updateDto != default;

        List<ModelFieldInput> modelInputs = new List<ModelFieldInput>();
        if (isThereUpdateDto)
        {
            modelInputs = CreateFormInputsModelByDto(entity, updateDto!);
        }
        else
        {
            modelInputs = CreateFormInputsModelByEntity(entity, fieldList);
        }

        StringBuilder sb = new StringBuilder();

        sb.Append($@"
@using WebUI.Models.ViewModels.{entity.Name}_
@model {entity.Name}UpdateViewModel
@{{
    Layout = null;
}}");

        sb.Append(CreateForm(entity, fieldList, modelInputs!, "Update", false, updateDto));

        return sb.ToString();
    }



    #region Helpers

    #region Index Helpers
    // Html Helpers
    private string CreateFilterForm(Entity entity, List<ModelFieldInput> modelFilterFields)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var modelFilterField in modelFilterFields)
        {
            sb.Append(modelFilterField.InputCode);
        }

        if (entity.SoftDeletable)
        {
            sb.Append($@"                
                <div class=""col-md-4 col-sm-6 col-12"">
                    <div class=""input-group"">
                        <span class=""input-group-text bg-lightest"">
                            Include Deleted
                        </span>
                        <div class=""input-group-text"">
                            <input name=""IsDeleted"" type=""checkbox"" class=""form-check-input"" />
                        </div>
                    </div>
                </div>");
        }

        return $@"
        <form id=""form_page_filter"">
            <div class=""row row-gap-4"">
                {sb.ToString()}
                <div class=""col-md-3 col-sm-4 col-6"">
                    <button onclick=""InitilazeTable(this)"" type=""button"" class=""btn btn-light"">
                        <span class=""dynamic-content"">
                            <i class=""fa-solid fa-search me-2""></i>
                            Search
                        </span>
                    </button>
                </div>
            </div>
        </form>";
    }

    public string CreateTableByEntity(Entity entity, List<Field> fields)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var field in fields)
        {
            string labelText = field.Name.DivideToLabelName();
            sb.AppendLine($"<th>{labelText}</th>");
        }

        if (entity.Auditable)
        {
            sb.AppendLine("<th>Create Date</th>");
            sb.AppendLine("<th>Last Update Date</th>");
        }

        if (entity.SoftDeletable)
        {
            sb.AppendLine("<th>Status</th>");
            sb.AppendLine("<th>Delete Date</th>");
        }

        sb.AppendLine("<th>Actions</th>");

        return $@"
        <table id=""table_page"" class=""table table-hover table-striped"">
            <thead class=""bg-light"">
                <tr> 
                    {sb.ToString()}
                </tr>
            </thead>
        </table>";
    }

    public string CreateTableByDto(Entity entity, Dto reportDto)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var dtoField in reportDto.DtoFields)
        {
            string labelText = dtoField.Name.DivideToLabelName();
            sb.AppendLine($"<th>{labelText}</th>");
        }

        if (entity.Auditable)
        {
            sb.AppendLine("<th>Create Date</th>");
            sb.AppendLine("<th>Last Update Date</th>");
        }

        if (entity.SoftDeletable)
        {
            sb.AppendLine("<th>Status</th>");
            sb.AppendLine("<th>Delete Date</th>");
        }

        sb.AppendLine("<th>Actions</th>");


        return $@"
        <table id=""table_page"" class=""table table-hover table-striped"">
            <thead class=""bg-light"">
                <tr> 
                    {sb.ToString()}
                </tr>
            </thead>
        </table>";
    }


    // Script Helpers
    private string CreateTableInitMethodByEntity(Entity entity, List<Field> fieldList, List<ModelFieldInput> modelFilterFields)
    {
        StringBuilder sb = new StringBuilder();

        return $@"
            PageTable = DatatableManager.Create({{serverSide: true,
                tableId: 'table_page',
                path: '{entity.Name}/DatatableServerSide',
                method: 'Post',
                buttonElement: e,
                {DynamicFilterObject(entity, modelFilterFields)}
                {ColumnsObjectByEntity(entity, fieldList)}
                {CustomTableButtons(entity)}
            }})";
    }

    private string CreateTableInitMethodByDto(Entity entity, Dto reportDto, List<ModelFieldInput> modelFilterFields)
    {
        return $@"
            PageTable = DatatableManager.Create({{serverSide: true,
                tableId: 'table_page',
                path: '{entity.Name}/DatatableServerSide',
                method: 'Post',
                buttonElement: e,
                {DynamicFilterObject(entity, modelFilterFields)}
                {ColumnsObjectByDto(entity, reportDto)}
                {CustomTableButtons(entity)}
            }})";
    }


    private string DynamicFilterObject(Entity entity, List<ModelFieldInput> modelFilterFields)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var modelFilterField in modelFilterFields)
        {
            // input select
            if (modelFilterField.InputGroup == 1)
            {
                sb.AppendLine($@"
                {{
                    operator: 'eq',
                    field: ""{modelFilterField.InputName.ToCamelCase()}"",
                    value: $(""select[name='{modelFilterField.InputName}']"").val(),
                }},");
            }
            // input select
            else if (modelFilterField.InputGroup == 3)
            {
                sb.AppendLine($@"
                {{
                    operator: 'contains',
                    field: ""{modelFilterField.InputName.ToCamelCase()}"",
                    value: $(""input[name='{modelFilterField.InputName}']"").val(),
                }},");
            }
            else
            {
                sb.AppendLine($@"
                {{
                    operator: 'eq',
                    field: ""{modelFilterField.InputName.ToCamelCase()}"",
                    value: $(""input[name='{modelFilterField.InputName}']"").val(),
                }},");
            }
        }

        if (entity.SoftDeletable)
        {
            sb.AppendLine(@"
            {
                operator: 'eq',
                field: ""IsDeleted"",
                value: false,
                logic: 'or',
                filters: [
                    {
                        operator: 'eq',
                        field: ""IsDeleted"",
                        value: $(""input[name='IsDeleted']"").val(),
                    }
                ]
            }");
        }

        return $@"
                requestData: {{
                    filter: {{
                        operator: 'base',
                        logic: 'and',
                        filters: [
                             {sb.ToString()}
                        ]
                    }}
                }},";
    }

    private string ColumnsObjectByEntity(Entity entity, List<Field> fields)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var field in fields)
        {
            // date type
            if (field.GetVariableGroup() == 5)
            {
                sb.AppendLine($@"
                    {{
                        data: '{field.Name.ToCamelCase()}',
                        render: function (data) {{
                            if(data == null) return '';
                            return moment(data).format('DD.MM.YYYY HH:mm');
                        }}
                    }},");
            }
            // bool
            else if (field.GetVariableGroup() == 4)
            {
                sb.AppendLine($@"
                    {{
                        data: '{field.Name.ToCamelCase()}',
                        render: function (data) {{
                            if(data == true) return (`<span class=""badge rounded-pill bg-label-danger""><i class=""fa-solid fa-xmark""></i></span>`);
                            else if(data == false) return (`<span class=""badge rounded-pill bg-label-success""><i class=""fa-solid fa-check""></i></span>`);
                            else return ('');
                        }}
                    }},");
            }
            else
            {
                sb.AppendLine($@"{{data: '{field.Name.ToCamelCase()}' }},");
            }
        }

        if (entity.Auditable)
        {
            sb.AppendLine(@"
                    {
                        data: 'createDateUtc',
                        render: function (data) {
                            if(data == null) return '';
                            return moment(data).format('DD.MM.YYYY HH:mm');
                        }
                    },");

            sb.AppendLine(@"
                    {
                        data: 'updateDateUtc',
                        render: function (data) {
                            if(data == null) return '';
                            return moment(data).format('DD.MM.YYYY HH:mm');
                        }
                    },");
        }

        if (entity.SoftDeletable)
        {
            sb.AppendLine(@"
                    {
                        data: 'isDeleted',
                        render: function (data) {
                            if(data == true) return (`<span class=""badge rounded-pill bg-label-danger""><i class=""fa-solid fa-xmark""></i></span>`);
                            else if(data == false) return (`<span class=""badge rounded-pill bg-label-success""><i class=""fa-solid fa-check""></i></span>`);
                            else return ('');
                        }
                    },");

            sb.AppendLine(@"
                    {
                        data: 'deletedDateUtc',
                        render: function (data) {
                            if(data == null) return '';
                            return moment(data).format('DD.MM.YYYY HH:mm');
                        }
                    },");
        }

        sb.AppendLine($@"                    
                    {{
                        data: null,
                        defaultContent: '',
                        searchable: false,
                        createdCell: function (td, cellData, rowData, row, col)
                        {{ 
                            {RowButtons(entity)}
                        }}
                    }}");

        return $@"
                columns: [
                    {sb.ToString()}
                ],";
    }

    private string ColumnsObjectByDto(Entity entity, Dto reportDto)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var dtoField in reportDto.DtoFields)
        {
            int variableGroup = dtoField.SourceField.GetVariableGroup();

            // date type
            if (variableGroup == 5)
            {
                sb.AppendLine($@"
                    {{
                        data: '{dtoField.Name.ToCamelCase()}',
                        render: function (data) {{
                            if(data == null) return '';
                            return moment(data).format('DD.MM.YYYY HH:mm');
                        }}
                    }},");
            }
            // bool
            else if (variableGroup == 4)
            {
                sb.AppendLine($@"
                    {{
                        data: '{dtoField.Name.ToCamelCase()}',
                        render: function (data) {{
                            if(data == true) return (`<span class=""badge rounded-pill bg-label-danger""><i class=""fa-solid fa-xmark""></i></span>`);
                            else if(data == false) return (`<span class=""badge rounded-pill bg-label-success""><i class=""fa-solid fa-check""></i></span>`);
                            else return ('');
                        }}
                    }},");
            }
            else
            {
                sb.AppendLine($@"{{data: '{dtoField.SourceField.Name.ToCamelCase()}' }},");
            }
        }

        if (entity.Auditable)
        {
            sb.AppendLine(@"
                    {
                        data: 'createDateUtc',
                        render: function (data) {
                            if(data == null) return '';
                            return moment(data).format('DD.MM.YYYY HH:mm');
                        }
                    },");

            sb.AppendLine(@"
                    {
                        data: 'updateDateUtc',
                        render: function (data) {
                            if(data == null) return '';
                            return moment(data).format('DD.MM.YYYY HH:mm');
                        }
                    },");
        }

        if (entity.SoftDeletable)
        {
            sb.AppendLine(@"
                    {
                        data: 'isDeleted',
                        render: function (data) {
                            if(data == true) return (`<span class=""badge rounded-pill bg-label-danger""><i class=""fa-solid fa-xmark""></i></span>`);
                            else if(data == false) return (`<span class=""badge rounded-pill bg-label-success""><i class=""fa-solid fa-check""></i></span>`);
                            else return ('');
                        }
                    },");

            sb.AppendLine(@"
                    {
                        data: 'deletedDateUtc',
                        render: function (data) {
                            if(data == null) return '';
                            return moment(data).format('DD.MM.YYYY HH:mm');
                        }
                    },");
        }

        sb.AppendLine($@"                    
                    {{
                        data: null,
                        defaultContent: '',
                        searchable: false,
                        createdCell: function (td, cellData, rowData, row, col)
                        {{ 
                            {RowButtons(entity)}
                        }}
                    }}");

        return $@"
                columns: [
                    {sb.ToString()}
                ],";
    }



    private string RowButtons(Entity entity)
    {
        // id: rowData.id
        string uniqueFieldParams = string.Join(", ", entity.Fields.Where(f => f.IsUnique).Select(d => d.Name.ToCamelCase()));

        string entityLabelName = entity.Name.DivideToLabelName();
        return $@"
                            DatatableManager.AppendRowButtons(td,
                                [
                                    // 1) Update Button
                                    DatatableManager.RowButton({{
                                        kind: DatatableManager.buttonKinds.update,
                                        onClick: async (e_btn) =>
                                        {{
                                            await RequestManager.Get({{
                                                path: '{entity.Name}/UpdateForm',
                                                requestData: {{
                                                    {uniqueFieldParams}
                                                }},
                                                dataType: 'text',
                                                showToastrSuccess: false,
                                                buttonElement: e_btn.currentTarget,
                                                onSuccess: (formHtml) =>
                                                {{
                                                    ModalManager.CreateModal({{
                                                        title: ""Update {entityLabelName} Informations"",
                                                        innerHtml: formHtml,
                                                        modalSize: ""xl"",
                                                        buttons: [
                                                            ModalManager.Button({{
                                                                kind: ModalManager.buttonKinds.update,
                                                                onClick: (e_btn_modal, e_modal, e_form) =>
                                                                {{
                                                                    RequestManager.HandleRequest({{
                                                                        type: e_form.attr(""method""),
                                                                        path: e_form.attr(""action""),
                                                                        requestData: e_form.serializeArray(),
                                                                        buttonElement: e_btn_modal,
                                                                        onSuccess: () =>
                                                                        {{
                                                                            $(e_modal).modal(""hide"");
                                                                            PageTable.reload();
                                                                        }}
                                                                    }})
                                                                }}
                                                            }})
                                                        ]
                                                    }}).show();
                                                }}
                                            }})
                                        }}
                                    }}),
                                    // 2) Delete Button
                                    DatatableManager.RowButton({{
                                        kind: DatatableManager.buttonKinds.delete,
                                        onClick: async () =>
                                        {{
                                            ModalManager.DeleteModal({{
                                                onClick: (e_btn, e_mdl) =>
                                                {{
                                                    RequestManager.Delete({{
                                                        path: '{entity.Name}/Delete',
                                                        requestData: {{
                                                            // deleteModel destegi yok
                                                            {uniqueFieldParams}
                                                        }},
                                                        buttonElement: e_btn,
                                                        onSuccess: () =>
                                                        {{
                                                            $(e_mdl).modal(""hide"");
                                                            PageTable.reload();
                                                        }}
                                                    }})
                                                }}
                                            }}).show();
                                        }}
                                    }})
                                ]
                            );";
    }

    private string CustomTableButtons(Entity entity)
    {
        string entityLabelName = entity.Name.DivideToLabelName();

        return $@"
                customButtons:
                [
                    {{
                        text: '<span class=""dynamic-content""><i class=""fa-solid fa-file-circle-plus me-2""></i>Add New {entityLabelName}</span>',
                        className: 'create-new btn btn-primary mx-2',
                        action: (e_btn) =>
                        {{
                            RequestManager.Get({{
                                path: '{entity.Name}/CreateForm',
                                dataType: 'text',
                                showToastrSuccess: false,
                                buttonElement: e_btn.currentTarget,
                                onSuccess: (formHtml) =>
                                {{
                                    ModalManager.CreateModal({{
                                        title: ""Add New {entityLabelName}"",
                                        innerHtml: formHtml,
                                        modalSize: ""xl"",
                                        buttons: [
                                            ModalManager.Button({{
                                                kind: ModalManager.buttonKinds.save,
                                                onClick: (e_btn_modal, e_modal, e_form) =>
                                                {{
                                                    RequestManager.HandleRequest({{
                                                        type: e_form.attr(""method""),
                                                        path: e_form.attr(""action""),
                                                        requestData: e_form.serializeArray(),
                                                        buttonElement: e_btn_modal,
                                                        onSuccess: () =>
                                                        {{
                                                            $(e_modal).modal(""hide"");
                                                            PageTable.reload();
                                                        }}
                                                    }})
                                                }}
                                            }})
                                        ]
                                    }}).show();
                                }}
                            }})
                        }}
                    }}
                ]";
    } 

    // Helper UI Model
    private List<ModelFieldInput> CreateFilterInputsModel(Entity entity, List<Field> fields)
    {
        StringBuilder sb = new StringBuilder();

        Dictionary<string, string> selectableRelations = new Dictionary<string, string>();
        foreach (var field in fields)
        {
            Relation? relation = _relationRepository.Get(
              filter: f => f.ForeignFieldId == field.Id && f.RelationTypeId == (byte)RelationTypeEnums.OneToMany,
              include: i => i.Include(x => x.PrimaryField).ThenInclude(x => x.Entity));

            if (relation == null) continue;

            string selectPropName = field.Name.ToForeignFieldSlectListName(relation.PrimaryField.Entity.Name);
            selectableRelations.Add(field.Name, relation.PrimaryField.Entity.Name);
        }

        List<ModelFieldInput> result = new List<ModelFieldInput>();
        foreach (var field in fields)
        {
            ModelFieldInput modelFilterFieldInput = new ModelFieldInput();

            int variableGroupType = field.GetVariableGroup(selectableRelations);

            string labelText = field.Name.DivideToLabelName();
            string inputName = field.Name;

            modelFilterFieldInput.InputGroup = variableGroupType;
            modelFilterFieldInput.InputName = inputName;
            modelFilterFieldInput.InputName = inputName;

            // ### SelectList ###
            if (variableGroupType == 1)
            {
                var relation = selectableRelations.First(f => f.Key.Trim().ToLower() == field.Name.Trim().ToLower());
                string entityName = relation.Value;
                string listPropName = relation.Key.ToForeignFieldSlectListName(entityName);

                modelFilterFieldInput.InputCode = $@"
                <div class=""col-md-4 col-sm-6 col-12"">
                    <div class=""input-group d-flex flex-nowrap"">
                        <span class=""input-group-text bg-lightest"">
                            {labelText}
                        </span>
                        <select name=""{inputName}"" asp-items=""Model.{listPropName}"" class=""autoInitSelect2 form-select"">
                            <option></option>
                        </select>
                    </div>
                </div>";
            }
            // ### Number ###
            else if (variableGroupType == 2)
            {
                modelFilterFieldInput.InputCode = $@"
                <div class=""col-md-4 col-sm-6 col-12"">
                    <div class=""input-group"">
                        <span class=""input-group-text bg-lightest"">
                            {labelText}
                        </span>
                        <input name=""{inputName}"" type=""number"" class=""form-control"" />
                    </div>
                </div>";
            }
            // ### Text ###
            else if (variableGroupType == 3)
            {
                modelFilterFieldInput.InputCode = $@"
                <div class=""col-md-4 col-sm-6 col-12"">
                    <div class=""input-group"">
                        <span class=""input-group-text bg-lightest"">
                            {labelText}
                        </span>
                        <input name=""{inputName}"" type=""text"" class=""form-control"" />
                    </div>
                </div>";
            }
            // ### Date ###
            else if (variableGroupType == 4)
            {
                modelFilterFieldInput.InputCode = $@"
                <div class=""col-md-4 col-sm-6 col-12"">
                    <div class=""input-group"">
                        <span class=""input-group-text bg-lightest"">
                            {labelText}
                        </span>
                        <input name=""{inputName}"" type=""text"" class=""autoInitDatePicker form-control"" />
                    </div>
                </div>";
            }
            // ### CheckBox ###
            else if (variableGroupType == 5)
            {
                modelFilterFieldInput.InputCode = $@"                
                <div class=""col-md-4 col-sm-6 col-12"">
                    <div class=""input-group"">
                        <span class=""input-group-text bg-lightest"">
                            {labelText}
                        </span>
                        <div class=""input-group-text"">
                            <input name=""{inputName}"" type=""checkbox"" class=""form-check-input"" />
                        </div>
                    </div>
                </div>";
            }
        }

        return result;
    }
    #endregion



    private List<ModelFieldInput> CreateFormInputsModelByEntity(Entity entity, List<Field> fields)
    {
        StringBuilder sb = new StringBuilder();

        Dictionary<string, string> selectableRelations = new Dictionary<string, string>();
        foreach (var field in fields)
        {
            Relation? relation = _relationRepository.Get(
              filter: f => f.ForeignFieldId == field.Id && f.RelationTypeId == (byte)RelationTypeEnums.OneToMany,
              include: i => i.Include(x => x.PrimaryField).ThenInclude(x => x.Entity));

            if (relation == null) continue;

            string selectPropName = field.Name.ToForeignFieldSlectListName(relation.PrimaryField.Entity.Name);
            selectableRelations.Add(field.Name, relation.PrimaryField.Entity.Name);
        }

        List<ModelFieldInput> result = new List<ModelFieldInput>();
        foreach (var field in fields)
        {
            ModelFieldInput modelFilterFieldInput = new ModelFieldInput();

            int variableGroupType = field.GetVariableGroup(selectableRelations);

            string labelText = field.Name.DivideToLabelName();
            string inputName = field.Name;

            modelFilterFieldInput.InputGroup = variableGroupType;
            modelFilterFieldInput.InputName = inputName;
            modelFilterFieldInput.InputName = inputName;

            // ### SelectList ###
            if (variableGroupType == 1)
            {
                var relation = selectableRelations.First(f => f.Key.Trim().ToLower() == field.Name.Trim().ToLower());
                string entityName = relation.Value;
                string listPropName = relation.Key.ToForeignFieldSlectListName(entityName);

                modelFilterFieldInput.InputCode = $@"
                <div class=""col-sm-6"">
                    <label asp-for=""CreateModel.{inputName}"" class=""form-label"">
                        {labelText}
                    </label>
                    <select asp-for=""CreateModel.{inputName}"" asp-items=""Model.{listPropName}"" class=""autoInitSelect2 form-select"">
                        <option></option>
                    </select>
                    <span asp-validation-for=""CreateModel.{inputName}"" class=""form_validation_feedback""></span>
                </div>";
            }
            // ### Number ###
            else if (variableGroupType == 2)
            {
                modelFilterFieldInput.InputCode = $@"
                <div class=""col-sm-6"">
                    <label asp-for=""CreateModel.{inputName}"" class=""form-label"">
                        {labelText}
                    </label>
                    <input asp-for=""CreateModel.{inputName}"" type=""number"" class=""form-control"" />
                    <span asp-validation-for=""CreateModel.{inputName}"" class=""form_validation_feedback""></span>
                </div>";
            }
            // ### Text ###
            else if (variableGroupType == 3)
            {
                modelFilterFieldInput.InputCode = $@"
                <div class=""col-sm-6"">
                    <label asp-for=""CreateModel.{inputName}"" class=""form-label"">
                        {labelText}
                    </label>
                    <input asp-for=""CreateModel.{inputName}"" type=""text"" class=""form-control"" />
                    <span asp-validation-for=""CreateModel.{inputName}"" class=""form_validation_feedback""></span>
                </div>";
            }
            // ### Date ###
            else if (variableGroupType == 4)
            {
                modelFilterFieldInput.InputCode = $@"
                <div class=""col-sm-6"">
                    <label asp-for=""CreateModel.{inputName}"" class=""form-label"">
                        {labelText}
                    </label>
                    <input asp-for=""CreateModel.{inputName}"" type=""text"" class=""autoInitDatePicker form-control"" />
                    <span asp-validation-for=""CreateModel.{inputName}"" class=""form_validation_feedback""></span>
                </div>";
            }
            // ### CheckBox ###
            else if (variableGroupType == 5)
            {
                modelFilterFieldInput.InputCode = $@"                
                <div class=""col-sm-6"">
                    <div class=""form-check"">
                        <label asp-for=""CreateModel.{inputName}"" class=""form-check-label"">
                            {labelText}
                        </label>
                        <input asp-for=""CreateModel.{inputName}"" type=""checkbox"" class=""form-check-input"" />
                        <span asp-validation-for=""CreateModel.{inputName}"" class=""form_validation_feedback""></span>
                    </div>
                </div>";
            }
        }

        return result;
    }
    private List<ModelFieldInput> CreateFormInputsModelByDto(Entity entity, Dto dto)
    {
        StringBuilder sb = new StringBuilder();

        Dictionary<string, string> selectableRelations = new Dictionary<string, string>();
        foreach (var dtoField in dto.DtoFields)
        {
            Relation? relation = _relationRepository.Get(
              filter: f => f.ForeignFieldId == dtoField.SourceField.Id && f.RelationTypeId == (byte)RelationTypeEnums.OneToMany,
              include: i => i.Include(x => x.PrimaryField).ThenInclude(x => x.Entity));

            if (relation == null) continue;

            string selectPropName = dtoField.Name.ToForeignFieldSlectListName(relation.PrimaryField.Entity.Name);
            selectableRelations.Add(dtoField.Name, relation.PrimaryField.Entity.Name);
        }

        List<ModelFieldInput> result = new List<ModelFieldInput>();
        foreach (var dtoField in dto.DtoFields)
        {
            ModelFieldInput modelFilterFieldInput = new ModelFieldInput();

            int variableGroupType = selectableRelations.Any(f => f.Key == dtoField.Name) ? 1 : dtoField.SourceField.GetVariableGroup(selectableRelations);

            string labelText = dtoField.SourceField.Name.DivideToLabelName();
            string inputName = dtoField.SourceField.Name;

            modelFilterFieldInput.InputGroup = variableGroupType;
            modelFilterFieldInput.InputName = inputName;
            modelFilterFieldInput.InputName = inputName;

            // ### SelectList ###
            if (variableGroupType == 1)
            {
                var relation = selectableRelations.First(f => f.Key.Trim().ToLower() == dtoField.SourceField.Name.Trim().ToLower());
                string entityName = relation.Value;
                string listPropName = relation.Key.ToForeignFieldSlectListName(entityName);

                modelFilterFieldInput.InputCode = $@"
                <div class=""col-sm-6"">
                    <label asp-for=""CreateModel.{inputName}"" class=""form-label"">
                        {labelText}
                    </label>
                    <select asp-for=""CreateModel.{inputName}"" asp-items=""Model.{listPropName}"" class=""autoInitSelect2 form-select"">
                        <option></option>
                    </select>
                    <span asp-validation-for=""CreateModel.{inputName}"" class=""form_validation_feedback""></span>
                </div>";
            }
            // ### Number ###
            else if (variableGroupType == 2)
            {
                modelFilterFieldInput.InputCode = $@"
                <div class=""col-sm-6"">
                    <label asp-for=""CreateModel.{inputName}"" class=""form-label"">
                        {labelText}
                    </label>
                    <input asp-for=""CreateModel.{inputName}"" type=""number"" class=""form-control"" />
                    <span asp-validation-for=""CreateModel.{inputName}"" class=""form_validation_feedback""></span>
                </div>";
            }
            // ### Text ###
            else if (variableGroupType == 3)
            {
                modelFilterFieldInput.InputCode = $@"
                <div class=""col-sm-6"">
                    <label asp-for=""CreateModel.{inputName}"" class=""form-label"">
                        {labelText}
                    </label>
                    <input asp-for=""CreateModel.{inputName}"" type=""text"" class=""form-control"" />
                    <span asp-validation-for=""CreateModel.{inputName}"" class=""form_validation_feedback""></span>
                </div>";
            }
            // ### Date ###
            else if (variableGroupType == 4)
            {
                modelFilterFieldInput.InputCode = $@"
                <div class=""col-sm-6"">
                    <label asp-for=""CreateModel.{inputName}"" class=""form-label"">
                        {labelText}
                    </label>
                    <input asp-for=""CreateModel.{inputName}"" type=""text"" class=""autoInitDatePicker form-control"" />
                    <span asp-validation-for=""CreateModel.{inputName}"" class=""form_validation_feedback""></span>
                </div>";
            }
            // ### CheckBox ###
            else if (variableGroupType == 5)
            {
                modelFilterFieldInput.InputCode = $@"                
                <div class=""col-sm-6"">
                    <div class=""form-check"">
                        <label asp-for=""CreateModel.{inputName}"" class=""form-check-label"">
                            {labelText}
                        </label>
                        <input asp-for=""CreateModel.{inputName}"" type=""checkbox"" class=""form-check-input"" />
                        <span asp-validation-for=""CreateModel.{inputName}"" class=""form_validation_feedback""></span>
                    </div>
                </div>";
            }
        }

        return result;
    }


    private string CreateForm(Entity entity, List<Field> fields, List<ModelFieldInput> modelFieldInputs, string action, bool submitBtn, Dto? dto = default)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var modelFilterField in modelFieldInputs)
        {
            sb.Append(modelFilterField.InputCode);
        }

        string button = submitBtn ? @"
                <div class=""col-12"">
					<button type=""submit"" class=""btn btn-success"">
						<i class=""fa-solid fa-floppy-disk me-2""></i>
						Save
					</button>
				</div>" : string.Empty;

        string uniqueFieldInputs = string.Join("\n", fields.Where(f => f.IsUnique).Select(d =>
        {
            if (dto != default)
            {
                DtoField? dtoField = dto.DtoFields.FirstOrDefault(f => f.SourceFieldId == d.Id);
                if (dtoField != default) 
                    return @$"<input asp-for=""UpdateModel.{dtoField.Name}"" type=""hidden"" class=""form-control"" />";
            }
            return @$"<input asp-for=""UpdateModel.{d.Name}"" type=""hidden"" class=""form-control"" />";
        }).ToList());

        return $@"
		<form asp-controller=""{entity.Name}"" asp-action=""{action}"" method=""post"">
            <div class=""row row-gap-4 m-2"">
                {sb.ToString()}
                {button}
            </div>
        </form>";
    }



    private class ModelFieldInput
    {
        public int InputGroup { get; set; }
        public string InputName { get; set; } = null!;
        public string LabelText { get; set; } = null!;
        public string InputCode { get; set; } = null!;
    }
    #endregion
}
