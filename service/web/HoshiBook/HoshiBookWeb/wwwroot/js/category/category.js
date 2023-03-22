var dataTable

$(document).ready(function () {
    initialize()
    loadDatatable()
})

function changeMainFeatureBtnStatus(isShow)
{
    if (isShow)
    {
        $('#singleCreateBtn').show()
        $('#bulkCreateBtn').show()
        $('#exportDetailsBtn').show()
    }
    else
    {
        $('#singleCreateBtn').hide()
        $('#bulkCreateBtn').hide()
        $('#exportDetailsBtn').hide()
    }
}

function changeFileUploadFeatureBtnStatus(isShow)
{
    if (isShow)
    {
        $('#fileUpload').show()
        $('#sendFileBtn').show()
        $('#revertFileBtn').show()
    }
    else
    {
        $('#fileUpload').hide()
        $('#sendFileBtn').hide()
        $('#revertFileBtn').hide()
    }
}

function handleResetFileUpload() {
    $('#fileUpload').val('')
}

function initialize() {
    changeMainFeatureBtnStatus(true);
    changeFileUploadFeatureBtnStatus(false);
}

function loadDatatable() {
    dataTable = $('#tblData').DataTable({
        ajax: {
            url: '/Admin/Category/GetAll',
        },
        columns: [
            { data: 'id', width: '10%' },
            { data: 'name', width: '15%' },
            { data: 'displayOrder', width: '10%' },
            {
                data: 'id',
                render: function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/Admin/Category/Edit/${data}"
                            class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i> Edit
                            </a>
                            <a onclick=Delete('/Admin/Category/Delete/${data}')
                            class="btn btn-danger mx-2">
                                <i class="bi bi-trash"></i> Delete
                            </a>
                        </div>
                    `
                },
                width: '15%',
            },
        ],
    })
}

function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!',
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload()
                        toastr.success(data.message)
                    } else {
                        toastr.error(data.message)
                    }
                },
            })
        }
    })
}

function handleBulkCreateClick() {
    changeMainFeatureBtnStatus(false)
    changeFileUploadFeatureBtnStatus(true)
    handleResetFileUpload();
}

function handleRevertFileClick() {
    changeMainFeatureBtnStatus(true)
    changeFileUploadFeatureBtnStatus(false)
}

function handleResetFile() {
    handleRevertFileClick()
    handleResetFileUpload()
}

function handleSendFileClick() {
    var getUploadFile = document.getElementById('fileUpload')

    if (getUploadFile.files.length === 0) {
        showMessage('Upload File', 'Please select a file to upload')
        return false
    }

    var form = $('#categoryListImportForm')[0]
    var data = new FormData(form)
    $.ajax({
        url: '/Admin/Category/BulkCreate',
        type: 'POST',
        data: data,
        contentType: false,
        processData: false,
        success: function (data) {
            if (data.success) {
                handleResetFile()
                dataTable.ajax.reload()
                toastr.success(data.message)
            } else {
                toastr.error(data.message)
            }
        },
    })
}